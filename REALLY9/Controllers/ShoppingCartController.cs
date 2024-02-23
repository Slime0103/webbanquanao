using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using REALLY9.Extension;
using REALLY9.Models;
using REALLY9.ModelViews;

namespace REALLY9.Controllers
{
    public class ShoppingCartController : Controller
    {

        private readonly Really9Context _context;
        private readonly IToastNotification toastNotification;
        public ShoppingCartController(Really9Context context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        public List<CartItem> GioHang 
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if(gh ==default(List<CartItem>))
                {
                    gh= new List<CartItem>();
                }
                return gh;
            }
        }
        [HttpPost]
        [Route("/api/cart/add")]
        public IActionResult AddToCart(int productID, int? amount)
        {
            List<CartItem> giohang=GioHang;
            try
            {
                CartItem item = GioHang.SingleOrDefault(p => p.product.ProductId == productID);
                if (item != null) //cap nhat so luong
                {
                    if (amount.HasValue)
                    {
                        item.amount = amount.Value;
                        
                    }
                    else
                    {

                        item.amount++;
                    }
                }
                else
                {
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productID);
                    item = new CartItem
                    {
                        amount = amount.HasValue ? amount.Value : 1,
                        product = hh
                    };
                    giohang.Add(item);
                }
                toastNotification.AddSuccessToastMessage("ADD PRODUCT SUCCESS");
                HttpContext.Session.Set<List<CartItem>>("GioHang", giohang);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }

            
        }


        [HttpPost]
        [Route("/api/cart/update")]
        public IActionResult UpdateCart(int productID, int? amount)
        {

            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            try
            {
                if(cart != null)
                {
                    CartItem item = cart.SingleOrDefault(p=>p.product.ProductId==productID);
                    if(item != null && amount.HasValue)
                    {
                        item.amount = amount.Value;
                    }
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("/api/cart/remove")]
        public ActionResult Remove(int productID)
        {
            try
            {
                List<CartItem> giohang = GioHang;
                CartItem item = giohang.SingleOrDefault(p=>p.product.ProductId == productID);
                if(item != null)
                {
                    giohang.Remove(item);
                    toastNotification.AddSuccessToastMessage("REMOVE SUCCESS");

                }
                HttpContext.Session.Set<List<CartItem>>("GioHang", giohang);
                return Json(new { success = true });
            }
            catch { return Json(new { success = false });}
        }
        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            
            
            return View(GioHang);
        }
        [HttpPost]
        [Route("/api/cart/applycoupon")]
        public ActionResult ApplyCoupon(string couponCode, decimal subtotal)
        {
            decimal discountPercentage = 0.0m;
            decimal discount = 0.0m;

            // Kiểm tra độ dài của mã coupon và áp dụng giảm giá tương ứng
            if (!string.IsNullOrEmpty(couponCode))
            {
                if (couponCode.Length == 4)
                {
                    discountPercentage = 0.2m; // Giảm giá 20% cho mã có độ dài 4 số
                }
                else if (couponCode.Length == 5)
                {
                    discountPercentage = 0.25m; // Giảm giá 25% cho mã có độ dài 5 số
                }
                else if (couponCode.Length == 6)
                {
                    discountPercentage = 0.3m; // Giảm giá 30% cho mã có độ dài 6 số
                }
            }

            // Tính giảm giá và giá trị sau khi giảm giá
            discount = subtotal * discountPercentage;
            decimal discountedTotal = subtotal - discount;

            return Json(new { success = true, discountedTotal = discountedTotal });
        }
    }
}
