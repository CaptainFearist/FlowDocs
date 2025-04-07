namespace File_Manager.Entities;

public partial class ChatParticipant
{
    public int ChatParticipantId { get; set; }

    public int ChatId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedDate { get; set; }

    public virtual Chat Chat { get; set; }

    public virtual User User { get; set; }
}
