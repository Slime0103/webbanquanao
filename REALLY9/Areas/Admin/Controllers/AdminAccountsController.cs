using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using PagedList;
using PagedList.Mvc;
using REALLY9.Extension;
using REALLY9.Helper;
using REALLY9.Models;
using REALLY9.ModelViews;

namespace REALLY9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    public class AdminAccountsController : Controller
    {
        private readonly Really9Context _context;
        private readonly IToastNotification toastNotification;

        public AdminAccountsController(Really9Context context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: Admin/AdminAccounts
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1; // Nếu không có trang được cung cấp, mặc định là trang 1
            var pageSize = 10; // Số lượng bản ghi trên mỗi trang

            var accounts = await _context.Accounts.AsNoTracking().ToListAsync();

            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description");
            List<SelectListItem> lsTrangThai = new List<SelectListItem>
    {
        new SelectListItem { Text = "Active", Value = "true" },
        new SelectListItem { Text = "Block", Value = "false" }
    };
            ViewData["lsTrangThai"] = lsTrangThai;

            var pagedAccounts = accounts.ToPagedList(pageNumber, pageSize);

            return View(pagedAccounts);
        }
        //public async Task<IActionResult> Index()
        //{
        //    var accounts = await _context.Accounts.AsNoTracking().ToListAsync();

        //    ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description");
        //    List<SelectListItem> lsTrangThai = new List<SelectListItem>();
        //    lsTrangThai.Add(new SelectListItem() { Text = "Active", Value = "true" });
        //    lsTrangThai.Add(new SelectListItem() { Text = "Block", Value = "false" });
        //    ViewData["lsTrangThai"] = lsTrangThai;

        //    return _context.Accounts != null ?
        //                  View(await _context.Accounts.ToListAsync()) :
        //                  Problem("Entity set 'Really9Context.Accounts'  is null.");
        //}
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
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
                var taikhoan = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (taikhoan != null)
                    return Json(data: "Email:" + Email + " has been used");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [Route("tai-khoan-admin-cua-toi.html", Name = "AdminDashboard")]
        public IActionResult AdminDashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.AccountId == Convert.ToInt32(taikhoanID));
                
            }

            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("admin-dang-ky.html", Name = "AdminDangKy")]
        public IActionResult AdminDangKyTaiKhoan()
        {
            ViewData["Roles"] = new SelectList(_context.Roles, "RoleName", "RoleName");
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("admin-dang-ky.html", Name = "AdminDangKy")]
        public async Task<IActionResult> AdminDangKyTaiKhoan(AdminRegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var role = _context.Roles.FirstOrDefault(r => r.RoleName == taikhoan.RoleName);
                    if (role == null)
                    {
                        toastNotification.AddErrorToastMessage("Sai thong tin role");
                        return View(taikhoan);
                    }
                    string salt = Utilities.GetRandomKey();
                    Account admintaikhoan = new Account()
                    {
                        Fullname = taikhoan.FullName,
                       
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt,
                        CreateDate = DateTime.Now,
                        RoleId = role.RoleId,

                    };
                    try
                    {
                        _context.Add(admintaikhoan);
                        toastNotification.AddSuccessToastMessage("SIGN UP SUCCESS");
                        await _context.SaveChangesAsync();
                        // Save Session
                        HttpContext.Session.SetString("AccountId", admintaikhoan.AccountId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("AccountId");
                        //Identity
                        var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, admintaikhoan.Fullname),
                            new Claim("AccountId",admintaikhoan.AccountId.ToString())

                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        // var ShoppingCart = GioHang;
                        // if (ShoppingCart.Count > 0) return RedirectToAction("Shipping", "Checkout");
                        return RedirectToAction("Index", "AdminAccounts");
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("AdminDangKyTaiKhoan", "AdminAccounts");
                    }
                }
                else
                {
                    
                    return View(taikhoan);
                }
            }
            catch
            {
                
                return View(taikhoan);
            }

        }

        [AllowAnonymous]
        [Route("admin-dang-nhap.html", Name = "AdminDangNhap")]
        public IActionResult AdminLogin(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null)
            {

                return RedirectToAction("Index", "AdminAccounts");
            }
            ViewData["Roles"] = new SelectList(_context.Roles, "RoleName", "RoleName");
            return View();
        }




        [HttpPost]
        [AllowAnonymous]
        [Route("admin-dang-nhap.html", Name = "AdminDangNhap")]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel account, string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(account.UserName);
                    if (!isEmail) return View(account);

                    var khachhang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == account.UserName);
                    if (khachhang == null) return RedirectToAction("AdminDangKyTaiKhoan");

                    string pass = (account.Password + khachhang.Salt.Trim()).ToMD5();
                    if (khachhang.Password != pass)
                    {
                        toastNotification.AddErrorToastMessage("Sai thông tin đăng nhập");
                        return View(account);
                    }

                    if (khachhang.Active == false)
                    {
                        return RedirectToAction("ThongBao", "AdminAccounts");
                    }

                    var role = _context.Roles.FirstOrDefault(r => r.RoleId == khachhang.RoleId);
                    if (role != null && role.RoleName == account.RoleName)
                    {
                        
                        HttpContext.Session.SetString("AccountId", khachhang.AccountId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("AccountId");

                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, khachhang.Fullname),
                     new Claim(ClaimTypes.Role, role.RoleName),
                    new Claim("AccountId", khachhang.AccountId.ToString())
                };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        
                        toastNotification.AddErrorToastMessage("Vai trò không phù hợp");
                        ViewData["Roles"] = new SelectList(_context.Roles, "RoleName", "RoleName");
                        return View(account);
                    }
                }
            }
            catch
            {
                
                return RedirectToAction("AdminDangKyTaiKhoan", "AdminAccounts");
            }
            ViewData["Roles"] = new SelectList(_context.Roles, "RoleName", "RoleName");
            return View(account);
        }





        [Route("admin-dang-xuat.html", Name = "AdminLogout")]
        public IActionResult AdminLogout()
        {
            HttpContext.SignOutAsync();
            toastNotification.AddSuccessToastMessage("LOGOUT SUCCESS");
            HttpContext.Session.Remove("AccountId");
            return RedirectToAction("AdminLogin", "AdminAccounts");
        }

        [HttpPost]
        public IActionResult AdminChangePassword(AdminChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("AccountId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("AdminLogin", "AdminAccounts");
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Accounts.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "AdminAccounts");

                    var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    if (pass == taikhoan.Password)
                    {
                        string passnew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                        taikhoan.Password = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        return RedirectToAction("Index", "AdminAccounts");
                    }
                }

            }
            catch
            {
                return RedirectToAction("Index", "AdminAccounts");
            }
            return RedirectToAction("Index", "AdminAccounts");
        }


        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(m => m.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccoutId,Phone,Email,Password,Salt,Active,Fullname,RoleId,LastLogin,CreatDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                toastNotification.AddSuccessToastMessage("ADD SUCCESS");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccoutId,Phone,Email,Password,Salt,Active,Fullname,RoleId,LastLogin,CreatDate")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    toastNotification.AddSuccessToastMessage("EDIT SUCCESS");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(m=>m.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'Really9Context.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            toastNotification.AddSuccessToastMessage("DELETE SUCCESS");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return (_context.Accounts?.Any(e => e.AccountId == id)).GetValueOrDefault();
        }
        private async Task<int> GetAutoAssignedAdminId()
        {
            // Logic để xác định và trả về ID của admin
            // Ví dụ: Chọn admin dựa trên tải công việc hoặc chuyên môn
            var admin = await _context.Accounts
                                      .OrderBy(a => a.Fullname) // Giả sử có trường Workload
                                      .FirstOrDefaultAsync();
            return admin?.AccountId ?? 0; // Trả về 0 hoặc ID mặc định nếu không tìm thấy
        }
        [HttpGet]
        [Route("api/getAdmins")]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _context.Accounts
                                       
                                       .Select(a => new { a.AccountId, a.Fullname })
                                       .ToListAsync();
            return Ok(admins);
        }
    }
}
