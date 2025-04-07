using System.Collections.ObjectModel;

namespace File_Manager.MVVM.Model
{
    public class ChatModel
    {
        public int ChatId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }

        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
    }
}
