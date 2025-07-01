using System.ComponentModel.DataAnnotations;

namespace Hash_TrisDES.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public DateTime ExpirationTime { get; set; }
    }
}
