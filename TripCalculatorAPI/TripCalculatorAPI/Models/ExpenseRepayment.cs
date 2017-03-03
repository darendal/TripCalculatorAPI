using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    /// <summary>
    /// Repayment to be made line item.
    /// </summary>
    public class ExpenseRepayment
    {
        /// <summary>
        /// User who is receiving money
        /// </summary>
        public string PayTo { get; set; }

        /// <summary>
        /// User who is paying money
        /// </summary>
        public string PayFrom { get; set; }

        /// <summary>
        /// Money being paid, rounded to nearest cent
        /// </summary>
        public double Amout { get; set; }
    }
}