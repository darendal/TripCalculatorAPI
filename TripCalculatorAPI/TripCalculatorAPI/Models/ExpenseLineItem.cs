using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class ExpenseLineItem
    {
        public string Name { get; set; }

        private decimal _expenseAmount = 0;
        public decimal ExpenseAmount
        {
            get
            {
                return Math.Round(_expenseAmount, 2);
            }
            set
            {
                _expenseAmount = value;
            }
        }

        public bool EqualWithinOneCent(decimal amount)
        {
            return (ExpenseAmount >= amount - 0.01M) && (ExpenseAmount <= amount + 0.01M);
        }
        
    }
}