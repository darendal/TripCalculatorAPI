using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class User
    {
        public User()
        {
            //Avoid null errors when given an empty list of expenses by defaulting to an empty array
            Expenses = new decimal[0];
        }

        public string Name { get; set; }

        public decimal[] Expenses { get; set; }
    }
}