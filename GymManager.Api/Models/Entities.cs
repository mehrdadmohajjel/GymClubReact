namespace GymManager.Api.Models
{
    public enum Role
    {
        SuperAdmin = 0,
        GymAdmin = 1,
        Trainer = 2,
        Athlete = 3
    }

    public class Gym
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Movement> Movements { get; set; } = new List<Movement>();
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? GymId { get; set; }  // nullable for SuperAdmin account
        public Gym? Gym { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string NationalCode { get; set; } = null!; // username
        public string Phone { get; set; } = "";
        public string PasswordHash { get; set; } = null!;
        public Role Role { get; set; } = Role.Athlete;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Refresh token related
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        // relations
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
        public ICollection<BuffetPurchase> BuffetPurchases { get; set; } = new List<BuffetPurchase>();
        public ICollection<SalaryPayment> SalaryPayments { get; set; } = new List<SalaryPayment>();
    }

    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public enum MembershipType
    {
        SessionBased = 0,
        Monthly = 1
    }

    public class Membership
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public MembershipType Type { get; set; }
        public int RemainingSessions { get; set; } // for session-based
        public DateTime? ExpiresAt { get; set; }   // for monthly
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public decimal Amount { get; set; }
        public bool IsOnline { get; set; } = false;
        public bool IsPaid { get; set; } = false;
        public string? GatewayReference { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Movement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WorkoutPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid TrainerId { get; set; }
        public User Trainer { get; set; } = null!;
        public Guid AthleteId { get; set; }
        public User Athlete { get; set; } = null!;
        public string Title { get; set; } = "";
        public ICollection<WorkoutDay> Days { get; set; } = new List<WorkoutDay>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WorkoutDay
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; } = null!;
        public int DayIndex { get; set; }
        public string MovementName { get; set; } = null!;
        public int Sets { get; set; }
        public int Reps { get; set; }
    }

    public class BuffetItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BuffetPurchase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid ItemId { get; set; }
        public BuffetItem Item { get; set; } = null!;
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Attendance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }

    public class SalaryPayment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Gym Gym { get; set; } = null!;
        public Guid TrainerId { get; set; }
        public User Trainer { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
