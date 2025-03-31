using Microsoft.Maui.Controls;
using System.Globalization;

namespace IFF.Converters
{
    public class UsernameStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            if (string.IsNullOrEmpty(status)) return Colors.Transparent;
            if (status == "Логін доступний") return Colors.Green;
            return Colors.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}