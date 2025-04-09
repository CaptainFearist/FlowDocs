using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace File_Manager.Core.Services
{
    public class SenderNameForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentUser)
            {
                return isCurrentUser ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Color.FromRgb(0, 96, 169));
            }
            return new SolidColorBrush(Color.FromRgb(0, 96, 169));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
