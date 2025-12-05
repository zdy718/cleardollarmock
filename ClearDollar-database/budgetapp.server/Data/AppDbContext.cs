using Microsoft.EntityFrameworkCore;

namespace BudgetAppCSCE361.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tables
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // AppUser configuration
        b.Entity<AppUser>().HasKey(x => x.UserId);
        b.Entity<AppUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // Transaction configuration
        b.Entity<Transaction>()
            .HasOne(x => x.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Transaction>()
            .HasIndex(x => x.UserId);
        b.Entity<Transaction>()
            .HasIndex(x => x.PostedAt);

        // Tag configuration
        b.Entity<Tag>()
            .HasOne(x => x.User)
            .WithMany(u => u.Tags)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for Tag hierarchy
        b.Entity<Tag>()
            .HasOne(x => x.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction to avoid cycles

        b.Entity<Tag>()
            .HasIndex(x => x.UserId);
        b.Entity<Tag>()
            .HasIndex(x => x.ParentId);

        // Level constraint (1, 2, or 3)
        b.Entity<Tag>()
            .ToTable(t => t.HasCheckConstraint("CK_Tag_Level", "[Level] IN (1, 2, 3)"));

        // TransactionTag configuration (Many-to-Many)
        b.Entity<TransactionTag>()
            .HasKey(x => new { x.TransactionId, x.TagId });

        b.Entity<TransactionTag>()
            .HasOne(tt => tt.Transaction)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TransactionTag>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction to avoid cycles

        b.Entity<TransactionTag>()
            .HasIndex(x => x.TagId);

        // Budget configuration
        b.Entity<Budget>()
            .HasOne(x => x.User)
            .WithMany(u => u.Budgets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Budget>()
            .HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction to avoid cycles

        b.Entity<Budget>()
            .HasIndex(x => x.UserId);
        b.Entity<Budget>()
            .HasIndex(x => x.TagId);
        b.Entity<Budget>()
            .HasIndex(x => x.Month);
    }
}
