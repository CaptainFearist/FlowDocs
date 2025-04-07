namespace File_Manager.MVVM.ViewModel
{
    public class MessageViewModel
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }

        public bool IsOwnMessage { get; set; }
    }

}
