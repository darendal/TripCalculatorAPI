using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using TripCalculatorAPI.Controllers;
using System.Web.Http.Results;

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
        }

        [TestMethod]
        public void TestNullValuesPassed()
        {
            IHttpActionResult response =  tripController.Get(null);

            var contentResult = response as BadRequestErrorMessageResult;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Message);
            Assert.AreNotEqual(string.Empty, contentResult.Message);
        }
    }
}
