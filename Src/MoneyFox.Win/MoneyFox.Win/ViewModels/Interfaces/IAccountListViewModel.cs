﻿namespace MoneyFox.Win.ViewModels.Interfaces;

using System.Collections.ObjectModel;
using Accounts;
using CommunityToolkit.Mvvm.Input;
using Groups;

/// <summary>
///     Representation of the AccountListView.
/// </summary>
public interface IAccountListViewModel
{
    /// <summary>
    ///     All existing accounts
    /// </summary>
    ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>> Accounts { get; }

    /// <summary>
    ///     Indicates if there are accounts to display.
    /// </summary>
    bool HasNoAccounts { get; }

    /// <summary>
    ///     View Model for the balance view integrated in the account list view
    /// </summary>
    IBalanceViewModel BalanceViewModel { get; }

    /// <summary>
    ///     View Model for the actions associated with the account list.
    /// </summary>
    IAccountListViewActionViewModel ViewActionViewModel { get; }

    /// <summary>
    ///     Loads the data
    /// </summary>
    AsyncRelayCommand LoadDataCommand { get; }

    /// <summary>
    ///     Open the payment overview for this Account.
    /// </summary>
    RelayCommand<AccountViewModel> OpenOverviewCommand { get; }

    /// <summary>
    ///     Edit the selected Account
    /// </summary>
    RelayCommand<AccountViewModel> EditAccountCommand { get; }

    /// <summary>
    ///     Deletes the selected Account
    /// </summary>
    AsyncRelayCommand<AccountViewModel> DeleteAccountCommand { get; }
}
