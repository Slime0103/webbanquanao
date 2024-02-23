
using Microsoft.AspNetCore.Mvc;
using REALLY9.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using PagedList.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using REALLY9.Helper;
using REALLY9.Extension;
using REALLY9.ModelViews;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Humanizer;
using NToastNotify;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using REALLY9.Services;

namespace REALLY9.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly Really9Context _context;
        private readonly IEmailService _emailService;
        private readonly IToastNotification toastNotification;
        public AccountsController(Really9Context context, IToastNotification toastNotification, IEmailService emailService)
        {
            _context = context;
            this.toastNotification = toastNotification;
            this._emailService = emailService;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Phone:" + Phone + " has been used");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email:" + Email + " has been used");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsDonHang = _context.Orders.Include(x => x.TransactStatus)
                                       .AsNoTracking()
                                       .Where(x => x.CustomerId == khachhang.CustomerId)
                                       .OrderByDescending(x => x.Orderdate)
                                       .ToList();
                    ViewBag.DonHang = lsDonHang; 
                    return View(khachhang);
                }
            }

            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangKyTaiKhoan()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangKyTaiKhoan(RegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_context.Customers.Any(u => u.Email == taikhoan.Email))
                    {
                        return BadRequest("Email này đã được sử dụng");
                    }

                    string salt = Utilities.GetRandomKey();
                    string verificationToken = CreateRandomToken(); // Tạo token xác nhận

                    Customer khachhang = new Customer()
                    {
                        FullName = taikhoan.FullName,
                        Birthday = taikhoan.Birthday,
                        Address = taikhoan.Address,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = false, // Tài khoản không được kích hoạt từ đầu
                        Salt = salt,
                        VerificationToken = verificationToken, // Lưu token xác nhận
                        CreateDate = DateTime.Now,
                    };

                    _context.Add(khachhang);
                    await _context.SaveChangesAsync();

                    // Gửi email xác nhận
                    string verificationUrl = Url.Action("VerifyEmail", "Accounts", new { token = verificationToken }, Request.Scheme);
                    string emailBody = $"Vui lòng xác nhận email của bạn bằng cách nhấp vào liên kết này: <a href=\"{verificationUrl}\">Xác nhận Email</a>";
                    await _emailService.SendEmailAsync(khachhang.Email, "Xác nhận email của bạn", emailBody);

                    toastNotification.AddSuccessToastMessage("Đăng ký thành công. Vui lòng kiểm tra email của bạn để xác nhận.");
                    return RedirectToAction("Login", "Accounts");
                }

                return View(taikhoan);
            }
            catch
            {
                // Xử lý lỗi
                return View(taikhoan);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var customer = _context.Customers.SingleOrDefault(u => u.VerificationToken == token);
            if (customer == null)
            {
                
                return View("Error"); 
            }

            customer.Active = true;
            customer.VerificationToken = null; 
            await _context.SaveChangesAsync();

          
            return RedirectToAction("Login", "Accounts");
        }

        public string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {

                return RedirectToAction("Dashboard", "Accounts");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl = null)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    if (khachhang == null) return RedirectToAction("DangKyTaiKhoan");

                    string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
                    if (khachhang.Password != pass)
                    {
                        toastNotification.AddErrorToastMessage("Sai thông tin đăng nhập");
                        return View(customer);
                    }

                    if (khachhang.Active == false)
                    {
                        toastNotification.AddErrorToastMessage("Vui long xac nhan email");

                        return RedirectToAction("Login", "Accounts");
                    }
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, khachhang.FullName),
                new Claim("CustomerId", khachhang.CustomerId.ToString())
            };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    toastNotification.AddSuccessToastMessage("SIGN IN SUCCESS");
                    await HttpContext.SignInAsync(claimsPrincipal);
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {
                return RedirectToAction("DangKyTaiKhoan", "Accounts");

            }
            return View(customer);


        }


        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            toastNotification.AddSuccessToastMessage("LOGOUT SUCCESS");
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "Accounts");

                    var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    if (pass == taikhoan.Password)
                    {
                        string passnew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                        taikhoan.Password = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                }

            }
            catch
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return RedirectToAction("Dashboard", "Accounts");
        }


        [AllowAnonymous]
        [Route("quen-mat-khau.html", Name = "ChangePassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("quen-mat-khau.html", Name = "QuenMatKhau")]
        public async Task<IActionResult> ForgotPassword(ForgotPassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Customers.SingleOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            var newPassword = new Random().Next(100000, 999999).ToString();
            var encryptedPassword = (newPassword + user.Salt.Trim()).ToMD5();

            user.Password = encryptedPassword;
            _context.Update(user);
            await _context.SaveChangesAsync();

            var subject = "Doi mat khau";
            var message = $"Sau đây là mật khẩu mới của bạn: {newPassword}";
            await _emailService.SendEmailAsync(model.Email, subject, message);

            return RedirectToAction("Login", "Accounts");
        }

        [HttpGet]
        [Route("api/getCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                                          .Select(c => new { c.CustomerId, c.FullName })
                                          .ToListAsync();
            return Ok(customers);
        }

        //public async Task GenerateForgotPassWordTokenAysnc(ApplicationBuilder app)
        //{

        //}
    }
}