﻿using File_Manager.Entities;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace File_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private IT_DepartmentsContext _context;

        public MainWindow()
        {
            InitializeComponent();

            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                         "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                         "TrustServerCertificate=True");

            _context = new IT_DepartmentsContext(optionsBuilder.Options);
        }

        private async void LoginEnter_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginLogIn.Text.Trim();
            string password = LoginPassInvis.Password.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Введите логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = await _context.Users
                                         .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    int departmentId = user.DepartmentId;
                    int userId = user.UserId;

                    System.Windows.Window nextWindow;

                    switch (departmentId)
                    {
                        case 1:
                            nextWindow = new AccountingWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 2:
                            nextWindow = new TestersWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 3:
                            nextWindow = new DevelopersWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 4:
                            nextWindow = new ProductManagersWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 5:
                            nextWindow = new HRWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 6:
                            nextWindow = new MarketingWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 7:
                            nextWindow = new SalesWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 8:
                            nextWindow = new Adminka(userId, user.FirstName, user.LastName);
                            break;
                        case 9:
                            nextWindow = new DesignWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        case 10:
                            nextWindow = new LegalDepartmentWindow(departmentId, userId, user.FirstName, user.LastName);
                            break;
                        default:
                            MessageBox.Show("Отдел не определен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                    }

                    nextWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (LoginPassInvis.Visibility == Visibility.Visible)
            {
                LoginPassInvis.Visibility = Visibility.Collapsed;
                LoginPassVis.Text = LoginPassInvis.Password;
                LoginPassVis.Visibility = Visibility.Visible;
            }
            else
            {
                LoginPassInvis.Visibility = Visibility.Visible;
                LoginPassVis.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // При наведении мыши показываем пароль
            LoginPassVis.Text = LoginPassInvis.Password;
            LoginPassVis.Visibility = Visibility.Visible;
            LoginPassInvis.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // При уходе мыши скрываем TextBox и показываем PasswordBox
            LoginPassVis.Visibility = Visibility.Collapsed;
            LoginPassInvis.Visibility = Visibility.Visible;
        }

        private void LoginLogIn_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginLogIn.Text))
            {
                LoginLogInHint.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginLogIn_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginLogIn.Text))
            {
                LoginLogInHint.Visibility = Visibility.Visible;
            }
        }

        private void LoginPassInvis_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginPassInvis.Password))
            {
                LoginPassHint.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginPassInvis_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginPassInvis.Password))
            {
                LoginPassHint.Visibility = Visibility.Visible;
            }
        }
    }
}