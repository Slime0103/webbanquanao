using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using REALLY9.Extension;
using REALLY9.Models;
using REALLY9.ModelViews;
using REALLY9.Services;

namespace REALLY9.Controllers
{
    public class DiscountController : Controller
    {
        private readonly Really9Context _context;
        private readonly IEmailService _emailService;
        private readonly IToastNotification toastNotification;
        public DiscountController(Really9Context context, IToastNotification toastNotification, IEmailService emailService)
        {
            _context = context;
            this.toastNotification = toastNotification;
            this._emailService = emailService;
        }
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để sử dụng mã giảm giá" });
            }

            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            if (cart == null || cart.Count == 0)
            {
                return Json(new { success = false, message = "Giỏ hàng trống" });
            }

            int totalMoney = (int)cart.Sum(x => x.TotalMoney);

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Discountcode == couponCode && d.Active == true);
            if (discount == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc không còn hiệu lực" });
            }

            int discountPercentage = GetDiscountPercentage(discount.Levels);
            int newTotal = totalMoney - (totalMoney * discountPercentage / 100);
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("AppliedDiscountId", discount.DiscountId);

            return Json(new { success = true, newTotal = newTotal });
        }
        private int GetDiscountPercentage(int? level)
        {
            return level switch
            {
                1 => 10,
                2 => 15,
                3 => 20,
                _ => 0,
            };
        }
        
        private int CalculateTotalMoney(int orderId)
        {
            var orderDetails = _context.OrderDetails
                                       .Where(od => od.OrderId == orderId)
                                       .ToList();

            int totalMoney = orderDetails.Sum(od => od.Total ?? 0);
            return totalMoney;
        }
        
    }
}
