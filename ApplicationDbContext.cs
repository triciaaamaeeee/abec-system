using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<Archive> Archives => Set<Archive>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BatchHistory> BatchHistories => Set<BatchHistory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseSchedule> CourseSchedules => Set<CourseSchedule>();
    public DbSet<DocumentRequest> DocumentRequests => Set<DocumentRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Admin>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Applicant>(e =>
        {
            e.HasOne(x => x.Course)
                .WithMany(c => c.Applicants)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.ApplicationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.ApplicationStatus).HasConversion<string>();
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasIndex(x => x.ApplicantId).IsUnique();
            e.HasIndex(x => x.SecurityPin).IsUnique();
            e.HasOne(x => x.Applicant)
                .WithOne(a => a.Student)
                .HasForeignKey<Student>(x => x.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Batch)
                .WithMany(b => b.Students)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<BatchHistory>(e =>
        {
            e.HasOne(x => x.Batch)
                .WithMany(b => b.BatchHistories)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ActionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<CourseSchedule>(e =>
        {
            e.HasOne(x => x.Course)
                .WithMany(c => c.CourseSchedules)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Batch)
                .WithMany(b => b.CourseSchedules)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentRequest>(e =>
        {
            e.HasOne(x => x.Student)
                .WithMany(s => s.DocumentRequests)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.RequestDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(x => x.Student)
                .WithMany(s => s.Notifications)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Applicant)
                .WithMany(a => a.Notifications)
                .HasForeignKey(x => x.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Archive>(e =>
        {
            e.HasOne(x => x.Student)
                .WithMany(s => s.Archives)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ArchiveDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserSetting>(e =>
        {
            e.HasIndex(x => x.AdminId).IsUnique();
            e.HasOne(x => x.Admin)
                .WithOne(a => a.UserSetting)
                .HasForeignKey<UserSetting>(x => x.AdminId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
