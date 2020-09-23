﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MoneyFox.Extensions;
using Xamarin.Forms;

namespace MoneyFox.ViewModels.SetupAssistant
{
    public class CategoryIntroductionViewModel : ViewModelBase
    {
        public RelayCommand GoToAddAccountCommand
            => new RelayCommand(async () => await Shell.Current.GoToModalAsync(ViewModelLocator.AddCategoryRoute));

        public RelayCommand NextStepCommand
            => new RelayCommand(async () => await Shell.Current.GoToAsync(ViewModelLocator.AddPaymentRoute));

        public RelayCommand BackCommand
            => new RelayCommand(async () => await Shell.Current.GoToAsync(".."));
    }
}
