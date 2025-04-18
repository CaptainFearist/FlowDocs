﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Microsoft.EntityFrameworkCore;

namespace File_Manager.Entities;

public partial class IT_DepartmentsContext : DbContext
{
    public IT_DepartmentsContext(DbContextOptions<IT_DepartmentsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentFile> DepartmentFiles { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<FileType> FileTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<ChatParticipant> ChatParticipants { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<ChatAttachment> ChatAttachments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=HoneyPot\\SQLEXPRESS;" +
                                        "Initial Catalog=IT_Departments;Integrated Security=True;MultipleActiveResultSets=True;" +
                                        "TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD51F9F984");
            entity.HasIndex(e => e.DepartmentName, "UQ__Departme__D949CC34B1C3D426").IsUnique();
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DepartmentFile>(entity =>
        {
            entity.HasKey(e => e.DepartmentFileId).HasName("PK__Departme__2F8053D834E41377");
            entity.Property(e => e.DepartmentFileId).HasColumnName("DepartmentFileID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.FileId).HasColumnName("FileID");
            entity.HasOne(d => d.Department).WithMany(p => p.DepartmentFiles)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departmen__Depar__4316F928");
            entity.HasOne(d => d.File).WithMany(p => p.DepartmentFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departmen__FileI__440B1D61");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Files__6F0F989FB5D12E75");
            entity.Property(e => e.FileId).HasColumnName("FileID");
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FileContent)
                .IsRequired()
                .HasColumnType("varbinary(max)");
            entity.Property(e => e.FileTypeId).HasColumnName("FileTypeID");
            entity.Property(e => e.UploadDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.HasOne(d => d.FileType).WithMany(p => p.Files)
                .HasForeignKey(d => d.FileTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Files__FileTypeI__45F365D3");
            entity.HasOne(d => d.User).WithMany(p => p.Files)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Files__UserID__44FF419A");
        });

        modelBuilder.Entity<FileType>(entity =>
        {
            entity.HasKey(e => e.FileTypeId).HasName("PK__FileType__0896767E5CE82497");
            entity.HasIndex(e => e.FileTypeName, "UQ__FileType__2A63AD845B032546").IsUnique();
            entity.Property(e => e.FileTypeId).HasColumnName("FileTypeID");
            entity.Property(e => e.FileTypeName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC8E7EB540");
            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4ECE15E2A").IsUnique();
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ImagePath)
                .HasColumnType("varbinary(max)");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__Departmen__46E78A0C");

            entity.HasMany(u => u.ChatParticipants).WithOne(cp => cp.User).HasForeignKey(cp => cp.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatParticipants_Users");
            entity.HasMany(u => u.Messages).WithOne(m => m.Sender).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Messages_Users");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId);
            entity.Property(e => e.ChatName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasMany(c => c.ChatParticipants).WithOne(cp => cp.Chat).HasForeignKey(cp => cp.ChatId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatParticipants_Chats");
            entity.HasMany(c => c.Messages).WithOne(m => m.Chat).HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Messages_Chats");
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(e => e.ChatParticipantId);
            entity.HasIndex(e => new { e.ChatId, e.UserId }, "UQ_ChatParticipants").IsUnique();
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(cp => cp.Chat).WithMany(c => c.ChatParticipants).HasForeignKey(cp => cp.ChatId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatParticipants_Chats");
            entity.HasOne(cp => cp.User).WithMany(u => u.ChatParticipants).HasForeignKey(cp => cp.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatParticipants_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.Content)
                .HasColumnType("text");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(m => m.Chat).WithMany(c => c.Messages).HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Messages_Chats");
            entity.HasOne(m => m.Sender).WithMany(u => u.Messages).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Messages_Users");
            entity.HasMany(m => m.ChatAttachments).WithOne(ca => ca.Message).HasForeignKey(ca => ca.MessageId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatAttachments_Messages");
        });

        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(e => e.ChatAttachmentId);

            entity.HasOne(ca => ca.Message).WithMany(m => m.ChatAttachments).HasForeignKey(ca => ca.MessageId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ChatAttachments_Messages");
            entity.HasOne(ca => ca.File).WithMany(f => f.ChatAttachments).HasForeignKey(ca => ca.FileId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_ChatAttachments_Files");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
