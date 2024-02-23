using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using REALLY9.Models;
using REALLY9.ModelViews;
using System.Diagnostics;

namespace REALLY9.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Really9Context _context;

        public HomeController(ILogger<HomeController> logger, Really9Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int CatID = 0)
        {
            HomeViewVM model= new HomeViewVM();
            var lsProducts= _context.Products.AsNoTracking()
                .Where(x=>x.Active==true&&x.HomeFlag==true)
                .OrderByDescending(x=>x.DateCreated)
                .ToList();

            List<ProductHomeVM> lsProductViews= new List<ProductHomeVM>();
            var lsCats = _context.Categories.AsNoTracking()
                .Where(x=>x.Published==true)
                .OrderByDescending(x=>x.Ordering)
                .ToList();
            foreach (var item in lsCats)
            {
                ProductHomeVM productHome= new ProductHomeVM();
                productHome.category=item;
                productHome.lsProducts=lsProducts.Where(x=>x.CatId==item.CatId).ToList();
                lsProductViews.Add(productHome);    

            }

            var TinTuc=_context.TblTinTucs.AsNoTracking()
                .Where(x=>x.Published==true&&x.IsNewfeed==true)
                .OrderByDescending(x=>x.CreatedDate)
                .Take(3)
                .ToList();
            model.Products=lsProductViews;
            model.TblTinTucs=TinTuc;
            ViewBag.AllProducts = lsProducts;
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName", CatID);
            return View(model);
        }

      
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("ContactUs.html", Name = "Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("AboutUs.html", Name = "About")]
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}