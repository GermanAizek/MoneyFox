﻿namespace MoneyFox.Converter
{

    using System;
    using System.Globalization;
    using ConverterLogic;
    using ViewModels.Payments;
    using Xamarin.Forms;

    public class PaymentAmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var payment = (PaymentViewModel)value;
            if (payment == null)
            {
                return string.Empty;
            }

            return PaymentAmountConverterLogic.GetAmountSign(payment);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
