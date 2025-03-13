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

        public DepartmentWindow(int departmentId, int userId, IT_DepartmentsContext context)
        {
            _departmentId = departmentId;
            _userId = userId;
            _context = context;

            InitializeComponent();
            LoadDepartmentFiles();

            this.Closed += (s, e) => _context.Dispose();
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
                var accountingFiles = await _context.DepartmentFiles
                    .Where(df => df.DepartmentId == _departmentId)
                    .Select(df => df.File)
                    .ToListAsync();

                FilesListView.ItemsSource = accountingFiles.Select(f => new FileInfoViewModel
                {
                    FileName = f.FileName,
                    UploadDate = f.UploadDate
                }).ToList();
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
                    // Создание нового файла
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

                    LoadDepartmentFiles(); // Обновление списка файлов
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

        private void DownloadFile(FileInfoViewModel selectedFileInfo)
        {
            var file = _context.Files.FirstOrDefault(f => f.FileName == selectedFileInfo.FileName);
            if (file == null)
            {
                MessageBox.Show("Файл не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                MessageBox.Show("Файл не найден в системе. Проверьте путь: " + file.FilePath, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    System.IO.File.Copy(file.FilePath, saveFileDialog.FileName, overwrite: true);
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
                    }
                    else
                    {
                        MessageBox.Show($"Файл {fileName} имеет неподдерживаемый формат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                await LoadDepartmentFiles();
            }
        }

    }
}
