using Microsoft.AspNetCore.Mvc;

namespace REALLY9.Controllers
{
    public class ChatController : Controller
    {
        [Route("chat.html")]
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
