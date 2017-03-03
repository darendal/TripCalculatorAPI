using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TripCalculatorAPI.Models
{
    public class User
    {
        public string Name { get; set; }

        public decimal[] Expenses { get; set; }
    }
}