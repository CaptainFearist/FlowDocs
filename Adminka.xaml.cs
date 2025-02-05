using File_Manager.Entities;
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
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\FEARIST;" +
                                         "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                         "TrustServerCertificate=True");

            _context = new IT_DepartmentsContext(optionsBuilder.Options);
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            DepartmentsPanel.Children.Clear();
            var departments = _context.Departments.ToList();
            string imagePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", "folder.png");

            foreach (var department in departments)
            {
                var departmentButton = new Button
                {
                    Width = 235,
                    Height = 50,
                    Tag = department.DepartmentId,
                    Margin = new Thickness(5),
                    Style = (Style)FindResource("DepartmentButton")
                };

                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath)),
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                var departmentTextBlock = new TextBlock
                {
                    Text = department.DepartmentName,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(icon);
                stackPanel.Children.Add(departmentTextBlock);

                departmentButton.Content = stackPanel;
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
                optionsBuilder.UseSqlServer("Data Source=HoneyPot\\FEARIST;" +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                            "TrustServerCertificate=True");

                var newContext = new IT_DepartmentsContext(optionsBuilder.Options);

                var departmentWindow = new DepartmentWindow(departmentId, _userId, newContext);
                departmentWindow.Show();
            }
        }

        private void DepartmentTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is int departmentId)
            {
                var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
                optionsBuilder.UseSqlServer("Data Source=HoneyPot\\FEARIST;" +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                            "TrustServerCertificate=True");

                var newContext = new IT_DepartmentsContext(optionsBuilder.Options);

                var departmentWindow = new DepartmentWindow(departmentId, _userId, newContext);
                departmentWindow.ShowDialog();
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
    }
}
