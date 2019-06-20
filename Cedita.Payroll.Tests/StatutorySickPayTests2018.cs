﻿using Cedita.Payroll.Models.Statutory.Assessments;
using Cedita.Payroll.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cedita.Payroll.Tests
{
    [TestClass]
    public class StatutorySickPayTests2018 : StatutoryTests
    {

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void CanLoadEmbeddedBankHolidayProvider()
        {
            var bankHolidayConfigData = BankHolidayDataProvider.GetBankHolidayConfigurationData();
            Assert.IsTrue(bankHolidayConfigData.BankHolidays.Count() > 0, "Bank Holiday Configuration Data Could not be loaded");

            var bankHolidaysThisYear = BankHolidayDataProvider.GetBankHolidays(new DateTime(2019, 01, 01));
            Assert.IsTrue(bankHolidaysThisYear.Count() > 0, "Bank Holiday Configuration Data for 2019 could not be found");

        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOneWeekSickPayClaimWithWaitingDays()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 02, 18))
                .WithEndDate(new DateTime(2019, 02, 22))
                .WithNextPaymentDate(new DateTime(2019, 02, 22))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(true)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var statutoryPayments = GetSspCalculation(2018, sickPayAssessment).Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(2, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(36.82m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(1, statutoryPayments.Count(), "Unexpected total collection of payments");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOneWeekSickPayClaimWithNoWaitingDays()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 02, 18))
                .WithEndDate(new DateTime(2019, 02, 22))
                .WithNextPaymentDate(new DateTime(2019, 02, 22))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var statutoryPayments = GetSspCalculation(2018, sickPayAssessment).Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(5, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(92.05m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(1, statutoryPayments.Count(), "Unexpected total collection of payments");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidTwoWeekSickPayClaimWithNoWaitingDaysOverMultiplePeriods()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 02, 18))
                .WithEndDate(new DateTime(2019, 03, 02))
                .WithNextPaymentDate(new DateTime(2019, 02, 22))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();
              
            var statutoryPayments = GetSspCalculation(2018, sickPayAssessment).Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(10, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(184.10m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(2, statutoryPayments.Count(), "Unexpected total collection of payments");
            Assert.AreEqual(new DateTime(2019,03,01), statutoryPayments.First().PaymentDate, "Unexpected payment date for first payment collection");
            Assert.AreEqual(new DateTime(2019,03,08), statutoryPayments.Skip(1).Single().PaymentDate, "Unexpected payment date for second payment collection");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOneWeekSickPayClaimWithBankHolidaysUnpaid()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 17))
                .WithEndDate(new DateTime(2019, 04, 24))
                .WithActiveContract(true)
                .WithNextPaymentDate(new DateTime(2019, 04, 19))
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var statutoryAssessment = GetSspCalculation(2018, sickPayAssessment);
            var statutoryPayments = statutoryAssessment.Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(4, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(73.64m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(2, statutoryPayments.Count(), "Unexpected total collection of payments");
            Assert.AreEqual(new DateTime(2019, 04, 26), statutoryPayments.First().PaymentDate, "Unexpected payment date for first payment collection");
            Assert.AreEqual(new DateTime(2019, 05, 03), statutoryPayments.Skip(1).Single().PaymentDate, "Unexpected payment date for second payment collection");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOverlappingSickPayClaimWithoutBankHolidays()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 17))
                .WithEndDate(new DateTime(2019, 04, 24))
                .WithNextPaymentDate(new DateTime(2019, 04, 19))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var overlappingSickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 22))
                .WithEndDate(new DateTime(2019, 04, 26))
                .WithNextPaymentDate(new DateTime(2019, 04, 26))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var sspEngine = statutoryFactory.CreateSspCalculationEngine(2018);
            var calculation = sspEngine.Calculate(overlappingSickPayAssessment, new List<SickPayAssessment> { sickPayAssessment });
            var statutoryPayments = calculation.Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(2, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(36.82m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(1, statutoryPayments.Count(), "Unexpected total collection of payments");
            Assert.AreEqual(new DateTime(2019, 05, 03), statutoryPayments.First().PaymentDate, "Unexpected payment date for first payment collection");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOverlappingSickPayClaimWithBankHolidays()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 17))
                .WithEndDate(new DateTime(2019, 04, 24))
                .WithNextPaymentDate(new DateTime(2019, 04, 19))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var overlappingSickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 22))
                .WithEndDate(new DateTime(2019, 04, 26))
                .WithNextPaymentDate(new DateTime(2019, 04, 26))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(true)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var sspEngine = statutoryFactory.CreateSspCalculationEngine(2018);
            var calculation = sspEngine.Calculate(overlappingSickPayAssessment, new List<SickPayAssessment> { sickPayAssessment });
            var statutoryPayments = calculation.Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(3, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(55.23m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(1, statutoryPayments.Count(), "Unexpected total collection of payments");
            Assert.AreEqual(new DateTime(2019, 05, 03), statutoryPayments.First().PaymentDate, "Unexpected payment date for first payment collection");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOverlappingSickPayClaimWith3HistoricalClaims()
        {
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 17))
                .WithEndDate(new DateTime(2019, 04, 24))
                .WithNextPaymentDate(new DateTime(2019, 04, 19))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var overlappingSickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 22))
                .WithEndDate(new DateTime(2019, 04, 26))
                .WithNextPaymentDate(new DateTime(2019, 04, 26))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var finalSickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 04, 15))
                .WithEndDate(new DateTime(2019, 05, 10))
                .WithNextPaymentDate(new DateTime(2019, 04, 19))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(false)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();   

            var sspEngine = statutoryFactory.CreateSspCalculationEngine(2018);
            var calculation = sspEngine.Calculate(finalSickPayAssessment, new List<SickPayAssessment> { sickPayAssessment, overlappingSickPayAssessment });
            var statutoryPayments = calculation.Payments;

            // Calculate 2 days sick pay
            Assert.AreEqual(11, statutoryPayments.Sum(m => m.Qty), "Unexpected total days sick pay");
            Assert.AreEqual(202.51m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(3, statutoryPayments.Count(), "Unexpected total collection of payments");
            Assert.AreEqual(new DateTime(2019, 04, 26), statutoryPayments.First().PaymentDate, "Unexpected payment date for first payment collection");
        }


        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidOneWeekSickPayClaimWithWaitingDaysAndUnpaidBankHolidays()
        {
            /*•	Please can the logic be applied that if one of the waiting days falls on a bank holiday when we say NO to pay bank holidays that the next working day is used to 
             * apply the waiting day, EG: sick note for 02/05-16/05. 3 waiting days = 02/05, 03/05 and 07/05 as 06/05 is a bank holiday. */
            // Week long sick note, this is the first sick note they have claimed
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 05, 02))
                .WithEndDate(new DateTime(2019, 05, 16))
                .WithNextPaymentDate(new DateTime(2019, 05, 03))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(true)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .GetAssessment();

            var statutoryPayments = GetSspCalculation(2018, sickPayAssessment).Payments;

            // In this instance, the 2nd, 3rd and 7th are the 3 waiting days, with the 6th being a bank holiday
            // The first payment group should only be a sum of 3 days
            Assert.AreEqual(3, statutoryPayments.First().Qty, "Unexpected total days sick in first week");
            Assert.AreEqual(7 * 18.41m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(2, statutoryPayments.Count(), "Unexpected total collection of payments");
        }

        [TestCategory("Statutory Sick Pay Tests"), TestMethod]
        public void ValidSspClaimWith140DayCap()
        {
            var sickPayAssessment = (new MockSickPayAssessment())
                .WithStartDate(new DateTime(2019, 05, 02))
                .WithEndDate(new DateTime(2019, 05, 16))
                .WithNextPaymentDate(new DateTime(2019, 05, 03))
                .WithActiveContract(true)
                .WithBankHolidaysPaid(false)
                .WithIsFirstSicknote(true)
                .WithIsFitForWork(false)
                .WithTotalEarningsInPeriod(2000m)
                .WithTotalPaymentsInPeriod(8)
                .WithHistoricalSickDayTotal(139)
                .GetAssessment();

            var statutoryPayments = GetSspCalculation(2018, sickPayAssessment).Payments;

            // In this instance, the 2nd, 3rd and 7th are the 3 waiting days, with the 6th being a bank holiday
            // The first payment group should only be a sum of 3 days
            Assert.AreEqual(1, statutoryPayments.First().Qty, "Unexpected total days sick in first week");
            Assert.AreEqual(1 * 18.41m, statutoryPayments.Sum(m => m.Qty * m.Cost), "Unexpected amount of sick pay");
            Assert.AreEqual(1, statutoryPayments.Count(), "Unexpected total collection of payments");

        }
    }
}
