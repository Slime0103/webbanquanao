using Microsoft.AspNetCore.Mvc;
using REALLY9.Extension;
using REALLY9.ModelViews;

namespace REALLY9.Controllers.Components
{
    public class HeaderCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            return View(cart);
        }
    }
}
