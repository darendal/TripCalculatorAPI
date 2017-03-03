using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class ExpenseLineItem
    {
        public string Name { get; set; }

        public double ExpenseAmount { get; set; }
    }
}