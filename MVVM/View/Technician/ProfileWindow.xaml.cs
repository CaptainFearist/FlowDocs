using File_Manager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace File_Manager.MVVM.View.Technician
{
    public partial class ProfileWindow : Window
    {
        private User _currentUser;
        private IT_DepartmentsContext _context;
        private string _newImagePath;

        public ProfileWindow(User user, IT_DepartmentsContext context)
        {
            InitializeComponent();
            _currentUser = user;
            _context = context;
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

            PasswordBoxInvis.Password = _currentUser.Password;
            PasswordBoxInvis.Visibility = Visibility.Visible;
            PasswordBoxVis.Text = _currentUser.Password;
            PasswordBoxVis.Visibility = Visibility.Collapsed;
        }

        private void LoadUserProfileImage()
        {
            if (!string.IsNullOrEmpty(_currentUser.ImagePath))
            {
                UserProfileImageBrush.ImageSource = new BitmapImage(new Uri(_currentUser.ImagePath));
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
                _newImagePath = openFileDialog.FileName;
                UserProfileImageBrush.ImageSource = new BitmapImage(new Uri(_newImagePath));
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _currentUser.FirstName = FirstNameTextBlock.Text;
            _currentUser.LastName = LastNameTextBlock.Text;
            _currentUser.Username = LoginTextBox.Text;
            _currentUser.Email = EmailTextBox.Text;

            string password = PasswordBoxVis.Text;
            _currentUser.Password = password;

            try
            {
                _context.Users.Attach(_currentUser);
                _context.Entry(_currentUser).State = EntityState.Modified;

                if (!string.IsNullOrEmpty(_newImagePath))
                {
                    _currentUser.ImagePath = _newImagePath;
                    _context.Entry(_currentUser).Property(u => u.ImagePath).IsModified = true;
                }

                await _context.SaveChangesAsync();

                StatusTextBlock.Text = "Изменения сохранены.";
                StatusTextBlock.Visibility = Visibility.Visible;
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                await Task.Delay(3000);
                Dispatcher.Invoke(() => this.Close());
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

            PasswordBoxVis.Visibility = Visibility.Collapsed;
            PasswordBoxInvis.Visibility = Visibility.Visible;
            _newImagePath = null;
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

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            PasswordBoxVis.Text = PasswordBoxInvis.Password;
            PasswordBoxVis.Visibility = Visibility.Visible;
            PasswordBoxInvis.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            PasswordBoxVis.Visibility = Visibility.Collapsed;
            PasswordBoxInvis.Visibility = Visibility.Visible;
        }
    }
}