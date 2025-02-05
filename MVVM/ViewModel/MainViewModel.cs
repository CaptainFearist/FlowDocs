using File_Manager.Core;
using File_Manager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace File_Manager.MVVM.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileInfoViewModel> _files;
        public ObservableCollection<FileInfoViewModel> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        private readonly IT_DepartmentsContext _context;
        private readonly int _departmentId;

        public ICommand SortByDateAscendingCommand { get; }
        public ICommand SortByDateDescendingCommand { get; }

        public MainViewModel(IT_DepartmentsContext context, int departmentId)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _departmentId = departmentId;
            Files = new ObservableCollection<FileInfoViewModel>();

            SortByDateAscendingCommand = new RelayCommand(_ => SortByDateAscending());
            SortByDateDescendingCommand = new RelayCommand(_ => SortByDateDescending());

            LoadFiles();
        }

        public async void LoadFiles()
        {
            try
            {
                var files = await _context.DepartmentFiles
                    .Where(df => df.DepartmentId == _departmentId)
                    .Select(df => df.File)
                    .ToListAsync();

                Files.Clear();

                if (files != null && files.Any())
                {
                    foreach (var f in files)
                    {
                        Files.Add(new FileInfoViewModel
                        {
                            FileName = f.FileName,
                            UploadDate = f.UploadDate,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public void SortByDateAscending()
        {
            var sortedFiles = Files.OrderBy(f => f.UploadDate ?? DateTime.MinValue).ToList();
            Files.Clear();
            foreach (var file in sortedFiles)
            {
                Files.Add(file);
            }
        }

        public void SortByDateDescending()
        {
            var sortedFiles = Files.OrderByDescending(f => f.UploadDate ?? DateTime.MaxValue).ToList();
            Files.Clear();
            foreach (var file in sortedFiles)
            {
                Files.Add(file);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
