using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using REALLY9.Extension;
using REALLY9.Helper;
using REALLY9.Models;
using REALLY9.ModelViews;
using System.Collections.Generic;

namespace REALLY9.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly Really9Context _context;
        private readonly IToastNotification toastNotification;
        public CheckOutController(Really9Context context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }

        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(string returnUrl = null)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;

            }
            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "LocationId", "Name");
            ViewBag.GioHang = cart;
            return View(model);
        }

        [HttpPost]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(MuaHangVM muaHang)
        {
            var cart = GioHang;
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            var groupedProducts = cart.GroupBy(item => item.product.ProductName)
                          .Select(group => new
                          {
                              ProductName = group.Key,
                              Quantity = group.Sum(item => item.amount)
                          })
                          .ToList();
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;

                khachhang.LocationId = muaHang.TinhThanh;
                khachhang.District = muaHang.QuanHuyen;
                khachhang.Ward = muaHang.PhuongXa;
                khachhang.Address = muaHang.Address;
                _context.Update(khachhang);
                _context.SaveChanges();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    var appliedDiscountCode = HttpContext.Session.GetString("AppliedDiscountCode");
                    
                    Order donhang = new Order();
                    donhang.CustomerId = model.CustomerId;
                    donhang.Address = model.Address;
                    donhang.Orderdate = DateTime.Now;
                    donhang.TransactStatusId = 1;
                    donhang.Deleted = false;
                    donhang.Paid = false;
                    donhang.Note = Utilities.StripHTML(model.Note);
                    donhang.TotalMoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                    if (int.TryParse(Request.Form["DiscountedTotal"], out int discountedTotal))
                    {
                        donhang.TotalMoneyAfterusedis = discountedTotal;
                    }
                    else
                    {
                        // Nếu không có giá trị giảm giá, sử dụng tổng tiền ban đầu
                        donhang.TotalMoneyAfterusedis = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                    }




                    _context.Add(donhang);
                    _context.SaveChanges();

                    foreach (var item in cart)
                    {
                        string productQuantities = string.Join(", ", groupedProducts.Select(group => $"{group.Quantity} {group.ProductName}"));
                        var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderId = donhang.OrderId;
                        orderDetail.ProductId = item.product.ProductId;
                        orderDetail.CustomerName = khachhang.FullName;
                        orderDetail.ProductName = productQuantities;
                        orderDetail.Quantity = groupedProducts.Sum(group => group.Quantity);
                        orderDetail.Total = donhang.TotalMoneyAfterusedis;
                        orderDetail.Price = item.product.Price;
                        orderDetail.CreatedDate = DateTime.Now;
                        _context.Add(orderDetail);

                        Product product = _context.Products.AsNoTracking().FirstOrDefault(p => p.ProductId == item.product.ProductId);
                        if (product != null && product.UnitslnStock >= item.amount)
                        {
                            product.UnitslnStock -= item.amount;
                            _context.Update(product);
                        }
                        
                    }

                    _context.SaveChanges();
                    if (!string.IsNullOrEmpty(appliedDiscountCode))
                    {
                        var discount = _context.Discounts.FirstOrDefault(d => d.Discountcode == appliedDiscountCode);
                        if (discount != null)
                        {
                            _context.Discounts.Remove(discount);
                            _context.SaveChanges();
                        }

                        HttpContext.Session.Remove("AppliedDiscountCode");
                    }
                    toastNotification.AddSuccessToastMessage("BUY SUCCESS");
                    HttpContext.Session.Remove("GioHang");
                    return RedirectToAction("Success");
                }
            }
            catch
            {
                ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "LocationId", "Name");
                ViewBag.GioHang = cart;
                return View(model);
            }
            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "LocationId", "Name");
            ViewBag.GioHang = cart;
            return View(model);
        }

        
        [Route("dat-hang-thanh-cong.html", Name = "Success")]
        public IActionResult Success()
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if(string.IsNullOrEmpty(taikhoanID) )
                {
                    return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong.html" });
                }
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                var donhang=_context.Orders.AsNoTracking().Where(x=>x.CustomerId==Convert.ToInt32(taikhoanID)).OrderByDescending(x=>x.Orderdate).ToList();
                MuaHangSuccessVM successVM= new MuaHangSuccessVM();
                successVM.FullName = khachhang.FullName;
                successVM.Phone = khachhang.Phone;
                successVM.Address = khachhang.Address;
                successVM.PhuongXa = GetNameLocation(khachhang.Ward.Value);
                successVM.TinhThanh = GetNameLocation(khachhang.District.Value);
                return View(successVM);
            }
            catch
            {
                return View();
            }
        }
       
        public string GetNameLocation(int idlocation)
        {
            try
            {
                var location = _context.Locations.AsNoTracking().SingleOrDefault(x => x.LocationId == idlocation);
                if (location != null)
                {
                    return location.NameWithType;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
}
