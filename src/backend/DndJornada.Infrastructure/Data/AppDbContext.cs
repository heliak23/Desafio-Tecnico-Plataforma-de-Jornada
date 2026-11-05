using DndJornada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TimeRecord> TimeRecords => Set<TimeRecord>();
    public DbSet<TimeRecordAdjustment> TimeRecordAdjustments => Set<TimeRecordAdjustment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Name).IsRequired().HasMaxLength(150);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.Role).IsRequired();
            builder.Property(u => u.DefaultTimezoneId).IsRequired().HasMaxLength(50);
            builder.Property(u => u.DailyWorkHours).HasPrecision(4, 2).HasDefaultValue(8.00m);
            builder.Property(u => u.Department).HasMaxLength(100);

            builder.HasOne(u => u.Manager)
                .WithMany(m => m.Subordinates)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TimeRecord Configuration
        modelBuilder.Entity<TimeRecord>(builder =>
        {
            builder.ToTable("time_records");
            builder.HasKey(tr => tr.Id);
            builder.Property(tr => tr.TimestampUtc).IsRequired();
            builder.Property(tr => tr.TimezoneId).IsRequired().HasMaxLength(50);
            builder.Property(tr => tr.OffsetMinutes).IsRequired();
            builder.Property(tr => tr.Type).IsRequired();
            builder.Property(tr => tr.Source).IsRequired();
            builder.Property(tr => tr.LocationNotes).HasMaxLength(255);

            builder.HasOne(tr => tr.User)
                .WithMany(u => u.TimeRecords)
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice composto para busca rápida de registros por usuário e data
            builder.HasIndex(tr => new { tr.UserId, tr.TimestampUtc });
        });

        // TimeRecordAdjustment Configuration
        modelBuilder.Entity<TimeRecordAdjustment>(builder =>
        {
            builder.ToTable("time_record_adjustments");
            builder.HasKey(tra => tra.Id);
            builder.Property(tra => tra.TargetDate).IsRequired();
            builder.Property(tra => tra.RequestedType).IsRequired();
            builder.Property(tra => tra.RequestedTimestampUtc).IsRequired();
            builder.Property(tra => tra.TimezoneId).IsRequired().HasMaxLength(50);
            builder.Property(tra => tra.OffsetMinutes).IsRequired();
            builder.Property(tra => tra.Reason).IsRequired().HasMaxLength(500);
            builder.Property(tra => tra.Status).IsRequired();
            builder.Property(tra => tra.ReviewNotes).HasMaxLength(500);

            builder.HasOne(tra => tra.User)
                .WithMany(u => u.Adjustments)
                .HasForeignKey(tra => tra.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tra => tra.ReviewedBy)
                .WithMany()
                .HasForeignKey(tra => tra.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(tra => new { tra.UserId, tra.Status });
        });
    }
}

