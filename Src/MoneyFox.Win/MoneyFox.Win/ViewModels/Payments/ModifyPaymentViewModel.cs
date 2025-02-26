﻿namespace MoneyFox.Win.ViewModels.Payments;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Accounts;
using AutoMapper;
using Categories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core._Pending_.Common.Messages;
using Core.ApplicationCore.Domain.Aggregates.AccountAggregate;
using Core.ApplicationCore.Queries;
using Core.Common.Interfaces;
using Core.Resources;
using MediatR;
using Pages.Categories;
using Pages.Payments;
using Services;

public abstract class ModifyPaymentViewModel : ObservableRecipient, IModifyPaymentViewModel
{
    private readonly IMapper mapper;
    private readonly IMediator mediator;
    private readonly IDialogService dialogService;
    private readonly INavigationService navigationService;
    private ObservableCollection<AccountViewModel> chargedAccounts = new();

    private PaymentRecurrence recurrence;
    private PaymentViewModel selectedPayment = new();
    private ObservableCollection<AccountViewModel> targetAccounts = new();
    private string title = Strings.AddPaymentLabel;

    private string amountString = "";

    private ObservableCollection<CategoryViewModel> categories = new();

    private bool isBusy;

    /// <summary>
    ///     Default constructor
    /// </summary>
    protected ModifyPaymentViewModel(IMediator mediator, IMapper mapper, IDialogService dialogService, INavigationService navigationService)
    {
        this.dialogService = dialogService;
        this.navigationService = navigationService;
        this.mediator = mediator;
        this.mapper = mapper;
    }

    /// <summary>
    ///     Opens the create category dialog.
    /// </summary>
    public AsyncRelayCommand AddNewCategoryCommand => new(async () => await new AddCategoryDialog().ShowAsync());

    public List<PaymentType> PaymentTypeList => new() { PaymentType.Expense, PaymentType.Income, PaymentType.Transfer };

