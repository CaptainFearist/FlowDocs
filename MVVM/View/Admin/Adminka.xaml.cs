using File_Manager.Entities;
using File_Manager.MVVM.View.Technician;
using File_Manager.MVVM.ViewModel;
using File_Manager.MVVM.Model;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.IO;
using File_Manager.MVVM.View.Messenger;

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

        private Run CreateTextRun(string text, bool isBold = false)
        {
            Run run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }); // Preserve нужен для пробелов
            if (isBold)
            {
                RunProperties runProperties = new RunProperties(new Bold());
                run.RunProperties = runProperties;
            }
            return run;
        }

        private Paragraph CreateParagraphWithText(string text, bool isBold = false)
        {
            return new Paragraph(CreateTextRun(text, isBold));
        }

        private void AddTableCell(TableRow row, string text, bool isBold, JustificationValues justification)
        {
            TableCell cell = new TableCell();
            Paragraph paragraph = CreateParagraphWithText(text, isBold);

            // Выравнивание
            ParagraphProperties paragraphProperties = new ParagraphProperties(new Justification() { Val = justification });
            paragraph.ParagraphProperties = paragraphProperties;

            cell.Append(paragraph);
            row.Append(cell);
        }

        private void AddTableCell(TableRow row, string text, bool isBold = false)
        {
            AddTableCell(row, text, isBold, JustificationValues.Center);
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Name == "ReportButton")
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
                DateTime endDateExclusive = EndDatePicker.SelectedDate.Value.Date.AddDays(1);
                DateTime endDateInclusive = endDateExclusive.AddTicks(-1);

                try
                {
                    var files = _context.Files
                        .Where(f => f.UploadDate >= startDate && f.UploadDate < endDateExclusive && f.User != null)
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

                    var userGroups = files
                        .GroupBy(f => new { f.FirstName, f.LastName, f.DepartmentName })
                        .Select(g => new UserReportData
                        {
                            FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                            Department = g.Key.DepartmentName,
                            FileCount = g.Count(),
                        })
                        .OrderByDescending(g => g.FileCount)
                        .ToList();

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = $"Отчет_{startDate:yyyyMMdd}-{endDateInclusive:yyyyMMdd}_{DateTime.Now:yyyyMMddHHmmss}",
                        DefaultExt = ".docx",
                        Filter = "Word документы (.docx)|*.docx|Все файлы (*.*)|*.*",
                        Title = "Сохранить отчет"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(saveDialog.FileName, WordprocessingDocumentType.Document))
                        {
                            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                            mainPart.Document = new Document();
                            Body body = mainPart.Document.AppendChild(new Body());
                            
                            // Заголовок
                            body.Append(CreateParagraphWithText("Отчет по загруженным файлам", isBold: true));
                            body.Append(CreateParagraphWithText(""));
                            
                            // Сводная информация
                            body.Append(CreateParagraphWithText($"Период отчета: {startDate:dd.MM.yyyy} — {endDateInclusive:dd.MM.yyyy}"));
                            body.Append(CreateParagraphWithText($"Всего загружено файлов: {totalFiles}"));
                            body.Append(CreateParagraphWithText(""));
                            
                            // Таблица со статистикой
                            body.Append(CreateParagraphWithText("Статистика по пользователям:", isBold: true));

                            Table table = new Table();

                            TableProperties tblProps = new TableProperties(
                                new TableBorders(
                                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                                ),
                                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
                            );
                            table.AppendChild(tblProps);

                            TableRow headerRow = new TableRow();
                            AddTableCell(headerRow, "Пользователь", isBold: true);
                            AddTableCell(headerRow, "Отдел", isBold: true);
                            AddTableCell(headerRow, "Количество файлов", isBold: true, justification: JustificationValues.Center);
                            table.Append(headerRow);

                            foreach (var user in userGroups)
                            {
                                TableRow dataRow = new TableRow();
                                AddTableCell(dataRow, user.FullName);
                                AddTableCell(dataRow, user.Department);
                                AddTableCell(dataRow, user.FileCount.ToString(), isBold: false, justification: JustificationValues.Center);
                                table.Append(dataRow);
                            }

                            body.Append(table);
                        }

                        MessageBox.Show("Отчет успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при генерации отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (isDatePanelVisible)
                    {
                        ReportDatePanel.Visibility = Visibility.Collapsed;
                        isDatePanelVisible = false;
                        StartDatePicker.SelectedDate = null;
                        EndDatePicker.SelectedDate = null;
                    }
                }
            }
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.UserId == _userId);

            if (currentUser != null)
            {
                ChatsWindow chatsWindow = new ChatsWindow(currentUser);
                chatsWindow.Show();
            }
            else
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
