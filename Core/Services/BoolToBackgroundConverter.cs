using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace File_Manager.Core.Services
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentUser)
            {
                return isCurrentUser ? new SolidColorBrush(Color.FromRgb(225, 225, 225)) : new SolidColorBrush(Color.FromRgb(204, 229, 255));
            }
            return new SolidColorBrush(Color.FromRgb(225, 225, 225));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
