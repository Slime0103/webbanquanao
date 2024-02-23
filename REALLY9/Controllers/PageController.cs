using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using REALLY9.Models;

namespace REALLY9.Controllers
{
    public class PageController : Controller
    {
        private readonly Really9Context _context;

        public PageController(Really9Context context)
        {
            _context = context;
        }
      
        [Route("/page/{Alias}", Name = "PageDetails")]
        public IActionResult Details(string Alias)
        {
            if(string.IsNullOrEmpty(Alias)) return RedirectToAction("Index","Home");
            var page = _context.Pages.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
            if (page == null)
            {
                return RedirectToAction("Index", "Home");
            }
          
            return View(page);
        }
    }
}
