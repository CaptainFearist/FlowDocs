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
                SolidColorBrush currentUserBackground = new SolidColorBrush(Color.FromRgb(188, 204, 188));
                SolidColorBrush otherUserBackground = new SolidColorBrush(Color.FromRgb(218, 215, 205));

                return isCurrentUser ? currentUserBackground : otherUserBackground;
            }
            return new SolidColorBrush(Color.FromRgb(218, 215, 205));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
