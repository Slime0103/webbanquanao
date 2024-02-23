using Microsoft.AspNetCore.Mvc;
using REALLY9.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace REALLY9.Controllers
{
    public class LocationController : Controller
    {
        private readonly Really9Context _context;
        public LocationController(Really9Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region ==========GET LOCATION ========
        public ActionResult QuanHuyenList(int LocationId)
        {
            var QuanHuyens = _context.Locations.OrderBy(x=>x.LocationId)
                .Where(x=>x.ParentCode==LocationId&&x.Levels==2)
                .OrderBy(x=>x.Name) 
                .ToList();
            return Json(QuanHuyens);
        }
        public ActionResult PhuongXaList(int LocationId)
        {
            var PhuongXas = _context.Locations.OrderBy(x => x.LocationId)
                .Where(x => x.ParentCode == LocationId && x.Levels == 3)
                .OrderBy(x => x.Name)
                .ToList();
            return Json(PhuongXas);
        }
        #endregion===================
    }
}
