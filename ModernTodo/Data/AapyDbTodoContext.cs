using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ModernTodo.Data;

public partial class AapyDbTodoContext : DbContext
{
    public AapyDbTodoContext()
    {
    }

    public AapyDbTodoContext(DbContextOptions<AapyDbTodoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AapyRole> AapyRoles { get; set; }

    public virtual DbSet<AapyTag> AapyTags { get; set; }

    public virtual DbSet<AapyTask> AapyTasks { get; set; }

    public virtual DbSet<AapyToDoList> AapyToDoLists { get; set; }

    public virtual DbSet<AapyUser> AapyUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=AppDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AapyRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AAPY_Rol__3214EC07A5213105");

            entity.ToTable("AAPY_Role");

            entity.HasIndex(e => e.RoleName, "UQ__AAPY_Rol__8A2B6160DDC65B38").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AapyTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AAPY_Tag__3214EC07104A5793");

            entity.ToTable("AAPY_Tags");

            entity.Property(e => e.HexColor)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AapyTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AAPY_Tas__3214EC0704F70179");

            entity.ToTable("AAPY_Task");

            entity.Property(e => e.CompletedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ToDoList).WithMany(p => p.AapyTasks)
                .HasForeignKey(d => d.ToDoListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_ToDoList");

            entity.HasMany(d => d.Tags).WithMany(p => p.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "AapyTaskTag",
                    r => r.HasOne<AapyTag>().WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Fk_Tags"),
                    l => l.HasOne<AapyTask>().WithMany()
                        .HasForeignKey("TaskId")
                        .HasConstraintName("Fk_Task"),
                    j =>
                    {
                        j.HasKey("TaskId", "TagsId").HasName("PK_Tags_Todo");
                        j.ToTable("AAPY_TaskTags");
                    });
        });

        modelBuilder.Entity<AapyToDoList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AAPY_ToD__3214EC07D0A55E5B");

            entity.ToTable("AAPY_ToDoList");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AapyToDoLists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserId");
        });

        modelBuilder.Entity<AapyUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AAPY_Use__3214EC074D716757");

            entity.ToTable("AAPY_User");

            entity.HasIndex(e => e.Mail, "UQ__AAPY_Use__2724B2D14F8C8A92").IsUnique();

            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.RegisteredOn).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasDefaultValue(1);
            entity.Property(e => e.Salt)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.AapyUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
