﻿namespace MoneyFox.Win.ViewModels.DataBackup;

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core._Pending_.Common.Facades;
using Core.ApplicationCore.Domain.Exceptions;
using Core.ApplicationCore.UseCases.DbBackup;
using Core.Common.Interfaces;
using Core.Interfaces;
using Core.Resources;
using Serilog;

public class BackupViewModel : ObservableObject, IBackupViewModel
{
    private readonly IBackupService backupService;
    private readonly IConnectivityAdapter connectivity;
    private readonly IDialogService dialogService;
    private readonly ISettingsFacade settingsFacade;
    private readonly IToastService toastService;
    private bool backupAvailable;
    private UserAccount userAccount;

    private DateTime backupLastModified;
    private bool isLoadingBackupAvailability;

    public BackupViewModel(
        IBackupService backupService,
        IDialogService dialogService,
        IConnectivityAdapter connectivity,
        ISettingsFacade settingsFacade,
        IToastService toastService)
    {
        this.backupService = backupService;
        this.dialogService = dialogService;
        this.connectivity = connectivity;
        this.settingsFacade = settingsFacade;
        this.toastService = toastService;
    }

    public UserAccount UserAccount
    {
        get => userAccount;

        set
        {
            if (userAccount == value)
            {
                return;
            }

            userAccount = value;
            OnPropertyChanged();
        }
    }

    public AsyncRelayCommand InitializeCommand => new(async () => await InitializeAsync());

    public AsyncRelayCommand LoginCommand => new(async () => await LoginAsync());

    public AsyncRelayCommand LogoutCommand => new(async () => await LogoutAsync());

    public AsyncRelayCommand BackupCommand => new(async () => await CreateBackupAsync());

    public AsyncRelayCommand RestoreCommand => new(async () => await RestoreBackupAsync());

