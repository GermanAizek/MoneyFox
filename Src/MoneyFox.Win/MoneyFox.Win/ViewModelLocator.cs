namespace MoneyFox.Win;

using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using ViewModels;
using ViewModels.About;
using ViewModels.Accounts;
using ViewModels.Categories;
using ViewModels.DataBackup;
using ViewModels.Interfaces;
using ViewModels.Payments;
using ViewModels.Settings;
using ViewModels.Statistics;
using ViewModels.Statistics.StatisticCategorySummary;

public class ViewModelLocator
{
    protected ViewModelLocator() { }

    public static ShellViewModel ShellVm => ServiceLocator.Current.GetInstance<ShellViewModel>();

    //*****************
    //  Data Entry
    //*****************

    public static IAccountListViewModel AccountListVm => ServiceLocator.Current.GetInstance<IAccountListViewModel>();

    public static CategoryListViewModel CategoryListVm => ServiceLocator.Current.GetInstance<CategoryListViewModel>();

    public static PaymentListViewModel PaymentListVm => ServiceLocator.Current.GetInstance<PaymentListViewModel>();

    public static SelectCategoryListViewModel SelectCategoryListVm => ServiceLocator.Current.GetInstance<SelectCategoryListViewModel>();

    public static AddAccountViewModel AddAccountVm => ServiceLocator.Current.GetInstance<AddAccountViewModel>();

    internal static AddCategoryViewModel AddCategoryVm => ServiceLocator.Current.GetInstance<AddCategoryViewModel>();

    public static AddPaymentViewModel AddPaymentVm => ServiceLocator.Current.GetInstance<AddPaymentViewModel>();

    public static EditAccountViewModel EditAccountVm => ServiceLocator.Current.GetInstance<EditAccountViewModel>();

    public static EditCategoryViewModel EditCategoryVm => ServiceLocator.Current.GetInstance<EditCategoryViewModel>();

    public static EditPaymentViewModel EditPaymentVm => ServiceLocator.Current.GetInstance<EditPaymentViewModel>();

    public static BackupViewModel BackupVm => ServiceLocator.Current.GetInstance<BackupViewModel>();

    //*****************
    //  Common
    //*****************
    public static SelectDateRangeDialogViewModel SelectDateRangeDialogVm => ServiceLocator.Current.GetInstance<SelectDateRangeDialogViewModel>();

    //*****************
    //  Statistics
    //*****************
    public static StatisticCashFlowViewModel StatisticCashFlowVm => ServiceLocator.Current.GetInstance<StatisticCashFlowViewModel>();

    public static StatisticCategoryProgressionViewModel StatisticCategoryProgressionVm
        => ServiceLocator.Current.GetInstance<StatisticCategoryProgressionViewModel>();

    public static StatisticAccountMonthlyCashflowViewModel StatisticAccountMonthlyCashflowVm
        => ServiceLocator.Current.GetInstance<StatisticAccountMonthlyCashflowViewModel>();

    public static StatisticCategorySpreadingViewModel StatisticCategorySpreadingVm => ServiceLocator.Current.GetInstance<StatisticCategorySpreadingViewModel>();

    public static StatisticCategorySummaryViewModel StatisticCategorySummaryVm => ServiceLocator.Current.GetInstance<StatisticCategorySummaryViewModel>();

    public static StatisticSelectorViewModel StatisticSelectorVm => ServiceLocator.Current.GetInstance<StatisticSelectorViewModel>();

    //*****************
    //  Settings
    //*****************
    public static WindowsSettingsViewModel SettingsVm => ServiceLocator.Current.GetInstance<WindowsSettingsViewModel>();

    public static AboutViewModel AboutVm => ServiceLocator.Current.GetInstance<AboutViewModel>();

    public static void RegisterServices(ContainerBuilder registrations)
    {
        var container = registrations.Build();
        if (container != null)
        {
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
        }
    }
}
