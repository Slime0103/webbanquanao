using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;
using System.Threading.Tasks;

namespace REALLY9.Controllers
{
    public class GPTController : Controller
    {
        private readonly OpenAIAPI openai;

        public GPTController()
        {
            openai = new OpenAIAPI("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");
        }

        public IActionResult Chat()
        {
            return View(); // Trả về view "Chat.cshtml"
        }

        [HttpGet]
        [Obsolete]
        public async Task<IActionResult> UseChatGPT(string query)
        {
            CompletionRequest completionRequest = new CompletionRequest
            {
                Prompt = query,
                Model = OpenAI_API.Models.Model.DavinciText,
                 MaxTokens = 2000
            };

            var completions = await openai.Completions.CreateCompletionAsync(completionRequest);
            string OutPutResult = string.Join("\n", completions.Completions.Select(c => c.Text));

            return Content(OutPutResult);
        }
    }
}