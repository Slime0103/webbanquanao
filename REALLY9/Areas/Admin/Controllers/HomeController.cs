using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REALLY9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/Admin", Name = "AdminHome")]
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
