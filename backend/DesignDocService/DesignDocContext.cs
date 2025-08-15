using DesignDocService.Models;
using Microsoft.EntityFrameworkCore;

namespace DesignDocService
{
    /// <summary>
    /// Entity Framework Core context for design documents and comments.
    /// Configures entities and indexes for efficient querying.
    /// </summary>
    public class DesignDocContext(DbContextOptions<DesignDocContext> options) : DbContext(options)
    {
        public DbSet<DesignDocument> DesignDocuments => Set<DesignDocument>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure enumeration conversions to strings
            modelBuilder.Entity<DesignDocument>()
                .Property(d => d.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Comment>()
                .Property(c => c.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Comment>()
                .Property(c => c.Severity)
                .HasConversion<string>();

            // Configure relationships
            modelBuilder.Entity<DesignDocument>()
                .HasMany(d => d.Comments)
                .WithOne()
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for filtering
            modelBuilder.Entity<DesignDocument>()
                .HasIndex(d => d.Team);
            modelBuilder.Entity<DesignDocument>()
                .HasIndex(d => d.Product);
            modelBuilder.Entity<DesignDocument>()
                .HasIndex(d => d.Author);

            base.OnModelCreating(modelBuilder);
        }
    }
}