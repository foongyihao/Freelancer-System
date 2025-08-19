using CDN.Freelancers.Domain;
using Microsoft.EntityFrameworkCore;

namespace CDN.Freelancers.Infrastructure;

/// <summary>
/// Entity Framework Core <see cref="DbContext"/> managing Freelancer aggregate persistence.
/// </summary>
/// <remarks>
/// Model configuration defines required fields, max lengths, unique indexes and cascade delete
/// behavior for child collections (<see cref="Skillset"/> and <see cref="Hobby"/>).
/// </remarks>
public class FreelancerDbContext : DbContext
{
    /// <summary>
    /// Initializes the context with externally provided options (e.g. provider configuration).
    /// </summary>
    public FreelancerDbContext(DbContextOptions<FreelancerDbContext> options) : base(options) {}

    /// <summary>
    /// Set representing freelancer root entities.
    /// </summary>
    public DbSet<Freelancer> Freelancers => Set<Freelancer>();
    /// <summary>
    /// Set representing skills belonging to freelancers.
    /// </summary>
    public DbSet<Skillset> Skillsets => Set<Skillset>();
    /// <summary>
    /// Set representing hobbies belonging to freelancers.
    /// </summary>
    public DbSet<Hobby> Hobbies => Set<Hobby>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Freelancer entity
        modelBuilder.Entity<Freelancer>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Username).IsRequired().HasMaxLength(100);
            e.Property(f => f.Email).IsRequired().HasMaxLength(200);
            e.Property(f => f.PhoneNumber).HasMaxLength(30);
            e.HasMany(f => f.Skillsets).WithOne().HasForeignKey(s => s.FreelancerId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(f => f.Hobbies).WithOne().HasForeignKey(h => h.FreelancerId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(f => f.Username).IsUnique();
            e.HasIndex(f => f.Email).IsUnique();
        });

        // Configure Skillset entity
        modelBuilder.Entity<Skillset>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
        });
        // Configure Hobby entity
        modelBuilder.Entity<Hobby>(e =>
        {
            e.Property(h => h.Name).IsRequired().HasMaxLength(100);
        });
    }
}
