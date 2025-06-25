using Hash_TrisDES.Data;
using Hash_TrisDES.Models;
using Hash_TrisDES.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hash_TrisDES.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SecurityService _security;

        public AccountController(ApplicationDbContext context, SecurityService security)
        {
            _context = context;
            _security = security;
        }   

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            var salt = _security.GenerateSalt();
            var encryptedPassword = _security.EncryptPassword(username, password, salt);

            var user = new User
            {
                Username = username,
                Salt = salt,
                EncryptedPassword = encryptedPassword,
                FailAttempts = 0,
                IsLocked = false,
                IsAdmin = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                AddLog(username, false, "Tài khoản không tồn tại");
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View();
            }

            if (user.IsLocked)
            {
                AddLog(username, false, "Tài khoản bị khóa");
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View();
            }

            var encryptedPassword = _security.EncryptPassword(username, password, user.Salt);
            if (user.EncryptedPassword == encryptedPassword)
            {
                // Đăng nhập thành công
                user.FailAttempts = 0;
                await _context.SaveChangesAsync();
                AddLog(username, true, "Đăng nhập thành công");

                if (user.IsAdmin)
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Sai mật khẩu
                user.FailAttempts += 1;
                if (user.FailAttempts >= 5)
                    user.IsLocked = true;

                await _context.SaveChangesAsync();
                AddLog(username, false, "Sai mật khẩu");

                int remaining = 5 - user.FailAttempts;
                ModelState.AddModelError("", $"Sai mật khẩu. Bạn còn {remaining} lần thử.");
                return View();
            }
        }

        // ===============================
        // ĐỔI MẬT KHẨU
        // ===============================
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản không hợp lệ hoặc đã bị khóa.");
                return View();
            }

            var encryptedOld = _security.EncryptPassword(username, oldPassword, user.Salt);
            if (user.EncryptedPassword != encryptedOld)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View();
            }

            // Mật khẩu cũ đúng, đổi mật khẩu mới
            var newSalt = _security.GenerateSalt();
            user.Salt = newSalt;
            user.EncryptedPassword = _security.EncryptPassword(username, newPassword, newSalt);

            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ===============================
        // Ghi log đăng nhập
        // ===============================
        private void AddLog(string username, bool isSuccess, string message)
        {
            var log = new LoginLog
            {
                Username = username,
                IsSuccess = isSuccess,
                Message = message,
                Timestamp = DateTime.Now
            };

            _context.LoginLogs.Add(log);
            _context.SaveChanges();
        }
    }
}

