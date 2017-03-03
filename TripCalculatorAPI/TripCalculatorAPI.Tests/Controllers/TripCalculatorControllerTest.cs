using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using TripCalculatorAPI.Controllers;
using System.Web.Http.Results;
using TripCalculatorAPI.Models;
using System.Collections.Generic;

namespace TripCalculatorAPI.Tests.Controllers
{
    [TestClass]
    public class TripCalculatorControllerTest
    {
        private TripCalculatorController tripController; 

        [TestInitialize]
        public void TestInitialize()
        {
            tripController = new TripCalculatorController();

            

            List<ExpenseLineItem> valid = new List<ExpenseLineItem>();
        }

        [TestMethod]
        public void TestNullValuesPassed()
        {
            IHttpActionResult response =  tripController.Get(null,null);

            var contentResult = response as BadRequestErrorMessageResult;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Message);
            Assert.AreNotEqual(string.Empty, contentResult.Message);
        }

        [TestMethod]
        public void TestInvalidExpenseValuesPassed()
        {
            List<ExpenseLineItem> invalid = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "A", ExpenseAmount = -2.00 },
                new ExpenseLineItem() {Name = "B", ExpenseAmount = 10.00 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 10.00 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = -10.00 },
                new ExpenseLineItem() {Name = "E", ExpenseAmount = 10.00 },
                new ExpenseLineItem() {Name = "F", ExpenseAmount = 10.00 },
            };

            IHttpActionResult response = tripController.Get(invalid, new List<string> { "A", "B", "C", "D", "E", "F" });

            var contentResult = response as BadRequestErrorMessageResult;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Message);
            Assert.AreNotEqual(string.Empty, contentResult.Message);

        }
    }
}
