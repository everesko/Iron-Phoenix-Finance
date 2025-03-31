using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace IFF.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid && parameter is string colors)
            {
                var colorArray = colors.Split('|');
                if (colorArray.Length == 2)
                {
                    try
                    {
                        return isValid ? Color.FromHex(colorArray[1]) : Color.FromHex(colorArray[0]);
                    }
                    catch (FormatException)
                    {
                        return Color.FromHex("#808080"); // Сірий за замовчуванням
                    }
                }
            }
            return Color.FromHex("#808080"); // Сірий за замовчуванням
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}