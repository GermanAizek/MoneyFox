namespace MoneyFox.Tests.Core.Commands.Payments.CreatePayment
{

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MoneyFox.Core.ApplicationCore.Domain.Aggregates.AccountAggregate;
    using MoneyFox.Core.Commands.Payments.CreatePayment;
    using MoneyFox.Core.Common.Interfaces;
    using MoneyFox.Infrastructure.Persistence;
    using Moq;
    using TestFramework;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class CreatePaymentCommandTests : IDisposable
    {
        private readonly AppDbContext context;
        private readonly Mock<IContextAdapter> contextAdapterMock;

        public CreatePaymentCommandTests()
        {
            context = InMemoryAppDbContextFactory.Create();
            contextAdapterMock = new Mock<IContextAdapter>();
            contextAdapterMock.SetupGet(x => x.Context).Returns(context);
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
        public async Task CreatePayment_PaymentSaved()
        {
            // Arrange
            var account = new Account(name: "test", initialBalance: 80);
            context.Add(account);
            context.SaveChanges();
            var payment1 = new Payment(date: DateTime.Now, amount: 20, type: PaymentType.Expense, chargedAccount: account);

            // Act
            await new CreatePaymentCommand.Handler(contextAdapterMock.Object).Handle(request: new CreatePaymentCommand(payment1), cancellationToken: default);

            // Assert
            Assert.Single(context.Payments);
            (await context.Payments.FindAsync(payment1.Id)).Should().NotBeNull();
        }

        [Theory]
        [InlineData(PaymentType.Expense, 60)]
        [InlineData(PaymentType.Income, 100)]
        public async Task CreatePayment_AccountCurrentBalanceUpdated(PaymentType paymentType, decimal newCurrentBalance)
        {
            // Arrange
            var account = new Account(name: "test", initialBalance: 80);
            context.Add(account);
            await context.SaveChangesAsync();
            var payment1 = new Payment(date: DateTime.Now, amount: 20, type: paymentType, chargedAccount: account);

            // Act
            await new CreatePaymentCommand.Handler(contextAdapterMock.Object).Handle(request: new CreatePaymentCommand(payment1), cancellationToken: default);

            // Assert
            var loadedAccount = await context.Accounts.FindAsync(account.Id);
            loadedAccount.Should().NotBeNull();
            loadedAccount.CurrentBalance.Should().Be(newCurrentBalance);
        }

        [Fact]
        public async Task CreatePaymentWithRecurring_PaymentSaved()
        {
            // Arrange
            var account = new Account(name: "test", initialBalance: 80);
            context.Add(account);
            context.SaveChanges();
            var payment1 = new Payment(date: DateTime.Now, amount: 20, type: PaymentType.Expense, chargedAccount: account);
            payment1.AddRecurringPayment(recurrence: PaymentRecurrence.Monthly, isLastDayOfMonth: false);

            // Act
            await new CreatePaymentCommand.Handler(contextAdapterMock.Object).Handle(request: new CreatePaymentCommand(payment1), cancellationToken: default);

            // Assert
            Assert.Single(context.Payments);
            Assert.Single(context.RecurringPayments);
            (await context.Payments.FindAsync(payment1.Id)).Should().NotBeNull();
            (await context.RecurringPayments.FindAsync(payment1.RecurringPayment.Id)).Should().NotBeNull();
        }
    }

}
