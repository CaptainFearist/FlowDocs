using File_Manager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Threading;

namespace File_Manager.MVVM.View.Admin
{
    /// <summary>
    /// Логика взаимодействия для UpdateUserProfile.xaml
    /// </summary>
    public partial class UpdateUserProfile : Window
    {
        private User _currentUser;
        private IT_DepartmentsContext _context;
        private byte[] _newImageBytes;

        public UpdateUserProfile(User user, IT_DepartmentsContext context)
        {
            InitializeComponent();
            _context = context;
            _currentUser = _context.Users.Find(user.UserId);

            if (_currentUser == null)
            {
                MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadUserData();
            LoadUserProfileImage();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadUserData()
        {
            FirstNameTextBlock.Text = _currentUser.FirstName;
            LastNameTextBlock.Text = _currentUser.LastName;
            LoginTextBox.Text = _currentUser.Username;
            EmailTextBox.Text = _currentUser.Email;

            PasswordBoxInvis.Password = "";
            PasswordBoxVis.Text = "";

            PasswordBoxInvis.Visibility = Visibility.Visible;
            PasswordBoxVis.Visibility = Visibility.Collapsed;
        }

        private void LoadUserProfileImage()
        {
            if (_currentUser.ImagePath != null && _currentUser.ImagePath.Length > 0)
            {
                try
                {
                    using (var memoryStream = new System.IO.MemoryStream(_currentUser.ImagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();
                        UserProfileImageBrush.ImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    UserProfileImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение по умолчанию: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserProfileImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _newImageBytes = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                    using (var memoryStream = new System.IO.MemoryStream(_newImageBytes))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();
                        UserProfileImageBrush.ImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _currentUser.FirstName = FirstNameTextBlock.Text.Trim();
            _currentUser.LastName = LastNameTextBlock.Text.Trim();
            _currentUser.Username = LoginTextBox.Text.Trim();
            _currentUser.Email = EmailTextBox.Text.Trim();

            string enteredPassword = PasswordBoxInvis.Password.Trim();
            if (PasswordBoxVis.Visibility == Visibility.Visible)
            {
                enteredPassword = PasswordBoxVis.Text.Trim();
            }

            if (!string.IsNullOrEmpty(enteredPassword))
            {
                _currentUser.Password = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
            }

            try
            {
                _context.Entry(_currentUser).State = EntityState.Modified;

                if (_newImageBytes != null && _newImageBytes.Length > 0)
                {
                    _currentUser.ImagePath = _newImageBytes;
                }

                await _context.SaveChangesAsync();

                StatusTextBlock.Text = "Изменения сохранены.";
                StatusTextBlock.Visibility = Visibility.Visible;
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                await Task.Delay(3000);
                Dispatcher.Invoke(() => this.DialogResult = true);
            }
            catch (DbUpdateException ex)
            {
                StatusTextBlock.Text = $"Ошибка при сохранении изменений: {ex.Message}";
                StatusTextBlock.Visibility = Visibility.Visible;
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Произошла непредвиденная ошибка: {ex.Message}";
                StatusTextBlock.Visibility = Visibility.Visible;
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }

            PasswordBoxInvis.Password = "";
            PasswordBoxVis.Text = "";
            PasswordBoxVis.Visibility = Visibility.Collapsed;
            PasswordBoxInvis.Visibility = Visibility.Visible;
            _newImageBytes = null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (PasswordBoxInvis.Visibility == Visibility.Visible)
            {
                PasswordBoxInvis.Visibility = Visibility.Collapsed;
                PasswordBoxVis.Text = PasswordBoxInvis.Password;
                PasswordBoxVis.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordBoxInvis.Password = PasswordBoxVis.Text;
                PasswordBoxInvis.Visibility = Visibility.Visible;
                PasswordBoxVis.Visibility = Visibility.Collapsed;
            }
        }
    }
}