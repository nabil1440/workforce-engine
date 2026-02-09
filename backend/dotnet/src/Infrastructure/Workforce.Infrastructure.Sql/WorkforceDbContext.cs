using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Infrastructure.Sql;

public sealed class WorkforceDbContext : DbContext
{
    public WorkforceDbContext(DbContextOptions<WorkforceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.JoiningDate).IsRequired();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired();
            entity.HasIndex(d => d.Name).IsUnique();
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired();
            entity.HasIndex(d => d.Name).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Status).HasConversion<int>();
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(p => p.EndDate);

            entity.HasMany(p => p.Members)
                .WithOne()
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Tasks)
                .WithOne()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => new { pm.ProjectId, pm.EmployeeId });
            entity.ToTable("ProjectMembers");
        });

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.ToTable("Tasks");
            entity.Property(t => t.Title).IsRequired();
            entity.Property(t => t.Status).HasConversion<int>();
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.Property(t => t.DueDate).IsRequired();
        });
    }
}
