using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TripCalculatorAPI.Models;

namespace TripCalculatorAPI.Controllers
{
    public class TripCalculatorController : ApiController
    {

        public ExpenseRepaymentCollection Post(IEnumerable<ExpenseLineItem> expenses)
        {
            ExpenseRepaymentCollection result = new ExpenseRepaymentCollection();


            return result;
        }

    }
}
