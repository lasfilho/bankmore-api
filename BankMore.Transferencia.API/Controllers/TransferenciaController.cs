using Microsoft.AspNetCore.Mvc;

namespace BankMore.Transferencia.API.Controllers
{
    public class TransferenciaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
