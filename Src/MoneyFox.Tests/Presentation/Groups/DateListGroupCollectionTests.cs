﻿namespace MoneyFox.Tests.Presentation.Groups
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using FluentAssertions;
    using MoneyFox.Groups;
    using MoneyFox.ViewModels.Payments;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class DateListGroupCollectionTests
    {
        [Fact]
        public void CreateGroupReturnsCorrectGroup()
        {
            // Arrange
            var paymentList = new List<PaymentViewModel>
            {
                new PaymentViewModel { Id = 1, Date = DateTime.Now }, new PaymentViewModel { Id = 2, Date = DateTime.Now.AddMonths(-1) }
            };

            // Act
            var createdGroup = DateListGroupCollection<PaymentViewModel>.CreateGroups(
                items: paymentList,
                getKey: s => s.Date.ToString(format: "D", provider: CultureInfo.CurrentCulture),
                getSortKey: s => s.Date);

            // Assert
            createdGroup.Should().HaveCount(2);
            createdGroup[0][0].Id.Should().Be(1);
            createdGroup[1][0].Id.Should().Be(2);
        }
    }

}