    public ObservableCollection<CategoryViewModel> Categories
    {
        get => categories;

        private set
        {
            categories = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(field: ref isBusy, newValue: value);
    }

    /// <summary>
    ///     Updates the targetAccountViewModel and chargedAccountViewModel Comboboxes' dropdown lists.
    /// </summary>
    public RelayCommand SelectedItemChangedCommand => new(UpdateOtherComboBox);

    /// <summary>
    ///     Saves the PaymentViewModel or updates the existing depending on the IsEdit Flag.
    /// </summary>
    public RelayCommand SaveCommand => new(async () => await SavePaymentBaseAsync());

    /// <inheritdoc />
    public RelayCommand CancelCommand => new(Cancel);

    /// <inheritdoc />
    public AsyncRelayCommand GoToSelectCategoryDialogCommand => new(async () => await new SelectCategoryDialog().ShowAsync());

    /// <summary>
    ///     Resets the CategoryViewModel of the currently selected PaymentViewModel
    /// </summary>
    public RelayCommand ResetCategoryCommand => new(ResetSelection);

    /// <summary>
    ///     The selected recurrence
    /// </summary>
    public PaymentRecurrence Recurrence
    {
        get => recurrence;

        set
        {
            if (recurrence == value)
            {
                return;
            }

            recurrence = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     List with the different recurrence types.     This has to have the same order as the enum
    /// </summary>
    public List<PaymentRecurrence> RecurrenceList
        => new()
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

    /// <summary>
    ///     The selected PaymentViewModel
    /// </summary>
    public PaymentViewModel SelectedPayment
    {
        get => selectedPayment;

        set
        {
            if (selectedPayment == value)
            {
                return;
            }

            selectedPayment = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AccountHeader));
            OnPropertyChanged(nameof(AmountString));
        }
    }

    public string AmountString
    {
        get => amountString;

        set
        {
            if (amountString == value)
            {
                return;
            }

            amountString = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gives access to all accounts for Charged Dropdown list
    /// </summary>
    public ObservableCollection<AccountViewModel> ChargedAccounts
    {
        get => chargedAccounts;

        private set
        {
            chargedAccounts = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gives access to all accounts for Target Dropdown list
    /// </summary>
    public ObservableCollection<AccountViewModel> TargetAccounts
    {
        get => targetAccounts;

        private set
        {
            targetAccounts = value;
            OnPropertyChanged();
        }
    }

    public virtual string Title
    {
        get => title;

        set
        {
            if (title == value)
            {
                return;
            }

            title = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Returns the Header for the AccountViewModel field
    /// </summary>
    public string AccountHeader => SelectedPayment.Type == PaymentType.Income ? Strings.TargetAccountLabel : Strings.ChargedAccountLabel;

    protected virtual async Task InitializeAsync()
    {
        var accounts = mapper.Map<List<AccountViewModel>>(await mediator.Send(new GetAccountsQuery()));
        ChargedAccounts = new(accounts);
        TargetAccounts = new(accounts);
        Categories = new(mapper.Map<List<CategoryViewModel>>(await mediator.Send(new GetCategoryBySearchTermQuery())));
        IsActive = true;
    }

    protected override void OnActivated()
    {
        Messenger.Register<ModifyPaymentViewModel, CategorySelectedMessage>(recipient: this, handler: (r, m) => r.ReceiveMessageAsync(m));
    }

    protected override void OnDeactivated()
    {
        Messenger.Unregister<CategorySelectedMessage>(this);
    }

    protected abstract Task SavePaymentAsync();

    private async Task SavePaymentBaseAsync()
    {
        if (SelectedPayment.ChargedAccount == null)
        {
            await dialogService.ShowMessageAsync(title: Strings.MandatoryFieldEmptyTitle, message: Strings.AccountRequiredMessage);

            return;
        }

        if (decimal.TryParse(s: AmountString, style: NumberStyles.Any, provider: CultureInfo.CurrentCulture, result: out var convertedValue))
        {
            SelectedPayment.Amount = convertedValue;
        }
        else
        {
            await dialogService.ShowMessageAsync(title: Strings.InvalidNumberTitle, message: Strings.InvalidNumberCurrentBalanceMessage);

            return;
        }

        if (SelectedPayment.Amount < 0)
        {
            await dialogService.ShowMessageAsync(title: Strings.AmountMayNotBeNegativeTitle, message: Strings.AmountMayNotBeNegativeMessage);

            return;
        }

        if (SelectedPayment.Category != null && SelectedPayment.Category.RequireNote && string.IsNullOrEmpty(SelectedPayment.Note))
        {
            await dialogService.ShowMessageAsync(title: Strings.MandatoryFieldEmptyTitle, message: Strings.ANoteForPaymentIsRequired);

            return;
        }

        if (SelectedPayment.IsRecurring
            && !SelectedPayment.RecurringPayment!.IsEndless
            && SelectedPayment.RecurringPayment.EndDate != null
            && SelectedPayment.RecurringPayment.EndDate < DateTime.Now)
        {
            await dialogService.ShowMessageAsync(title: Strings.InvalidEnddateTitle, message: Strings.InvalidEnddateMessage);

            return;
        }

        try
        {
            IsBusy = true;
            await SavePaymentAsync();
            Messenger.Send(new ReloadMessage());
            navigationService.GoBack();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Cancel()
    {
        navigationService.GoBack();
    }

    public async Task ReceiveMessageAsync(CategorySelectedMessage message)
    {
        if (SelectedPayment == null || message == null)
        {
            return;
        }

        SelectedPayment.Category = mapper.Map<CategoryViewModel>(await mediator.Send(new GetCategoryByIdQuery(message.CategoryId)));
    }

    private void ResetSelection()
    {
        SelectedPayment.Category = null;
    }

    private void UpdateOtherComboBox()
    {
        var tempCollection = new ObservableCollection<AccountViewModel>(ChargedAccounts);
        foreach (var account in TargetAccounts)
        {
            if (!tempCollection.Contains(account))
            {
                tempCollection.Add(account);
            }
        }

        foreach (var account in tempCollection)
        {
            //fills target accounts
            if (!TargetAccounts.Contains(account))
            {
                TargetAccounts.Add(account);
            }

            //fills charged accounts
            if (!ChargedAccounts.Contains(account))
            {
                ChargedAccounts.Add(account);
            }
        }

        TargetAccounts.Remove(selectedPayment.ChargedAccount);
    }
}
