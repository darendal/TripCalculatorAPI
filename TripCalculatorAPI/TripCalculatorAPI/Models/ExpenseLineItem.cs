using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class ExpenseLineItem
    {
        public string Name { get; set; }

        public decimal ExpenseAmount { get; set; }

        /// <summary>
        /// Check if this line items expense amount is within a cent of the given value
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool EqualWithinOneCent(decimal amount)
        {
            return (ExpenseAmount >= amount - 0.01M) && (ExpenseAmount <= amount + 0.01M);
        }
        
    }
}