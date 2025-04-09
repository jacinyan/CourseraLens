using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<CoursesTags> CoursesTags =>
        Set<CoursesTags>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CoursesTags>()
            .HasKey(r => new { r.CourseId, r.TagId });

        modelBuilder.Entity<CoursesTags>()
            .HasOne(x => x.Course)
            .WithMany(y =>
                y.CoursesTags)
            .HasForeignKey(f =>
                f.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoursesTags>()
            .HasOne(o => o.Tag)
            .WithMany(m =>
                m.CoursesTags)
            .HasForeignKey(f =>
                f.TagId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}