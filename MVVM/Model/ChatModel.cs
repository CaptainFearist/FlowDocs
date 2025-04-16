using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace File_Manager.MVVM.Model
{
    public class ChatModel
    {
        public int ChatId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public BitmapImage ProfileImage { get; set; }

        public byte[] ImagePath
        {
            get => null;
            set
            {
                if (value != null && value.Length > 0)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream(value))
                        {
                            ProfileImage = new BitmapImage();
                            ProfileImage.BeginInit();
                            ProfileImage.CacheOption = BitmapCacheOption.OnLoad;
                            ProfileImage.StreamSource = memoryStream;
                            ProfileImage.EndInit();
                            ProfileImage.Freeze();
                        }
                    }
                    catch (Exception ex)
                    {
                        ProfileImage = new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
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

        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
    }
}