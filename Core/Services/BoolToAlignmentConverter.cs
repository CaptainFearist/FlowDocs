using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace File_Manager.Core.Services
{
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentUser && isCurrentUser)
            {
                return HorizontalAlignment.Right; // Для текущего пользователя
            }
            return HorizontalAlignment.Left; // Для других пользователей
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
