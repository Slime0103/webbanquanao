using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using REALLY9.Models;

namespace REALLY9.Controllers
{
    public class BlogController : Controller
    {

        private readonly Really9Context _context;

        public BlogController(Really9Context context)
        {
            _context = context;
        }
        [Route("blog.html", Name ="Blog")]
        public IActionResult Index(int? page)
        {
           

            var pageNumber = page == null || page < 0 ? 1 : page.Value;
            var pageSize = 10;
            var lsTinTucs = _context.TblTinTucs
                .AsNoTracking()
                .OrderByDescending(x => x.PostId);
            PagedList<TblTinTuc> models = new PagedList<TblTinTuc>(lsTinTucs, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);

        }

        [Route("/tin-tuc/{Alias}-{id}.html",Name = "TinDetails")]
        public IActionResult Details(int id)
        {
            var tintuc = _context.TblTinTucs.AsNoTracking().SingleOrDefault(x => x.PostId == id);
            if (tintuc == null)
            {
                return RedirectToAction("Index");
            }
            var lsBaivietlienquan =_context.TblTinTucs.AsNoTracking().Where(x=>x.Published==true&&x.PostId!=id).Take(3).OrderByDescending(x=>x.CreatedDate).ToList();
            ViewBag.Baivietlienquan=lsBaivietlienquan;
            return View(tintuc);
        }
    }
}
