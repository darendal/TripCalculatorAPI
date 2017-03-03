using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using TripCalculatorAPI.Controllers;
using System.Web.Http.Results;
using TripCalculatorAPI.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Data;
using System.Linq;

namespace TripCalculatorAPI.Tests.Controllers
{
    [TestClass]
    public class TripCalculatorControllerTest
    {
        private TripCalculatorController tripController;
        private HttpServer server;

        private const string API_Address = "http://localhost/api/TripCalculator";

        private HttpResponseMessage message;

        [TestInitialize]
        public void TestInitialize()
        {
            message = null;

            tripController = new TripCalculatorController();
            tripController.Request = new System.Net.Http.HttpRequestMessage();
            tripController.Configuration = new HttpConfiguration();

            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "api/{controller}/{id}",
                 defaults: new { id = RouteParameter.Optional } // optional id
             );

            server = new HttpServer(config);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(message != null)
            {
                message.Dispose();
            }
        }

        private HttpResponseMessage GetHttpResponse( string content, HttpMethod method)
        {
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, API_Address))
                {
                    request.Content = new StringContent(content);
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    return client.SendAsync(request, CancellationToken.None).Result;
                }
            }
        }

        /// <summary>
        /// Test that passing a null list returns a 400 bad request error.
        /// Checks that the error returned also includes a detailed message
        /// </summary>
        [TestMethod]
        public void TestNullValuesPassed()
        {
            message = GetHttpResponse(string.Empty, HttpMethod.Post);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, message.StatusCode);

            dynamic json = message.Content.ReadAsAsync<dynamic>().Result;
            string errorText = json["Message"];

            Assert.AreNotEqual(string.Empty, errorText);
            
        }

        [TestMethod]
        public void TestInvalidExpenseValuesPassed()
        {
            List<User> invalid = new List<User>()
            {
                new User() { Name = "L", Expenses = new decimal[]{ -5.75M, 35M, 12.79M} },
                new User() { Name = "C", Expenses = new decimal[] { 12.00M, 15.00M, 23.23M } },
                new User() { Name = "D", Expenses = new decimal[] { 10M, 20M, 38.41M, -45M } }
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(invalid), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, message.StatusCode);

            dynamic json = message.Content.ReadAsAsync<dynamic>().Result;
            string errorText = json["Message"];

            Assert.AreNotEqual(string.Empty, errorText);

        }

        [TestMethod]
        public void TestValidData()
        {
            List<User> valid = new List<User>()
            {
                new User() { Name = "L", Expenses = new decimal[]{ 5.75M, 35M, 12.79M} },
                new User() { Name = "C", Expenses = new decimal[] { 12.00M, 15.00M, 23.23M } },
                new User() { Name = "D", Expenses = new decimal[] { 10M, 20M, 38.41M, 45M } }
            };


            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(valid));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                        ExpenseRepaymentCollection repayment = response.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;

                        Assert.AreEqual(1, repayment.Status);

                        List<ExpenseRepayment> repaymentsList = repayment.Repayments.ToList();

                        Assert.IsTrue(repayment.Repayments.Any(e => e.PayTo == "D" && e.PayFrom == "L" && e.Amout == 18.85M));
                        Assert.IsTrue(repayment.Repayments.Any(e => e.PayTo == "D" && e.PayFrom == "C" && e.Amout == 22.16M));
                    }
                }
            }
        }

        [TestMethod]
        public void TestAllExpensesZero()
        {
            List<User> zero = new List<User>()
            {
                new User() {Name = "L", Expenses = new decimal[] {0} },
                new User() {Name = "C", Expenses = new decimal[] {0} },
                new User() {Name = "D", Expenses = new decimal[] {0} }
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(zero), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(0, repayment.Status);
        }

        [TestMethod]
        public void TestAllExpensesEqual()
        {
            List<User> equal = new List<User>()
            {
                new User() {Name="L", Expenses = new decimal[] { 10 } },
                new User() {Name="C", Expenses = new decimal[] { 5, 5 } },
                new User() {Name="D", Expenses = new decimal[] { 2, 2, 2, 2, 2 } },
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(equal), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(0, repayment.Status);

        }

        [TestMethod]
        public void TestFloatingPointValues()
        {
            List<User> floating = new List<User>()
            {
                new User() {Name="L", Expenses = new decimal[] { 10.000000000001M } },
                new User() {Name="C", Expenses = new decimal[] { 5.0002000100999M, 5.0000000000009M } },
                new User() {Name="D", Expenses = new decimal[] { 2.0009999999999M, 1.999M, 1.9999999999999M, 2.00000000000001M, 2 } },
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(floating), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(0, repayment.Status);
        }

        [TestMethod]
        public void TestOnePennyOff()
        {
            List<User> onePennyOff = new List<User>()
            {
                new User() {Name="L", Expenses = new decimal[] { 10 } },
                new User() {Name="C", Expenses = new decimal[] { 5, 5 } },
                new User() {Name="D", Expenses = new decimal[] { 2, 2, 2, 2, 2.01M } },
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(onePennyOff), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(0, repayment.Status);
        }

        [TestMethod]
        public void TestDuplicateUsers()
        {
            List<User> duplicateUser = new List<User>()
            {
                new User() {Name="L", Expenses = new decimal[] { 18 } },
                new User() {Name="L", Expenses = new decimal[] { 3, 5 } },
                new User() {Name="L", Expenses = new decimal[] { 2, 2 } },
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(duplicateUser), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(0, repayment.Status);
        }
    }
}
