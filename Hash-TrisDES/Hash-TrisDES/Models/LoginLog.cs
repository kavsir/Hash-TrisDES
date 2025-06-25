using System.ComponentModel.DataAnnotations;

namespace Hash_TrisDES.Models
{
    public class LoginLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        public bool IsSuccess { get; set; }

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
