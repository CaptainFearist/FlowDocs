namespace File_Manager.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; }

    public DateTime SentDate { get; set; }

    public virtual Chat Chat { get; set; }

    public virtual User Sender { get; set; }

    public virtual ICollection<ChatAttachment> ChatAttachments { get; set; } = new List<ChatAttachment>();
}
