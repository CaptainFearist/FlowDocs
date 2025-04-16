using System.Windows.Media.Imaging;

namespace File_Manager.MVVM.ViewModel
{
    public class EmployeeViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
        public BitmapImage ProfileImage { get; set; }

        public byte[] ImagePath
        {
            get => null;
            set
            {
                if (value != null && value.Length > 0)
                {
                    using (var memoryStream = new System.IO.MemoryStream(value))
                    {
                        ProfileImage = new BitmapImage();
                        ProfileImage.BeginInit();
                        ProfileImage.CacheOption = BitmapCacheOption.OnLoad;
                        ProfileImage.StreamSource = memoryStream;
                        ProfileImage.EndInit();
                        ProfileImage.Freeze();
                    }
                }
                else
                {
                    ProfileImage = new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
                    ProfileImage.Freeze();
                }
            }
        }
    }
}