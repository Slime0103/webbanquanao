using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REALLY9.Models;
using REALLY9.ModelViews;

namespace REALLY9.Controllers.Components
{
    public class DonHangController : Controller
    {
        private readonly Really9Context _context;


        public DonHangController(Really9Context context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID)) return RedirectToAction("Login", "Accounts");
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang == null)
                {
                    return NotFound();
                }
                var donhang = await _context.Orders
                    .Include(x => x.TransactStatus)
                    .Include(x => x.OrderDetails)
                    .FirstOrDefaultAsync(m => m.OrderId == id && Convert.ToInt32(taikhoanID) == m.CustomerId);
                if (donhang == null)
                {
                    return NotFound();
                }
                var chitietdonhang = _context.OrderDetails
                    .Include(x => x.Product)

                    .AsNoTracking()
                    .Where(x => x.OrderId == id)
                    .OrderBy(x => x.OrderDetailId)
                    .ToList();
                XemDonHang donHang = new XemDonHang
                {
                    DonHang = donhang,
                    ChiTietDonHang = chitietdonhang
                };

                return PartialView("Details", donHang);
                
            }
            catch
            {
                return NotFound();
            }
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
