using File_Manager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using File_Manager.MVVM.ViewModel;
using System.Windows.Input;
using System.Diagnostics;

namespace File_Manager
{
    public partial class DepartmentWindow : Window
    {
        private readonly IT_DepartmentsContext _context;
        private readonly int _departmentId;
        private readonly int _userId;
        private WindowResizer _windowResizer;
        public string DepartmentName { get; set; }

        public DepartmentWindow(int departmentId, int userId, IT_DepartmentsContext context, string departmentName)
        {
            _departmentId = departmentId;
            _userId = userId;
            _context = context;
            DepartmentName = departmentName;
            InitializeComponent();
            DataContext = this;
            LoadDepartmentFiles();

            this.Closed += (s, e) => _context.Dispose();
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

        private static readonly Dictionary<string, int> FileTypeMappings = new Dictionary<string, int>
        {
            { ".doc", 1 }, { ".docx", 1 }, { ".pdf", 1 }, { ".txt", 1 }, { ".rtf", 1 },
            { ".xls", 1 }, { ".xlsx", 1 }, { ".ppt", 1 }, { ".pptx", 1 },
            { ".jpg", 2 }, { ".jpeg", 2 }, { ".png", 2 }, { ".gif", 2 },
            { ".bmp", 2 }, { ".tiff", 2 }, { ".mp4", 3 }
        };

        private async Task LoadDepartmentFiles()
        {
            try
            {
                var departmentFiles = await _context.DepartmentFiles
                    .Where(df => df.DepartmentId == _departmentId)
                    .Include(df => df.File)
                    .ToListAsync();

                FilesListView.ItemsSource = departmentFiles
                    .Where(df => df.File != null)
                    .Select(df => new FileInfoViewModel
                    {
                        FileName = df.File?.FileName ?? string.Empty,
                        UploadDate = df.File.UploadDate.HasValue ? df.File.UploadDate : null // Explicitly check for HasValue
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавление файла
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
                    try
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(selectedFilePath);
                        long fileSize = fileBytes.Length;

                        var newFile = new Entities.File
                        {
                            FileName = fileName,
                            FileContent = fileBytes,
                            FileSize = fileSize,
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

                        await LoadDepartmentFiles();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при чтении или сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неподдерживаемый тип файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Удаление файла
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
                    await LoadDepartmentFiles();
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

        // Скачивание файла
        private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListView.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите файл для скачивания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FilesListView.SelectedItem is FileInfoViewModel selectedFileInfo)
            {
                DownloadFile(selectedFileInfo);
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

        private async Task DownloadFile(FileInfoViewModel selectedFileInfo)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FileName == selectedFileInfo.FileName);
            if (file == null)
            {
                MessageBox.Show("Файл не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                FileName = selectedFileInfo.FileName,
                Filter = "All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllBytes(saveFileDialog.FileName, file.FileContent);
                    MessageBox.Show("Файл успешно сохранен!", "Скачивание", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Поиск файлов
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

                FilesListView.ItemsSource = filteredFiles.Select(file => new FileInfoViewModel
                {
                    FileName = file.FileName,
                    UploadDate = file.UploadDate
                }).ToList();
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

        private void FilesListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private async void FilesListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in droppedFiles)
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
                    var uploadDate = DateTime.Now;

                    if (FileTypeMappings.TryGetValue(fileExtension, out int fileTypeId))
                    {
                        try
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                            long fileSize = fileBytes.Length;

                            var newFile = new Entities.File
                            {
                                FileName = fileName,
                                FileContent = fileBytes,
                                FileSize = fileSize,
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
                            await LoadDepartmentFiles();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при чтении или сохранении файла {fileName}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Файл {fileName} имеет неподдерживаемый формат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                await LoadDepartmentFiles();
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
    }
}
