using Microsoft.Maui.Controls;
using System.Globalization;

namespace IFF.Converters
{
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            var inverse = parameter is bool boolParam && !boolParam;
            return inverse ? string.IsNullOrEmpty(str) : !string.IsNullOrEmpty(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}