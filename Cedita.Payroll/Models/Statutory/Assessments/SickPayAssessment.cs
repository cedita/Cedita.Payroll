﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cedita.Payroll.Models.Statutory.Assessments
{
    public class SickPayAssessment : BaseStatutoryAssessment
    {
        public bool IncludeBankHolidays { get; set; }
        public bool FirstSickNote { get; set; }

    }
}