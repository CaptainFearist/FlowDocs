using File_Manager.Entities;
using System.Windows;
using System.Windows.Input;

namespace File_Manager
{
    public partial class AddUserWindow : Window
    {
        private readonly IT_DepartmentsContext _context;

        public AddUserWindow(IT_DepartmentsContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FirstNameTextBox.Text;
            var lastName = LastNameTextBox.Text;
            var email = EmailTextBox.Text;
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            var departmentIdText = DepartmentIdTextBox.Text;

            if (!string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                int.TryParse(departmentIdText, out int departmentId))
            {
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Username = username,
                    Password = password,
                    DepartmentId = departmentId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                MessageBox.Show("Пользователь добавлен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
