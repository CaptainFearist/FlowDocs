﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace File_Manager.Entities;

public partial class DepartmentFile
{
    public int DepartmentFileId { get; set; }

    public int DepartmentId { get; set; }

    public int FileId { get; set; }

    public virtual Department Department { get; set; }

    public virtual File File { get; set; }
}
