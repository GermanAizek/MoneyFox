﻿namespace MoneyFox.Views.Statistics
{

    using System;
    using Popups;
    using ViewModels.Statistics;
    using Xamarin.CommunityToolkit.Extensions;
    using Xamarin.Forms;

    public partial class StatisticCashFlowPage
    {
        public StatisticCashFlowPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.StatisticCashFlowViewModel;
            ViewModel.LoadedCommand.Execute(null);
        }

        private StatisticCashFlowViewModel ViewModel => (StatisticCashFlowViewModel)BindingContext;

        private void OpenFilterDialog(object sender, EventArgs e)
        {
            var popup = new DateSelectionPopup(dateFrom: ViewModel.StartDate, dateTo: ViewModel.EndDate);
            Shell.Current.ShowPopup(popup);
        }
    }

}
