using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class ExpenseRepaymentCollection
    {
        public ExpenseRepayment[] Repayments { get; set; }

        public byte status { get; set; }
    }
}