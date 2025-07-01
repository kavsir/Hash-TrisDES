using Hash_TrisDES.Models;
using Microsoft.EntityFrameworkCore;

namespace Hash_TrisDES.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    }
}
