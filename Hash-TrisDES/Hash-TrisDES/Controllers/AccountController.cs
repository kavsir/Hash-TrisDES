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
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, SecurityService security,  IEmailService emailService)
        {
            _context = context;
            _security = security;
            _emailService = emailService;
        }   

        [HttpGet]
        public IActionResult Register() => View();

       [HttpPost]
public async Task<IActionResult> Register(
    string username, string password, string confirmPassword,
    string ten, DateTime ngaySinh, string phone, string email)
{
    if (password != confirmPassword)
    {
        ModelState.AddModelError("", "Mật khẩu nhập lại không khớp.");
        return View();
    }

    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (existingUser != null)
    {
        ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
        return View();
    }

    var salt = _security.GenerateSalt();
    var encryptedPassword = _security.EncryptPassword(username, password, salt);

    var user = new User
    {
        Username = username,
        Salt = salt,
        EncryptedPassword = encryptedPassword,
        FailAttempts = 0,
        IsLocked = false,
        IsAdmin = false,
        Ten = ten,
        NgaySinh = DateOnly.FromDateTime(ngaySinh),
        Phone = phone,
        Email = email
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
        public async Task<IActionResult> ChangePassword(
    string username,
    string oldPassword,
    string newPassword,
    string verifyOption,
    string? verifyPhone,
    string? verifyEmail)
        {
            if (verifyOption == "phone" && string.IsNullOrEmpty(verifyPhone))
            {
                ModelState.AddModelError("", "Vui lòng nhập số điện thoại xác minh.");
                return View();
            }
            if (verifyOption == "email" && string.IsNullOrEmpty(verifyEmail))
            {
                ModelState.AddModelError("", "Vui lòng nhập email xác minh.");
                return View();
            }

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


            // ✔️ Nếu xác minh đúng → đổi mật khẩu
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
                return View();
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản bị khóa. Không thể đặt lại mật khẩu.");
                return View();
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError("", "Email của tài khoản này không khả dụng.");
                return View();
            }


            // Sinh token
            string token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Token = token,
                Username = username,
                ExpirationTime = DateTime.Now.AddMinutes(15)
            };
            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Gửi email
            string resetUrl = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            await _emailService.SendEmailAsync(user.Email!, "Đặt lại mật khẩu", $"Nhấn vào đây để đặt lại mật khẩu: {resetUrl}");

            ViewBag.Message = "Liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            var reset = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (reset == null || reset.ExpirationTime < DateTime.Now)
            {
                ViewBag.Message = "❌ Mã xác nhận không hợp lệ hoặc đã hết hạn.";
                return View("ResetPassword", "");
            }

            return View("ResetPassword", token); // truyền token sang view
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp.");
                return View("ResetPassword", token);
            }

            var reset = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (reset == null || reset.ExpirationTime < DateTime.Now)
            {
                ViewBag.Message = "❌ Mã xác nhận không hợp lệ hoặc đã hết hạn.";
                return View("ResetPassword", "");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == reset.Username);
            if (user == null)
            {
                ViewBag.Message = "❌ Không tìm thấy người dùng.";
                return View("ResetPassword", "");
            }

            // Tạo mật khẩu mới
            var newSalt = _security.GenerateSalt();
            user.Salt = newSalt;
            user.EncryptedPassword = _security.EncryptPassword(user.Username, newPassword, newSalt);
            user.FailAttempts = 0;
            user.IsLocked = false;

            // Xoá token đã dùng
            _context.PasswordResetTokens.Remove(reset);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập.";
            return View("ResetPassword", "");
        }


    }
}

