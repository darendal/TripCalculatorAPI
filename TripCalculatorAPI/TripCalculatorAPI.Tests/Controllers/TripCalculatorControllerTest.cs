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

        [TestInitialize]
        public void TestInitialize()
        {
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

            List<ExpenseLineItem> valid = new List<ExpenseLineItem>();
        }

        /// <summary>
        /// Test that passing a null list returns a 400 bad request error.
        /// Checks that the error returned also includes a detailed message
        /// </summary>
        [TestMethod]
        public void TestNullValuesPassed()
        {
            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent("");
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

                        dynamic json = response.Content.ReadAsAsync<dynamic>().Result;
                        string errorText = json["Message"];

                        Assert.AreNotEqual(string.Empty, errorText);
                    }
                }
            }
        }

        [TestMethod]
        public void TestInvalidExpenseValuesPassed()
        {
            List<ExpenseLineItem> invalid = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "A", ExpenseAmount = -2.00M },
                new ExpenseLineItem() {Name = "B", ExpenseAmount = 10.00M },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 10.00M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = -10.00M },
                new ExpenseLineItem() {Name = "E", ExpenseAmount = 10.00M },
                new ExpenseLineItem() {Name = "F", ExpenseAmount = 10.00M },
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(invalid));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

                        dynamic json = response.Content.ReadAsAsync<dynamic>().Result;
                        string errorText = json["Message"];

                        Assert.AreNotEqual(string.Empty, errorText);
                    }
                }
            }

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
            List<ExpenseLineItem> zero = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "L", ExpenseAmount = 0 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 0 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 0 },
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(zero));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                        ExpenseRepaymentCollection repayment = response.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;

                        Assert.AreEqual(0, repayment.Status);
                    }
                }
            }
        }

        [TestMethod]
        public void TestAllExpensesEqual()
        {
            List<ExpenseLineItem> equal = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "L", ExpenseAmount = 10 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(equal));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                        ExpenseRepaymentCollection repayment = response.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;

                        Assert.AreEqual(0, repayment.Status);
                    }
                }
            }
        }

        [TestMethod]
        public void TestFloatingPointValues()
        {
            List<ExpenseLineItem> floating = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "L", ExpenseAmount = 10.000000000001M },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5.0002000100999M },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5.0000000000009M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2.0009999999999M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 1.999M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 1.9999999999999M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2.00000000000001M },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(floating));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                        ExpenseRepaymentCollection repayment = response.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;

                        Assert.AreEqual(0, repayment.Status);
                    }
                }
            }
        }

        [TestMethod]
        public void TestOnePennyOff()
        {
            List<ExpenseLineItem> onePennyOff = new List<ExpenseLineItem>()
            {
                new ExpenseLineItem() {Name = "L", ExpenseAmount = 10 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5 },
                new ExpenseLineItem() {Name = "C", ExpenseAmount = 5 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2 },
                new ExpenseLineItem() {Name = "D", ExpenseAmount = 2.01M },
            };

            using (HttpMessageInvoker client = new HttpMessageInvoker(server))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_Address))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(onePennyOff));
                    request.Content.Headers.Add("content", "application/json");
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    using (HttpResponseMessage response = client.SendAsync(request, CancellationToken.None).Result)
                    {
                        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                        ExpenseRepaymentCollection repayment = response.Content.ReadAsAsync<ExpenseRepaymentCollection>().Result;

                        Assert.AreEqual(0, repayment.Status);
                    }
                }
            }
        }
    }
}
