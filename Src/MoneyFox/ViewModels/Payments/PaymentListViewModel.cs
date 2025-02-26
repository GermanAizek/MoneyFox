﻿namespace MoneyFox.ViewModels.Payments
{

    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounts;
    using AutoMapper;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Core._Pending_.Common.Messages;
    using Core.ApplicationCore.Domain.Aggregates.AccountAggregate;
    using Core.ApplicationCore.Queries;
    using Core.Resources;
    using Extensions;
    using Groups;
    using MediatR;
    using Views.Payments;
    using Xamarin.Forms;

    public class PaymentListViewModel : ObservableRecipient
    {
        private readonly IMapper mapper;

        private readonly IMediator mediator;

        private bool isRunning;

        private ObservableCollection<DateListGroupCollection<PaymentViewModel>>
            payments = new ObservableCollection<DateListGroupCollection<PaymentViewModel>>();

        private AccountViewModel selectedAccount = new AccountViewModel();

        public PaymentListViewModel(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            IsActive = true;
        }

        public AccountViewModel SelectedAccount
        {
            get => selectedAccount;

            set
            {
                selectedAccount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DateListGroupCollection<PaymentViewModel>> Payments
        {
            get => payments;

            private set
            {
                payments = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     List with the different recurrence types.
        ///     This has to have the same order as the enum
        /// </summary>
        public static List<PaymentRecurrence> RecurrenceList
            => new List<PaymentRecurrence>
            {
                PaymentRecurrence.Daily,
                PaymentRecurrence.DailyWithoutWeekend,
                PaymentRecurrence.Weekly,
                PaymentRecurrence.Biweekly,
                PaymentRecurrence.Monthly,
                PaymentRecurrence.Bimonthly,
                PaymentRecurrence.Quarterly,
                PaymentRecurrence.Biannually,
                PaymentRecurrence.Yearly
            };

        public AsyncRelayCommand GoToAddPaymentCommand
            => new AsyncRelayCommand(async () => await Shell.Current.GoToModalAsync(ViewModelLocator.AddPaymentRoute));

        public AsyncRelayCommand<PaymentViewModel> GoToEditPaymentCommand
            => new AsyncRelayCommand<PaymentViewModel>(
                async paymentViewModel => await Shell.Current.Navigation.PushModalAsync(
                    new NavigationPage(new EditPaymentPage(paymentViewModel.Id)) { BarBackgroundColor = Color.Transparent }));

        protected override void OnActivated()
        {
            Messenger.Register<PaymentListViewModel, ReloadMessage>(recipient: this, handler: (r, m) => OnAppearingAsync(SelectedAccount.Id));
            Messenger.Register<PaymentListViewModel, PaymentListFilterChangedMessage>(recipient: this, handler: (r, m) => LoadPaymentsByMessageAsync(m));
        }

        protected override void OnDeactivated()
        {
            Messenger.Unregister<ReloadMessage>(this);
            Messenger.Unregister<PaymentListFilterChangedMessage>(this);
        }

        public async Task OnAppearingAsync(int accountId)
        {
            SelectedAccount = mapper.Map<AccountViewModel>(await mediator.Send(new GetAccountByIdQuery(accountId)));
            await LoadPaymentsByMessageAsync(new PaymentListFilterChangedMessage());
        }

        public async Task LoadPaymentsByMessageAsync(PaymentListFilterChangedMessage message)
        {
            try
            {
                if (isRunning)
                {
                    return;
                }

                isRunning = true;
                var paymentVms = mapper.Map<List<PaymentViewModel>>(
                    await mediator.Send(
                        new GetPaymentsForAccountIdQuery(
                            accountId: SelectedAccount.Id,
                            timeRangeStart: message.TimeRangeStart,
                            timeRangeEnd: message.TimeRangeEnd,
                            isClearedFilterActive: message.IsClearedFilterActive,
                            isRecurringFilterActive: message.IsRecurringFilterActive,
                            filteredPaymentType: message.FilteredPaymentType)));

                paymentVms.ForEach(x => x.CurrentAccountId = SelectedAccount.Id);
                var dailyItems = DateListGroupCollection<PaymentViewModel>.CreateGroups(
                    items: paymentVms,
                    getKey: s => s.Date.ToString(format: "D", provider: CultureInfo.CurrentCulture),
                    getSortKey: s => s.Date);

                dailyItems.ForEach(CalculateSubBalances);
                Payments = new ObservableCollection<DateListGroupCollection<PaymentViewModel>>(dailyItems);
            }
            finally
            {
                isRunning = false;
            }
        }

        private void CalculateSubBalances(DateListGroupCollection<PaymentViewModel> group)
        {
            group.Subtitle = string.Format(
                format: Strings.ExpenseAndIncomeTemplate,
                arg0: group.Where(x => x.Type != PaymentType.Income && x.ChargedAccount.Id == SelectedAccount.Id)
                    .Sum(x => x.Amount),
                arg1: group.Where(
                        x => x.Type == PaymentType.Income
                             || x.Type == PaymentType.Transfer && x.TargetAccount != null && x.TargetAccount.Id == SelectedAccount.Id)
                    .Sum(x => x.Amount));
        }
    }

}
