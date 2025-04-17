﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace File_Manager.Entities;

public partial class File
{
    public int FileId { get; set; }

    public string FileName { get; set; }

    public byte[] FileContent { get; set; }

    public long? FileSize { get; set; }

    public DateTime? UploadDate { get; set; }

    public int? UserId { get; set; }

    public int FileTypeId { get; set; }

    public virtual ICollection<DepartmentFile> DepartmentFiles { get; set; } = new List<DepartmentFile>();

    public virtual FileType FileType { get; set; }

    public virtual User User { get; set; }


    public virtual ICollection<ChatAttachment> ChatAttachments { get; set; } = new List<ChatAttachment>();
}
