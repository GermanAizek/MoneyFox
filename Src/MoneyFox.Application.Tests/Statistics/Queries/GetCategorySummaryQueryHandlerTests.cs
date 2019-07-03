﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MoneyFox.Application.Statistics.Queries.GetCategorySummary;
using MoneyFox.Application.Tests.Infrastructure;
using MoneyFox.Domain;
using MoneyFox.Domain.Entities;
using MoneyFox.Persistence;
using Xunit;

namespace MoneyFox.Application.Tests.Statistics.Queries
{
    [ExcludeFromCodeCoverage]
    public class GetCategorySummaryQueryHandlerTests : IClassFixture<QueryTestFixture>
    {
        private readonly EfCoreContext context;

        public GetCategorySummaryQueryHandlerTests(QueryTestFixture fixture)
        {
            context = fixture.Context;
        }

        [Fact]
        public async Task GetValues_CorrectSums()
        {
            // Arrange
            var testCat1 = new Category("Ausgehen");
            var testCat2 = new Category("Rent");
            var testCat3 = new Category("Food");
            var testCat4 = new Category("Income");

            var account = new Account("test");

            var paymentList = new List<Payment>
            {
                new Payment(DateTime.Today, 60, PaymentType.Income, account, category:testCat1),
                new Payment(DateTime.Today, 90, PaymentType.Expense, account, category:testCat1),
                new Payment(DateTime.Today, 10, PaymentType.Expense, account, category:testCat3),
                new Payment(DateTime.Today, 90, PaymentType.Expense, account, category:testCat2),
                new Payment(DateTime.Today, 100, PaymentType.Income, account, category:testCat4)
            };

            context.Payments.AddRange(paymentList);
            context.SaveChanges();

            // Act
            var result = (await new GetCategorySummaryQueryHandler(context).Handle(new GetCategorySummaryQuery
            {
                    StartDate = DateTime.Today.AddDays(-3),
                    EndDate = DateTime.Today.AddDays(3)
                }, default))
                .ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(-90, result[0].Value);
            Assert.Equal(-30, result[1].Value);
            Assert.Equal(-10, result[2].Value);
            Assert.Equal(100, result[3].Value);
        }
    }
}
