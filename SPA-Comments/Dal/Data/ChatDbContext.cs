using Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Data;

public class ChatDbContext:DbContext
{
    public DbSet<CommentModel> Comments { get; set; }
    public DbSet<UserModel> Users { get; set; }
    public DbSet<FileModel> Files { get; set; }
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var boolToTinyIntConverter = new ValueConverter<bool?, byte?>(
                   v => v.HasValue ? (byte?)(v.Value ? 1 : 0) : null,
                   v => v.HasValue ? (v.Value == 1) : (bool?)null
           );

        modelBuilder.Entity<FileModel>()
            .Property(e => e.IsImage)
            .HasConversion(boolToTinyIntConverter);

        modelBuilder.Entity<CommentModel>()
        .HasOne(c => c.ParentComment)
        .WithMany(c => c.Replies)
        .HasForeignKey(c => c.ParentId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommentModel>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
