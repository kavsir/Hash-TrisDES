using Hash_TrisDES.Data;
using Hash_TrisDES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hash_TrisDES.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Where(u => !u.IsAdmin) // Không hiển thị admin khác
                .ToListAsync();
            return View(users);
        }
        // khóa tài khoản
        public async Task<IActionResult> Lock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsLocked = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        // ✅ Mở khóa tài khoản
        public async Task<IActionResult> Unlock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsLocked = false;
                user.FailAttempts = 0;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // ✅ Xóa tài khoản
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // ✅ Xem lịch sử đăng nhập
        public async Task<IActionResult> LoginLogs()
        {
            var logs = await _context.LoginLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }
    }
}
