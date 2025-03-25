using System.Collections.ObjectModel;
using System.ComponentModel;

namespace File_Manager.MVVM.ViewModel
{
    public class NamesEnter : INotifyPropertyChanged
    {
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        private ObservableCollection<FileItem> _files;
        public ObservableCollection<FileItem> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public NamesEnter()
        {
            Files = new ObservableCollection<FileItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FileItem
    {
        public string FileName { get; set; }
        public string UploadDate { get; set; }
    }
}
