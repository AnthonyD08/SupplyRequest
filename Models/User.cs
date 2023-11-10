namespace WebApi.Models
{
    public class User
    {
        public int UserId { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public int RoleId { get; set; } = 1;
        public int BossId { get; set; } = 1;
        public int? AccountantType { get; set;  } = null;
        public bool Active { get; set; } = true;
    }
}
