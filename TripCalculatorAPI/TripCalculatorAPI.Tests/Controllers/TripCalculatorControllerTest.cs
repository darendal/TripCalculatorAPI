using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TripCalculatorAPI.Controllers;
using TripCalculatorAPI.Models;

namespace TripCalculatorAPI.Tests.Controllers
{
    [TestClass]
    public class TripCalculatorControllerTest
    {
        private const string APIAddress = "http://localhost/api/TripCalculator";

        private TripCalculatorController tripController;
        private HttpServer server;

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
            if (message != null)
            {
                message.Dispose();
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
                new User() { Name = "L", Expenses = new decimal[] { -5.75M, 35M, 12.79M } },
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
                new User() { Name = "L", Expenses = new decimal[] { 5.75M, 35M, 12.79M } },
                new User() { Name = "C", Expenses = new decimal[] { 12.00M, 15.00M, 23.23M } },
                new User() { Name = "D", Expenses = new decimal[] { 10M, 20M, 38.41M, 45M } }
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, APIAddress))
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
                new User() {Name = "L", Expenses = new decimal[] {0}},
                new User() {Name = "C", Expenses = new decimal[] {0 } },
                new User() {Name = "D", Expenses = new decimal[] {0 } }
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

        [TestMethod]
        public void TestLargeDataSet()
        {
            List<User> data = new List<User>()
            {
                new User() {Name = "A", Expenses = new decimal[] {1,2,3,4,5,6,7,8,9,10 } },
                new User() {Name = "B", Expenses = new decimal[] {11,6,7,8,9,10 } },
                new User() {Name = "C", Expenses = new decimal[] {1,8,9,10 } },
                new User() {Name = "D", Expenses = new decimal[] {1,2,12.55m } },
                new User() {Name = "E", Expenses = new decimal[] {3,4,5,6,7,8,9.8M,10 } },
                new User() {Name = "F", Expenses = new decimal[] {1,20 } },
                new User() {Name = "G", Expenses = new decimal[] { } },
                new User() {Name = "H", Expenses = new decimal[] {7,8,9,10 } },
                new User() {Name = "I", Expenses = new decimal[] {1,2,3,4,5,6,7,8,9,10 } },
                new User() {Name = "J", Expenses = new decimal[] {1,2,3,4,5,8,9,10 } },
                new User() {Name = "K", Expenses = new decimal[] {1,2,3,4,5,6,7,8,9,10 } },
                new User() {Name = "L", Expenses = new decimal[] {1,2,3.14M,6,7,8,9,10 } },
                new User() {Name = "M", Expenses = new decimal[] {0.01M,1,2,3,4,5,6,7,8,9,10 } },
                new User() {Name = "N", Expenses = new decimal[] {1,2,3,10 } },
                new User() {Name = "O", Expenses = new decimal[] {1,22,3,4,5,6,7,8,9,10 } },
                new User() {Name = "P", Expenses = new decimal[] {1,10 } },
                new User() {Name = "Q", Expenses = new decimal[] {1,24 } }
            };

            message = GetHttpResponse(JsonConvert.SerializeObject(data), HttpMethod.Post);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, message.StatusCode);

            ExpenseRepaymentCollection repayment = message.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;
            Assert.AreEqual(1, repayment.Status);

            // Check that nobody is paying themselves
            Assert.IsTrue(repayment.Repayments.Any(e => e.PayFrom.Equals(e.PayFrom, StringComparison.CurrentCultureIgnoreCase)));

            Assert.IsTrue(VerifyRepaymentCollection(repayment, data));
        }

        private HttpResponseMessage GetHttpResponse(string content, HttpMethod method)
        {
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, APIAddress))
                {
                    request.Content = new StringContent(content);
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    return client.SendAsync(request, CancellationToken.None).Result;
                }
            }
        }

        /// <summary>
        /// Helper method used to verify if a given Expense Repayment Collection
        /// can reasonably be used to satisfy the given user set
        /// </summary>
        /// <param name="repayment"></param>
        /// <param name="originalData"></param>
        /// <returns> true if repayment matches original data, false otherwise</returns>
        private bool VerifyRepaymentCollection(ExpenseRepaymentCollection repayment, List<User> originalData)
        {
            var d = originalData.ToDictionary(x => x.Name, y => y.Expenses.Sum());

            foreach (var e in repayment.Repayments)
            {
                d[e.PayTo] -= e.Amout;
                d[e.PayFrom] += e.Amout;
            }

            var l = d.Select(x => new ExpenseLineItem() { Name = x.Key, ExpenseAmount = x.Value });
            decimal average = l.Average(y => y.ExpenseAmount);

            return l.All(x => x.EqualWithinOneCent(average));
        }
    }
}
