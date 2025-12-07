using GymManager.Api.Models;
using Microsoft.EntityFrameworkCore;


namespace GymManager.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Gym> Gyms { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Membership> Memberships { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Movement> Movements { get; set; } = null!;
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; } = null!;
        public DbSet<WorkoutDay> WorkoutDays { get; set; } = null!;
        public DbSet<BuffetItem> BuffetItems { get; set; } = null!;
        public DbSet<BuffetPurchase> BuffetPurchases { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<SalaryPayment> SalaryPayments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // unique on NationalCode
            builder.Entity<User>()
                .HasIndex(u => u.NationalCode)
                .IsUnique();

            // relations
            builder.Entity<Gym>()
                .HasMany(g => g.Users)
                .WithOne(u => u.Gym)
                .HasForeignKey(u => u.GymId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            builder.Entity<WorkoutPlan>()
                .HasMany(p => p.Days)
                .WithOne(d => d.WorkoutPlan)
                .HasForeignKey(d => d.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // other indexes for common queries
            builder.Entity<Membership>()
                .HasIndex(m => new { m.GymId, m.UserId });

            builder.Entity<Payment>()
                .HasIndex(p => new { p.GymId, p.CreatedAt });
        }
    }
}
