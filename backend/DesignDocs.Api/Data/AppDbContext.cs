using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DesignDocs.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ReviewParticipant> ReviewParticipants => Set<ReviewParticipant>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentAnchor> CommentAnchors => Set<CommentAnchor>();
    public DbSet<CommentMessage> CommentMessages => Set<CommentMessage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DocMetrics> DocMetrics => Set<DocMetrics>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Document>(e =>
        {
            e.HasIndex(d => d.Status);
            e.HasIndex(d => d.Team);
            e.HasIndex(d => d.UpdatedAt).HasSortOrder(SortOrder.Descending);
            e.HasIndex(d => d.Slug).IsUnique();
            e.HasIndex(d => d.Tags).HasMethod("gin").HasOperators("jsonb_path_ops");
            e.Property(d => d.Tags).HasColumnType("jsonb");
        });

        b.Entity<ReviewParticipant>(e =>
        {
            e.HasIndex(r => new { r.DocumentId, r.UserId, r.Role }).IsUnique();
        });

        b.Entity<Comment>(e =>
        {
            e.HasIndex(c => new { c.DocumentId, c.Status });
            e.HasIndex(c => new { c.DocumentId, c.Severity });
        });

        b.Entity<CommentAnchor>(e =>
        {
            e.HasIndex(a => a.CommentId);
            e.HasIndex(a => a.IsDetached);
        });

        b.Entity<CommentMessage>(e =>
        {
            e.HasIndex(m => m.CommentId);
        });

        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Login).IsUnique();
        });

        b.Entity<DocMetrics>().HasKey(m => m.DocumentId);
    }
}
