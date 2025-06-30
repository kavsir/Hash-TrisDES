using Hash_TrisDES.Data;
using Hash_TrisDES.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hash_TrisDES.Services;

namespace Hash_TrisDES.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SecurityService _security;

        public AdminController(ApplicationDbContext context, SecurityService security)
        {
            _context = context;
            _security = security;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Where(u => !u.IsAdmin) 
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
        //  Mở khóa tài khoản
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

        // Xóa tài khoản
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
        [HttpGet]
        public IActionResult CreateAdmin() => View();

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string username, string password)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing != null)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View();
            }

            var salt = _security.GenerateSalt();
            var encryptedPassword = _security.EncryptPassword(username, password, salt);

            var admin = new User
            {
                Username = username,
                Salt = salt,
                EncryptedPassword = encryptedPassword,
                IsAdmin = true,
                IsLocked = false,
                FailAttempts = 0,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            ViewBag.Message = "✅ Tạo tài khoản admin thành công.";
            return View();
        }

    }
}
