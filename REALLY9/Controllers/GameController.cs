using Microsoft.AspNetCore.Mvc;
using REALLY9.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MailKit.Net;
using REALLY9.Services;
// Các thư viện khác

namespace REALLY9.Controllers
{
    [Route("Game")]
    public class GameController : Controller
    {
        private readonly Really9Context _context;
        private readonly IEmailService _emailService;

        public GameController(Really9Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("Play")]
        public async Task<IActionResult> Play()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Accounts"); 
            }

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (!int.TryParse(customerIdString, out int customerId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var user = await _context.Customers.FindAsync(customerId);

            if (user == null)
            {
                return RedirectToAction("Login", "Accounts");
            }

            if (DateTime.UtcNow < user.NextGameTime)
            {
                return View("WaitForNextGame"); 
            }

            
            var discountCode = await GetRandomDiscountCode();
            if (discountCode != null)
            {
                await _emailService.SendEmailAsync(user.Email, "Mã Giảm Giá Của Bạn", $"Mã giảm giá của bạn là: {discountCode.Discountcode}");
                discountCode.Active = false;
                _context.Discounts.Update(discountCode);
                await _context.SaveChangesAsync();
            }

            // Cập nhật thời gian chơi tiếp theo
            user.NextGameTime = DateTime.UtcNow.AddHours(4);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Redirect("https://slime0103.github.io/game/");
        }

        private async Task<Discount> GetRandomDiscountCode()
        {
            var discountCodes = await _context.Discounts
                                              .Where(d => d.Active == true)
                                              .ToListAsync();
            if (discountCodes.Count == 0)
            {
                return null;
            }
            var random = new Random();
            int index = random.Next(discountCodes.Count);
            return discountCodes[index];
        }
    }
}