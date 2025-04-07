namespace File_Manager.MVVM.ViewModel
{
    public class FileViewModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; internal set; }
    }
}
