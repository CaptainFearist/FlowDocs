using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace File_Manager.Core.Services
{
    public class SenderNameForegroundConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentUser)
            {
                SolidColorBrush currentUserColor = new SolidColorBrush(Color.FromRgb(40, 45, 64));

                SolidColorBrush otherUserColor = new SolidColorBrush(Color.FromRgb(45, 49, 66));

                return isCurrentUser ? currentUserColor : otherUserColor;
            }
            return new SolidColorBrush(Color.FromRgb(45, 49, 66));
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
