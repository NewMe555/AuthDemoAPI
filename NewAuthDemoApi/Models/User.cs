namespace AuthDemoApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        //public string RefreshToken { get; set; } = null!;
        //public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? LastLoginAt { get; set; } // <-- Add this
    }
}
