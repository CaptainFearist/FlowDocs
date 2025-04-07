namespace File_Manager.MVVM.Model
{
    public class MessageModel
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsSenderCurrentUser { get; set; }
    }

}
