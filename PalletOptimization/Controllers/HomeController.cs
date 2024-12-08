using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.AspNetCore.Mvc;
using PalletOptimization.Models;

namespace PalletOptimization.Controllers
{
    [Authorize] //HoneController actions can only be used by admin/approved users.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(); // Accessible only to logged-in users
        }

        public IActionResult Privacy()
        {
            return View(); // Accessible only to logged-in users
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}