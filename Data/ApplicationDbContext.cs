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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //First identity

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Folder)
                .WithMany(f => f.Notes)
                .HasForeignKey(n => n.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
            .HasKey(n => n.Id);



            modelBuilder.Entity<Note>()
           .Property(n => n.Id)
           .ValueGeneratedOnAdd();


            modelBuilder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);


            // Seed 3 folders
            modelBuilder.Entity<Folder>().HasData(
                new Folder { Id = 1, Name = "Work" },
                new Folder { Id = 2, Name = "Personal" },
                new Folder { Id = 3, Name = "Ideas" },
                new Folder { Id = 4, Name = "Done" }
            );

        
        }
    }
}
