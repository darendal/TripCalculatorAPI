using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class ExpenseRepaymentCollection
    {
        public ExpenseRepayment[] Repayments { get; set; }

        /// <summary>
        /// 1: Valid data, Repayments > 0 
        /// 0: Valid data, no repayments present
        /// </summary>
        public byte Status { get; set; }
    }
}