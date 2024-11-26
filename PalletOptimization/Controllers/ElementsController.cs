using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;
using System.Security.Cryptography;

namespace PalletOptimization.Controllers
{
    public class ElementsController : Controller
    {
        private readonly AppDbContext _context;

        public ElementsController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Planner()
        {
            return View();
        }

        public async Task<IActionResult> MakeOrder()
        {
            List<Elements> elements = new();
            Random random = new();
            int x = random.Next(1,15);
           
            for (int i = 0; i < x; i++)
            {
                int id = random.Next(1, 10);
                var Elements = await _context.Elements.FirstOrDefaultAsync(m => m.Id == id);
                elements.Add(Elements);
            }

            return View("Planner", elements);
        }
    }
}
