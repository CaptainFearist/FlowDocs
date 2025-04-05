using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace File_Manager.Entities;

public partial class Chat
{
    public int ChatId { get; set; }

    public string ChatName { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}