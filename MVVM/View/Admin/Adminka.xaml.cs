using File_Manager.Entities;
using File_Manager.MVVM.View.Technician;
using File_Manager.MVVM.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace File_Manager
{
    public partial class Adminka : Window
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _userId;
        private WindowResizer _windowResizer;
        private bool isDatePanelVisible = false;

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
            _windowResizer = new WindowResizer(this);
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
                    Style = (System.Windows.Style)FindResource("DepartmentButton"),
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

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!isDatePanelVisible)
            {
                ReportDatePanel.Visibility = Visibility.Visible;
                isDatePanelVisible = true;
                return;
            }

            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите обе даты для отчета.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            // Файлы с данными о пользователе и отделе
            var files = _context.Files
                .Where(f => f.UploadDate >= startDate && f.UploadDate <= endDate && f.User != null)
                .Select(f => new
                {
                    f.FileSize,
                    f.User.FirstName,
                    f.User.LastName,
                    DepartmentName = f.User.Department != null ? f.User.Department.DepartmentName : "Не указан"
                })
                .ToList();

            int totalFiles = files.Count;
            long totalSize = files.Sum(f => f.FileSize ?? 0);

            // Группировка по пользователям и отделам
            var userGroups = files
                .GroupBy(f => new { f.FirstName, f.LastName, f.DepartmentName })
                .Select(g => new
                {
                    FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                    Department = g.Key.DepartmentName,
                    FileCount = g.Count(),
                })
                .OrderByDescending(g => g.FileCount)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"📅 Отчет за период: {startDate:dd.MM.yyyy} — {endDate:dd.MM.yyyy}");
            sb.AppendLine($"Всего загружено файлов: {totalFiles}");
            sb.AppendLine();
            sb.AppendLine("📌 Статистика по пользователям:\n");

            foreach (var user in userGroups)
            {
                sb.AppendLine($"👤 {user.FullName} | 📁 {user.FileCount} файлов | 🏢 Отдел: {user.Department}");
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Отчет_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}",
                DefaultExt = ".docx",
                Filter = "Word документы (.docx)|*.docx|Все файлы (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(saveDialog.FileName, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());

                    Body body = mainPart.Document.Body;
                    body.Append(new Paragraph(new Run(new Text($"📅 Отчет за период: {startDate:dd.MM.yyyy} — {endDate:dd.MM.yyyy}"))));
                    body.Append(new Paragraph(new Run(new Text($"Всего загружено файлов: {totalFiles}"))));
                    body.Append(new Paragraph(new Run(new Text(""))));
                    body.Append(new Paragraph(new Run(new Text("📌 Статистика по пользователям:"))));

                    foreach (var user in userGroups)
                    {
                        body.Append(new Paragraph(new Run(new Text($"👤 {user.FullName} | 📁 {user.FileCount} файлов | 🏢 Отдел: {user.Department}"))));
                    }
                }

                MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            ReportDatePanel.Visibility = Visibility.Collapsed;
            isDatePanelVisible = false;
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = _context.Users.Find(_userId);

            if (currentUser != null)
            {
                var profileWindow = new ProfileWindow(currentUser, _context);
                profileWindow.Show();
            }
            else
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationWindow confirmWin = new ConfirmationWindow();
            confirmWin.Owner = this;
            confirmWin.ShowDialog();
        }

    }
}
