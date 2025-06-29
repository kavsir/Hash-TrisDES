using System.ComponentModel.DataAnnotations;

namespace Hash_TrisDES.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;  // SHA-256(username)

        [Required]
        public string Salt { get; set; } = null!;      // Random salt

        public string? Ten { get; set; }
        public DateOnly NgaySinh { get; set; }
        public string? Phone { get; set; } = null!; 
        public string? Email { get; set; } = null!; 

        [Required]
        public string EncryptedPassword { get; set; } = null!; // Hash + 3DES

        public int FailAttempts { get; set; } = 0;

        public bool IsLocked { get; set; } = false;

        public bool IsAdmin { get; set; } = false;     

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
