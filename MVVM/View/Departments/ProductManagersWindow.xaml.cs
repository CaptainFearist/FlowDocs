using File_Manager.Entities;
using File_Manager.MVVM.ViewModel;
using File_Manager.MVVM.View.Technician;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace File_Manager
{
    /// <summary>
    /// Логика взаимодействия для ProductManagersWindow.xaml
    /// </summary>
    public partial class ProductManagersWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _departmentId;
        private readonly int _userId;
        public NamesEnter ViewModel { get; set; }
        public ProductManagersWindow(int departmentId, int userId, string firstName, string lastName)
        {
            _departmentId = departmentId;
            _userId = userId;

            var optionsBuilder = new DbContextOptionsBuilder<IT_DepartmentsContext>();
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS; " +
                                            "Initial Catalog=IT_Departments;Integrated Security=True;" +
                                            "MultipleActiveResultSets=True; TrustServerCertificate=True");
            _context = new IT_DepartmentsContext(optionsBuilder.Options);

            ViewModel = new NamesEnter
            {
                FirstName = firstName,
                LastName = lastName
            };

            InitializeComponent();
            DataContext = ViewModel;
            LoadFilesAsync();
        }

        private static readonly Dictionary<string, int> FileTypeMappings = new Dictionary<string, int>
        {
            { ".doc", 1 }, { ".docx", 1 }, { ".pdf", 1 }, { ".txt", 1 }, { ".rtf", 1 },
            { ".xls", 1 }, { ".xlsx", 1 }, { ".ppt", 1 }, { ".pptx", 1 },
            { ".jpg", 2 }, { ".jpeg", 2 }, { ".png", 2 }, { ".gif", 2 },
            { ".bmp", 2 }, { ".tiff", 2 }, { ".mp4", 3 }
        };

        private async Task LoadFilesAsync()
        {
            try
            {
                var files = await _context.DepartmentFiles
                    .Where(df => df.DepartmentId == _departmentId)
                    .Select(df => df.File)
                    .ToListAsync();

                FilesListView.ItemsSource = files.Select(f => new FileInfoViewModel
                {
                    FileName = f.FileName,
                    UploadDate = f.UploadDate
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файлов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFilePath = openFileDialog.FileName;
                var fileName = System.IO.Path.GetFileName(selectedFilePath);
                var fileExtension = System.IO.Path.GetExtension(selectedFilePath).ToLower();
                var uploadDate = DateTime.Now;

                if (FileTypeMappings.TryGetValue(fileExtension, out int fileTypeId))
                {
                    var newFile = new Entities.File
                    {
                        FileName = fileName,
                        FilePath = selectedFilePath,
                        UploadDate = uploadDate,
                        FileTypeId = fileTypeId,
                        UserId = _userId
                    };

                    _context.Files.Add(newFile);
                    await _context.SaveChangesAsync();

                    var departmentFile = new DepartmentFile
                    {
                        DepartmentId = _departmentId,
                        FileId = newFile.FileId
                    };

                    _context.DepartmentFiles.Add(departmentFile);
                    await _context.SaveChangesAsync();

                    await LoadFilesAsync();
                }
                else
                {
                    MessageBox.Show("Неподдерживаемый тип файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListView.SelectedItem is FileInfoViewModel selectedFileInfo)
            {
                var selectedFileName = selectedFileInfo.FileName;

                var selectedFile = await _context.Files
                    .FirstOrDefaultAsync(f => f.FileName == selectedFileName);

                if (selectedFile != null)
                {
                    var departmentFile = await _context.DepartmentFiles
                        .FirstOrDefaultAsync(df => df.FileId == selectedFile.FileId && df.DepartmentId == _departmentId);

                    if (departmentFile != null)
                    {
                        _context.DepartmentFiles.Remove(departmentFile);
                    }

                    _context.Files.Remove(selectedFile);
                    await _context.SaveChangesAsync();
                    await LoadFilesAsync();
                }
                else
                {
                    MessageBox.Show("Файл не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите файл для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FilesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesListView.SelectedItem is FileInfoViewModel selectedFileInfo)
            {
                DownloadFile(selectedFileInfo);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите файл для скачивания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListView.SelectedItem is FileInfoViewModel selectedFileInfo)
            {
                await DownloadFile(selectedFileInfo);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите файл для скачивания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task DownloadFile(FileInfoViewModel selectedFileInfo)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileName == selectedFileInfo.FileName);
            if (file != null)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = selectedFileInfo.FileName,
                        Filter = "All Files (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        try
                        {
                            System.IO.File.Copy(file.FilePath, saveFileDialog.FileName, true);
                            MessageBox.Show("Файл успешно сохранен!", "Скачивание", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Файл не найден в системе. Проверьте путь: " + file.FilePath, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Файл не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchBox.Text.ToLower();

            try
            {
                var filteredFiles = await _context.DepartmentFiles
                    .Where(df => df.DepartmentId == _departmentId)
                    .Select(df => df.File)
                    .Where(file => file.FileName.ToLower().Contains(searchQuery))
                    .ToListAsync();

                var viewModelFiles = filteredFiles.Select(file => new FileInfoViewModel
                {
                    FileName = file.FileName,
                    UploadDate = file.UploadDate
                }).ToList();

                FilesListView.ItemsSource = viewModelFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске файлов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            string telegramChatUrl = "https://t.me/recrent";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = telegramChatUrl,
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Не удалось открыть ссылку в Telegram. Пожалуйста, проверьте подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchHint.Visibility = Visibility.Collapsed;
            }
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchHint.Visibility = Visibility.Visible;
            }
        }

        private void FilesListView_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void FilesListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private async void FilesListView_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
                    var uploadDate = DateTime.Now;

                    if (FileTypeMappings.TryGetValue(fileExtension, out int fileTypeId))
                    {
                        var newFile = new Entities.File
                        {
                            FileName = fileName,
                            FilePath = filePath,
                            UploadDate = uploadDate,
                            FileTypeId = fileTypeId,
                            UserId = _userId
                        };

                        _context.Files.Add(newFile);
                        await _context.SaveChangesAsync();

                        var departmentFile = new DepartmentFile
                        {
                            DepartmentId = _departmentId,
                            FileId = newFile.FileId
                        };

                        _context.DepartmentFiles.Add(departmentFile);
                        await _context.SaveChangesAsync();
                        await LoadFilesAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Файл {fileName} имеет неподдерживаемый формат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double totalWidth = FilesListView.ActualWidth - 25; // Вычитаем небольшие отступы
            if (totalWidth > 0)
            {
                var gridView = FilesListView.View as GridView;
                if (gridView != null && gridView.Columns.Count == 2)
                {
                    double columnWidth = totalWidth / 2; // Делаем оба столбца одинаковыми
                    gridView.Columns[0].Width = columnWidth;
                    gridView.Columns[1].Width = columnWidth;
                }
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
