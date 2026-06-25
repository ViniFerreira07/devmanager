using DevManager.Domain.Entities;
using DevManager.Application.Interfaces;
using DevManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DevManager.Infrastructure.Persistence;

public class DevManagerDbContext : DbContext, IAppDbContext
{
    public DevManagerDbContext(DbContextOptions<DevManagerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<ProgrammingLanguage> ProgrammingLanguages => Set<ProgrammingLanguage>();
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<DeveloperProgrammingLanguage> DeveloperProgrammingLanguages => Set<DeveloperProgrammingLanguage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasIndex(s => s.UF).IsUnique();
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.UF).IsRequired().HasMaxLength(2);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.HasOne(c => c.State).WithMany(s => s.Cities).HasForeignKey(c => c.StateId);
        });

        modelBuilder.Entity<ProgrammingLanguage>(entity =>
        {
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasIndex(d => d.Email).IsUnique();
            entity.Property(d => d.Name).IsRequired().HasMaxLength(150);
            entity.Property(d => d.Email).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Seniority).IsRequired().HasMaxLength(80);
            entity.Property(d => d.Observations).HasMaxLength(1000);
            entity.HasOne(d => d.City).WithMany(c => c.Developers).HasForeignKey(d => d.CityId);
            entity.HasMany(d => d.ProgrammingLanguages)
                .WithMany(p => p.Developers)
                .UsingEntity<DeveloperProgrammingLanguage>(
                    right => right.HasOne(x => x.ProgrammingLanguage).WithMany(x => x.DeveloperProgrammingLanguages).HasForeignKey(x => x.ProgrammingLanguageId),
                    left => left.HasOne(x => x.Developer).WithMany(x => x.DeveloperProgrammingLanguages).HasForeignKey(x => x.DeveloperId),
                    join =>
                    {
                        join.ToTable("DeveloperProgrammingLanguages");
                        join.HasKey(x => new { x.DeveloperId, x.ProgrammingLanguageId });
                    });
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "x");
            var property = Expression.Property(parameter, nameof(ISoftDelete.DeletedAt));
            var nullConstant = Expression.Constant(null, typeof(DateTime?));
            var body = Expression.Equal(property, nullConstant);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
