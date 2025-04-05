using System;
using System.Collections.Generic;

namespace File_Manager.Entities;

public partial class ChatAttachment
{
    public int ChatAttachmentId { get; set; }

    public int MessageId { get; set; }

    public int FileId { get; set; }

    public virtual Message Message { get; set; }

    public virtual File File { get; set; }
}