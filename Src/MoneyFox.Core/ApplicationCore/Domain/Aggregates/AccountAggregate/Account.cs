namespace MoneyFox.Core.ApplicationCore.Domain.Aggregates.AccountAggregate
{

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dawn;
    using JetBrains.Annotations;
    using MoneyFox.Core.Common.Interfaces;

    public class Account : EntityBase, IAggregateRoot
    {
        [UsedImplicitly]
        private Account() { }

        public Account(string name, decimal initialBalance = 0, string note = "", bool isExcluded = false)
        {
            Guard.Argument(value: name, name: nameof(name)).NotNull().NotWhiteSpace();
            Name = name;
            CurrentBalance = initialBalance;
            Note = note;
            IsExcluded = isExcluded;
        }

        public int Id
        {
            get;

            [UsedImplicitly]
            private set;
        }

        public string Name { get; private set; } = null!;

        public decimal CurrentBalance { get; private set; }

        public string? Note { get; private set; }

        public bool IsExcluded { get; private set; }

        public bool IsDeactivated { get; private set; }

        public void UpdateAccount(string name, string note = "", bool isExcluded = false)
        {
            Guard.Argument(value: name, name: nameof(name)).NotNull().NotWhiteSpace();
            Name = name;
            Note = note;
            IsExcluded = isExcluded;
        }

        public void AddPaymentAmount(Payment payment)
        {
            if (payment.IsCleared is false)
            {
                return;
            }

            if (payment.Type != PaymentType.Income && payment.ChargedAccount.Id == Id)
            {
                CurrentBalance -= payment.Amount;
            }
            else
            {
                CurrentBalance += payment.Amount;
            }
        }

        public void RemovePaymentAmount(Payment payment)
        {
            Guard.Argument(value: payment, name: nameof(payment)).NotNull();
            if (!payment.IsCleared)
            {
                return;
            }

            if (payment.Type != PaymentType.Income && payment.ChargedAccount.Id == Id)
            {
                CurrentBalance -= -payment.Amount;
            }
            else
            {
                CurrentBalance += -payment.Amount;
            }
        }

        public void Deactivate()
        {
            IsDeactivated = true;
        }
    }

}
