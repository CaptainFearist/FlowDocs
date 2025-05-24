using File_Manager.Entities;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace File_Manager
{
    public partial class MainWindow : System.Windows.Window
    {
        private IT_DepartmentsContext _context;
        private DispatcherTimer errorTimer;

        public MainWindow()
        {
            InitializeComponent();

            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                         "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                         "TrustServerCertificate=True");

            _context = new IT_DepartmentsContext(optionsBuilder.Options);

            errorTimer = new DispatcherTimer();
            errorTimer.Interval = TimeSpan.FromSeconds(5);
            errorTimer.Tick += ErrorTimer_Tick;
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

        private async void LoginEnter_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginLogIn.Text.Trim();
            string password = LoginPassInvis.Password.Trim();

            bool hasError = false;

            if (string.IsNullOrEmpty(username))
            {
                LoginError.Visibility = Visibility.Visible;
                LoginError.Text = "Введите логин";
                hasError = true;
            }
            else
            {
                LoginError.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordError.Visibility = Visibility.Visible;
                PasswordError.Text = "Введите пароль";
                hasError = true;
            }
            else
            {
                PasswordError.Visibility = Visibility.Collapsed;
            }

            if (hasError)
            {
                errorTimer.Stop();
                errorTimer.Start();
                return;
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(password, user.Password))
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
                        LoginError.Text = "Неверный логин или пароль";
                        LoginError.Visibility = Visibility.Visible;
                        PasswordError.Visibility = Visibility.Collapsed;
                        errorTimer.Stop();
                        errorTimer.Start();
                    }
                }
                else
                {
                    LoginError.Text = "Неверный логин или пароль";
                    LoginError.Visibility = Visibility.Visible;
                    PasswordError.Visibility = Visibility.Collapsed;
                    errorTimer.Stop();
                    errorTimer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ErrorTimer_Tick(object sender, EventArgs e)
        {
            LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            errorTimer.Stop();
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
            LoginPassVis.Text = LoginPassInvis.Password;
            LoginPassVis.Visibility = Visibility.Visible;
            LoginPassInvis.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
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