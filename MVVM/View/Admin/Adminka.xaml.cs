using File_Manager.Entities;
using File_Manager.MVVM.View.Technician;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace File_Manager
{
    public partial class Adminka : Window
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _userId;

        public Adminka(int userId, string firstName, string lastName)
        {
            InitializeComponent();
            _userId = userId;
            AdminNameTextBlock.Text = $"{firstName} {lastName}";

            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                         "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                         "TrustServerCertificate=True");

            _context = new IT_DepartmentsContext(optionsBuilder.Options);
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            DepartmentsPanel.Children.Clear();
            var departments = _context.Departments.ToList();

            string imagePath = "pack://application:,,,/Images/folder.png";

            foreach (var department in departments)
            {
                var departmentButton = new Button
                {
                    Tag = department.DepartmentId,
                    Margin = new Thickness(5),
                    Style = (Style)FindResource("DepartmentButton"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    MaxWidth = 650,
                    MaxHeight = 130
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                double buttonHeight = departmentButton.MaxHeight;

                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute)),
                    Width = Math.Min(buttonHeight * 0.4, 70),  // 40% от высоты кнопки, но не более 70px
                    Height = Math.Min(buttonHeight * 0.4, 70),
                    Margin = new Thickness(10)
                };

                var departmentTextBlock = new TextBlock
                {
                    Text = department.DepartmentName,
                    FontSize = Math.Min(buttonHeight * 0.15, 24), // 15% от высоты кнопки, но не более 24px
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                Grid.SetColumn(icon, 0);
                Grid.SetColumn(departmentTextBlock, 1);

                grid.Children.Add(icon);
                grid.Children.Add(departmentTextBlock);

                departmentButton.Content = grid;
                departmentButton.Click += DepartmentButton_Click;

                DepartmentsPanel.Children.Add(departmentButton);
            }
        }



        private void DepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int departmentId)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is DepartmentWindow)
                    {
                        window.Close();
                    }
                }

                var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
                optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                            "TrustServerCertificate=True");

                var newContext = new IT_DepartmentsContext(optionsBuilder.Options);

                var department = newContext.Departments.Find(departmentId);
                if (department != null)
                {
                    var departmentWindow = new DepartmentWindow(departmentId, _userId, newContext, department.DepartmentName);
                    departmentWindow.Show();
                }
            }
        }

        private void DepartmentTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is int departmentId)
            {
                var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
                optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                            "TrustServerCertificate=True");

                var newContext = new IT_DepartmentsContext(optionsBuilder.Options);

                var department = newContext.Departments.Find(departmentId);
                if (department != null)
                {
                    var departmentWindow = new DepartmentWindow(departmentId, _userId, newContext, department.DepartmentName);
                    departmentWindow.ShowDialog();
                }
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow(_context);
            addUserWindow.ShowDialog();
        }

        private void ViewAllUsersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is UsersWindow)
                {
                    window.Close();
                }
            }

            var usersWindow = new UsersWindow();
            usersWindow.Show();
        }


        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationWindow confirmWin = new ConfirmationWindow();
            confirmWin.Owner = this;
            confirmWin.ShowDialog();
        }

    }
}
