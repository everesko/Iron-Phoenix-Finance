﻿using Microsoft.Maui.Controls;

namespace IFF.Converters
{
    internal class RadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
                return parameter?.ToString();
            return null;
        }
    }
}
