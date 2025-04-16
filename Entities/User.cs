﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace File_Manager.Entities;

public partial class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int DepartmentId { get; set; }
    public byte[] ImagePath { get; set; }

    public virtual Department Department { get; set; }
    public virtual ICollection<File> Files { get; set; } = new List<File>();
    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
