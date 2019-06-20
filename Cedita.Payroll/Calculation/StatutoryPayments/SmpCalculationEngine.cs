﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cedita.Payroll.Abstractions;
using Cedita.Payroll.Configuration;
using Cedita.Payroll.Models.Statutory;
using Cedita.Payroll.Models.Statutory.Assessments;

namespace Cedita.Payroll.Calculation.StatutoryPayments
{
    [CalculationEngineTaxYear(TaxYear = 2018)]
    [CalculationEngineTaxYear(TaxYear = 2019)]
    public class SmpCalculationEngine : StatutoryCalculationEngine, IStatutoryMaternityPayCalculationEngine
    {
        public SmpCalculationEngine(TaxYearConfigurationData taxYearConfigurationData, BankHolidayConfigurationData bankHolidayConfigurationData) : base(taxYearConfigurationData, bankHolidayConfigurationData) {}

        public StatutoryCalculationResult<MaternityPayAssessment> Calculate(MaternityPayAssessment model)
        {
            var assessmentCalculation = new StatutoryCalculationResult<MaternityPayAssessment>();
            if (!model.UpcomingPaymentDate.HasValue)
                assessmentCalculation.AddError(StatutoryValidationError.MissingRequiredValue, "The next Upcoming Payment Date must be provided");
            if (!model.StartDate.HasValue)
                assessmentCalculation.AddError(StatutoryValidationError.MissingRequiredValue, "The Start Date must be provided");
            if (!model.DueDate.HasValue)
                assessmentCalculation.AddError(StatutoryValidationError.MissingRequiredValue, "The Due Date must be provided");
            if (model.UpcomingPaymentDate.Value < model.StartDate.Value)
                assessmentCalculation.AddError(StatutoryValidationError.InvalidUpcomingPayDate, "The upcoming process date cannot be before the Start Date");

            if (assessmentCalculation.Errors.Any())
                return assessmentCalculation;

            assessmentCalculation.IsEligible = model.IsEligible;
            if (!model.StartDate.HasValue)
                model.StartDate = model.DueDate;

            // Statutory Maternity Pay ends after 39 weeks 
            if (!model.EndDate.HasValue)
                model.EndDate = model.StartDate.Value.AddDays((7 * 39) - 1);

            var scheduledPayments = new List<StatutoryPayment>();

            var datesInRange = model.GetQualifyingDatesInRange();
            var nextPaymentDate = model.UpcomingPaymentDateForPeriod;

            var statPayment = new StatutoryPayment
            {
                ReferenceDate = nextPaymentDate,
                PaymentDate = nextPaymentDate.AddDays(7),
                Cost = taxYearConfigurationData.StatutoryMaternityPayDayRate,
                Qty = 0m
            };

            int totalDaysClaimed = 0;
            foreach (var claimDate in datesInRange)
            {
                totalDaysClaimed++;

                // First 6 weeks we claim the average rate
                if (totalDaysClaimed <= (7 * 6))
                {
                    statPayment.Cost = (model.AverageWeeklyEarnings * 0.9m) / 7;
                    statPayment.IsStatutoryMinimumRate = false;
                }

                if (claimDate > nextPaymentDate)
                {
                    scheduledPayments.Add(statPayment);

                    // Next payment is one week away, Fort/Monthly change
                    nextPaymentDate = nextPaymentDate.AddDays(7);

                    statPayment = new StatutoryPayment
                    {
                        ReferenceDate = nextPaymentDate,
                        PaymentDate = nextPaymentDate.AddDays(7),
                        Cost = taxYearConfigurationData.StatutoryMaternityPayDayRate,
                        Qty = 0m,
                    };
                }

                // If this is the first day of our statutory minimum, create a new holder at the reduced rate
                if (totalDaysClaimed == (7 * 6) + 1)
                {
                    scheduledPayments.Add(statPayment);

                    statPayment = new StatutoryPayment
                    {
                        ReferenceDate = statPayment.ReferenceDate,
                        PaymentDate = statPayment.PaymentDate,
                        Cost = taxYearConfigurationData.StatutoryMaternityPayDayRate,
                        Qty = 0m,
                    };
                }

                // We do want to pay this date
                statPayment.Qty += 1m;
            }

            // Add the last period
            scheduledPayments.Add(statPayment);

            // Filter out empty schedules
            assessmentCalculation.Payments = scheduledPayments.Where(m => m.Qty > 0).Select(m => m);

            return assessmentCalculation;
        }
    }
}