    public DateTime BackupLastModified
    {
        get => backupLastModified;

        private set
        {
            if (backupLastModified == value)
            {
                return;
            }

            backupLastModified = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingBackupAvailability
    {
        get => isLoadingBackupAvailability;

        private set
        {
            if (isLoadingBackupAvailability == value)
            {
                return;
            }

            isLoadingBackupAvailability = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoggedIn => settingsFacade.IsLoggedInToBackupService;

    public bool BackupAvailable
    {
        get => backupAvailable;

        private set
        {
            if (backupAvailable == value)
            {
                return;
            }

            backupAvailable = value;
            OnPropertyChanged();
        }
    }

    public bool IsAutoBackupEnabled
    {
        get => settingsFacade.IsBackupAutouploadEnabled;

        set
        {
            if (settingsFacade.IsBackupAutouploadEnabled == value)
            {
                return;
            }

            settingsFacade.IsBackupAutouploadEnabled = value;
            OnPropertyChanged();
        }
    }

    private async Task InitializeAsync()
    {
        await LoadedAsync();
    }

    private async Task LoadedAsync()
    {
        if (!IsLoggedIn)
        {
            OnPropertyChanged(nameof(IsLoggedIn));

            return;
        }

        if (!connectivity.IsConnected)
        {
            await dialogService.ShowMessageAsync(title: Strings.NoNetworkTitle, message: Strings.NoNetworkMessage);
        }

        IsLoadingBackupAvailability = true;
        try
        {
            BackupAvailable = await backupService.IsBackupExistingAsync();
            BackupLastModified = await backupService.GetBackupDateAsync();
            UserAccount = await backupService.GetUserAccount();
        }
        catch (BackupAuthenticationFailedException ex)
        {
            await backupService.LogoutAsync();
            await dialogService.ShowMessageAsync(title: Strings.AuthenticationFailedTitle, message: Strings.ErrorMessageAuthenticationFailed);
        }
        catch (Exception ex)
        {
            if (ex.StackTrace == "4f37.717b")
            {
                await backupService.LogoutAsync();
                await dialogService.ShowMessageAsync(title: Strings.AuthenticationFailedTitle, message: Strings.ErrorMessageAuthenticationFailed);
            }

            Log.Error(exception: ex, messageTemplate: "Issue on loading backup view");
        }

        IsLoadingBackupAvailability = false;
    }

    private async Task LoginAsync()
    {
        if (!connectivity.IsConnected)
        {
            await dialogService.ShowMessageAsync(title: Strings.NoNetworkTitle, message: Strings.NoNetworkMessage);
        }

        try
        {
            await backupService.LoginAsync();
            UserAccount = await backupService.GetUserAccount();
        }
        catch (BackupOperationCanceledException)
        {
            await dialogService.ShowMessageAsync(title: Strings.CanceledTitle, message: Strings.LoginCanceledMessage);
        }
        catch (Exception ex)
        {
            await dialogService.ShowMessageAsync(
                title: Strings.LoginFailedTitle,
                message: string.Format(format: Strings.UnknownErrorMessage, arg0: ex.Message));
        }

        OnPropertyChanged(nameof(IsLoggedIn));
        await LoadedAsync();
    }

    private async Task LogoutAsync()
    {
        try
        {
            await backupService.LogoutAsync();
        }
        catch (BackupOperationCanceledException)
        {
            await dialogService.ShowMessageAsync(title: Strings.CanceledTitle, message: Strings.LogoutCanceledMessage);
        }
        catch (Exception ex)
        {
            await dialogService.ShowMessageAsync(title: Strings.GeneralErrorTitle, message: ex.Message);
        }

        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(nameof(IsLoggedIn));
    }

    private async Task CreateBackupAsync()
    {
        if (!await ShowOverwriteBackupInfoAsync())
        {
            return;
        }

        await dialogService.ShowLoadingDialogAsync();
        try
        {
            await backupService.UploadBackupAsync(BackupMode.Manual);
            await toastService.ShowToastAsync(Strings.BackupCreatedMessage);
            BackupLastModified = DateTime.Now;
        }
        catch (BackupOperationCanceledException)
        {
            await dialogService.ShowMessageAsync(title: Strings.CanceledTitle, message: Strings.UploadBackupCanceledMessage);
        }
        catch (Exception ex)
        {
            await dialogService.ShowMessageAsync(title: Strings.BackupFailedTitle, message: ex.Message);
        }

        await dialogService.HideLoadingDialogAsync();
    }

    private async Task RestoreBackupAsync()
    {
        if (!await ShowOverwriteDataInfoAsync())
        {
            return;
        }

        var backupDate = await backupService.GetBackupDateAsync();
        if (settingsFacade.LastDatabaseUpdate <= backupDate || await ShowForceOverrideConfirmationAsync())
        {
            await dialogService.ShowLoadingDialogAsync();
            try
            {
                await backupService.RestoreBackupAsync(BackupMode.Manual);
                await toastService.ShowToastAsync(Strings.BackupRestoredMessage);
            }
            catch (BackupOperationCanceledException)
            {
                await dialogService.ShowMessageAsync(title: Strings.CanceledTitle, message: Strings.RestoreBackupCanceledMessage);
            }
            catch (Exception ex)
            {
                await dialogService.ShowMessageAsync(title: Strings.BackupFailedTitle, message: ex.Message);
            }
        }
        else
        {
            Log.Information("Restore Backup canceled by the user due to newer local data");
        }

        await dialogService.HideLoadingDialogAsync();
    }

    private async Task<bool> ShowOverwriteBackupInfoAsync()
    {
        return await dialogService.ShowConfirmMessageAsync(
            title: Strings.OverwriteTitle,
            message: Strings.OverwriteBackupMessage,
            positiveButtonText: Strings.YesLabel,
            negativeButtonText: Strings.NoLabel);
    }

    private async Task<bool> ShowOverwriteDataInfoAsync()
    {
        return await dialogService.ShowConfirmMessageAsync(
            title: Strings.OverwriteTitle,
            message: Strings.OverwriteDataMessage,
            positiveButtonText: Strings.YesLabel,
            negativeButtonText: Strings.NoLabel);
    }

    private async Task<bool> ShowForceOverrideConfirmationAsync()
    {
        return await dialogService.ShowConfirmMessageAsync(
            title: Strings.ForceOverrideBackupTitle,
            message: Strings.ForceOverrideBackupMessage,
            positiveButtonText: Strings.YesLabel,
            negativeButtonText: Strings.NoLabel);
    }
}
