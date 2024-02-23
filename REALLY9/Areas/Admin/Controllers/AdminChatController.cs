using Microsoft.AspNetCore.Mvc;

namespace REALLY9.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminChatController : Controller
    {
        [Route("/adminchat.html")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/admindetailschat.html")]
        public IActionResult Details()
        {
            return View();
        }

    }

}
