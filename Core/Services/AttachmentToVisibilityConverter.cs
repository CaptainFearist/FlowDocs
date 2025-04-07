using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File_Manager.MVVM.Model;
using System.Windows.Data;
using System.Windows;

namespace File_Manager.Core.Services
{
    public class AttachmentToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AttachedFileInMessage attachment)
            {
                return !string.IsNullOrEmpty(attachment.FileName) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
