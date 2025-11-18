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


        
        // Seed 3 folders
        modelBuilder.Entity<Folder>().HasData(
            new Folder { Id = 1, Name = "Work" },
            new Folder { Id = 2, Name = "Personal" },
            new Folder { Id = 3, Name = "Ideas" },
            new Folder { Id = 4, Name = "Done" }
        );

        // Seed 3 notes
        modelBuilder.Entity<Note>().HasData(
            new Note { Id = 1, Title = "Meeting Notes", Content = "Discuss Q1 roadmap", FolderId = 1, OrderIndex = 0 },
            new Note { Id = 2, Title = "Grocery List", Content = "Milk, Eggs, Bread", FolderId = 2, OrderIndex = 0},
            new Note { Id = 3, Title = "App Idea", Content = "Build a note-taking app", FolderId = 3, OrderIndex = 0 }
        );
    }   
    }
}
