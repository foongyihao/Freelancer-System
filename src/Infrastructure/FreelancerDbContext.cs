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
    /// Master skills.
    /// </summary>
    public DbSet<Skillset> Skillsets => Set<Skillset>();
    /// <summary>
    /// Master hobbies.
    /// </summary>
    public DbSet<Hobby> Hobbies => Set<Hobby>();
    /// <summary>
    /// Join table for Freelancer-Skillset (explicit entity)
    /// </summary>
    public DbSet<Freelancer_Skillset> FreelancerSkillsets => Set<Freelancer_Skillset>();
    /// <summary>
    /// Join table for Freelancer-Hobby (explicit entity)
    /// </summary>
    public DbSet<Freelancer_Hobby> FreelancerHobbies => Set<Freelancer_Hobby>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Freelancer entity
        modelBuilder.Entity<Freelancer>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Username).IsRequired().HasMaxLength(100);
            e.Property(f => f.Email).IsRequired().HasMaxLength(200);
            e.Property(f => f.PhoneNumber).HasMaxLength(30);
            e.HasIndex(f => f.Username).IsUnique();
            e.HasIndex(f => f.Email).IsUnique();
        });
        
        // Configure Skillset entity (master)
        modelBuilder.Entity<Skillset>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedOnAdd();
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(s => s.Name).IsUnique();
        });
        // Configure Hobby entity (master)
        modelBuilder.Entity<Hobby>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Id).ValueGeneratedOnAdd();
            e.Property(h => h.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(h => h.Name).IsUnique();
        });

        // Configure explicit join entities
    modelBuilder.Entity<Freelancer_Skillset>(e =>
        {
            e.ToTable("freelancer_skillcet");
            e.HasKey(x => new { x.FreelancerId, x.SkillsetId });
            e.HasOne(x => x.Freelancer)
                .WithMany(f => f.FreelancerSkillsets)
                .HasForeignKey(x => x.FreelancerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Skillset)
                .WithMany()
                .HasForeignKey(x => x.SkillsetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Freelancer_Hobby>(e =>
        {
            e.ToTable("freelancer_hobby");
            e.HasKey(x => new { x.FreelancerId, x.HobbyId });
            e.HasOne(x => x.Freelancer)
                .WithMany(f => f.FreelancerHobbies)
                .HasForeignKey(x => x.FreelancerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Hobby)
                .WithMany()
                .HasForeignKey(x => x.HobbyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
