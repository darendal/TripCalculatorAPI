using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TripCalculatorAPI.Models;

namespace TripCalculatorAPI.Controllers
{
    /// <summary>
    /// API for calculating expenses for a group of users
    /// </summary>
    public class TripCalculatorController : ApiController
    {
        /// <summary>
        /// Given a list of users, calculate the minimum number of payments to equalize expenses
        /// to the nearest cent
        /// </summary>
        /// <param name="users">List of users</param>
        /// <returns>Expense report collection, containing Expense Repayments</returns>
        public ExpenseRepaymentCollection Post(IEnumerable<User> users)
        {
            if (users == null || !users.Any() || users.All(u => !u.Expenses.Any()))
            {
                // empty result passed
                ThrowResponseException(HttpStatusCode.BadRequest, "Empty list of users entered");
            }

            if (users.SelectMany(u => u.Expenses).Any(exp => exp < 0))
            {
                // one or more expenses has a negative amount
                ThrowResponseException(HttpStatusCode.BadRequest, "One or more Expense line items have a negative expense amount");
            }

            ExpenseRepaymentCollection result = new ExpenseRepaymentCollection();

            // Flatten list into 1 expense line item per name, with their total expenditure
            var totalExpenses = users.GroupBy(u => u.Name)
                .Select(x => 
                    new ExpenseLineItem()
                    {
                        Name = x.Key,
                        ExpenseAmount = x.SelectMany(y => y.Expenses).Sum()
                    }
                 ).ToList();

            decimal totalExpenditure = totalExpenses.Sum(e => e.ExpenseAmount);

            if (totalExpenditure == 0)
            {
                return result;
            }

            decimal equalShares = totalExpenditure / totalExpenses.Count;

            if (totalExpenses.All(e => e.EqualWithinOneCent(equalShares)))
            {
                // All users have already paid equally, no repayments need to be done
                return result;
            }

            //Because we only ever need to look at the front of the line, a stack is more efficient
            Stack<ExpenseLineItem> paidMoreThanEqual = new Stack<ExpenseLineItem>(totalExpenses.Where(e => e.ExpenseAmount > equalShares));
            Stack<ExpenseLineItem> paidLessThanEqual = new Stack<ExpenseLineItem>(totalExpenses.Where(e => e.ExpenseAmount < equalShares));

            List<ExpenseRepayment> repayments = new List<ExpenseRepayment>();

            while (paidMoreThanEqual.Any() && paidLessThanEqual.Any())
            {
                ExpenseLineItem currentUnderpayer = paidLessThanEqual.Peek();
                ExpenseLineItem currentOverpayer = paidMoreThanEqual.Peek();

                decimal amountToPay = Math.Min(currentOverpayer.ExpenseAmount - equalShares, equalShares - currentUnderpayer.ExpenseAmount);

                repayments.Add(new ExpenseRepayment() { PayFrom = currentUnderpayer.Name, PayTo = currentOverpayer.Name, Amout = amountToPay });

                currentOverpayer.ExpenseAmount -= amountToPay;
                currentUnderpayer.ExpenseAmount += amountToPay;

                if (currentUnderpayer.EqualWithinOneCent(equalShares))
                {
                    paidLessThanEqual.Pop();
                }

                if (currentOverpayer.EqualWithinOneCent(equalShares))
                {
                    paidMoreThanEqual.Pop();
                }
            }

            result.Repayments = repayments.ToArray();
            result.Status = (byte)(result.Repayments.Any() ? 1 : 0);
            return result;
        }

        private void ThrowResponseException(HttpStatusCode statusCode, string message)
        {
            var errorResponse = Request.CreateErrorResponse(statusCode, message);
            throw new HttpResponseException(errorResponse);
        }
    }
}
