using Microsoft.EntityFrameworkCore;
using NotatApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace NotatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Folder> Folders { get; set; }

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<DiaryEntry> DiaryEntries { get; set; }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Note>()
        .HasOne(n => n.Folder)
        .WithMany(f => f.Notes)
        .HasForeignKey(n => n.FolderId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Note>()
        .HasOne<User>() 
        .WithMany()
        .HasForeignKey(n => n.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Folder>()
        .HasOne<User>()
        .WithMany()
        .HasForeignKey(f => f.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<TaskItem>()
        .HasOne<User>() 
        .WithMany()
        .HasForeignKey(t => t.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    
    modelBuilder.Entity<DiaryEntry>()
        .HasOne<User>() 
        .WithMany()
        .HasForeignKey(t => t.UserId)
        .OnDelete(DeleteBehavior.Cascade);    

    modelBuilder.Entity<Folder>().HasData(
        new Folder { Id = 1, Name = "Overdue", UserId = null },
        new Folder { Id = 2, Name = "Work", UserId = null },
        new Folder { Id = 3, Name = "Personal", UserId = null },
        new Folder { Id = 4, Name = "Ideas", UserId = null },
        new Folder { Id = 5, Name = "Done", UserId = null }
    );
}
    }

}