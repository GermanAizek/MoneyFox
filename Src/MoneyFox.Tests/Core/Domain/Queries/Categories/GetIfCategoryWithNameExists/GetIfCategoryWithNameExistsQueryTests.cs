﻿namespace MoneyFox.Tests.Core.Domain.Queries.Categories.GetIfCategoryWithNameExists
{

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MoneyFox.Core.ApplicationCore.Domain.Aggregates.CategoryAggregate;
    using MoneyFox.Core.ApplicationCore.Queries;
    using MoneyFox.Infrastructure.Persistence;
    using TestFramework;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class GetIfCategoryWithNameExistsQueryTests : IDisposable
    {
        private readonly AppDbContext context;

        public GetIfCategoryWithNameExistsQueryTests()
        {
            context = InMemoryAppDbContextFactory.Create();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            InMemoryAppDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task CategoryWithSameNameDontExist()
        {
            // Arrange
            var testCat1 = new Category("Ausgehen");
            await context.Categories.AddAsync(testCat1);
            await context.SaveChangesAsync();

            // Act
            var result = await new GetIfCategoryWithNameExistsQuery.Handler(context).Handle(
                request: new GetIfCategoryWithNameExistsQuery("Foo"),
                cancellationToken: default);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CategoryWithSameNameExist()
        {
            // Arrange
            var testCat1 = new Category("Ausgehen");
            await context.Categories.AddAsync(testCat1);
            await context.SaveChangesAsync();

            // Act
            var result = await new GetIfCategoryWithNameExistsQuery.Handler(context).Handle(
                request: new GetIfCategoryWithNameExistsQuery(testCat1.Name),
                cancellationToken: default);

            // Assert
            result.Should().BeTrue();
        }
    }

}
