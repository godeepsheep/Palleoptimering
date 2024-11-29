using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;
using System.Security.Cryptography;
using System.Xml.Linq;
using PalletOptimization.Enums;
using Azure.Identity;

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
            // Initialize an empty list of elements if no model is passed
            var elements = new List<CombinedModel>();
            return View(elements);
        }

        public async Task<IActionResult> MakeOrder()
        {
            var combinedModel = new List<CombinedModel>();
            var random = new Random();

            int count = random.Next(1, 15); // Generate 1-15 random elements
            for (int i = 0; i < count; i++)
            {
                int id = random.Next(41, 60); // Random IDs (assumes 1-10 range)
                var element = await _context.Elements.FirstOrDefaultAsync(m => m.Id == id);
                if (element != null)
                {
                    CombinedModel combined = new CombinedModel();
                    combined.Elements = element;
                    combinedModel.Add(combined);
                }
            }

            return View("Planner", combinedModel); // Pass the list of elements to the view
        }
        
        [HttpPost]
        public IActionResult UpdateRotationRule(Elements element, RotationOptions selectedRotationRule, List<Elements> elements)
        {
            System.Diagnostics.Debug.WriteLine("Here" + element);
            elements.Remove(element);
            // Update the RotationRules value
            element.RotationRules = selectedRotationRule;
            System.Diagnostics.Debug.WriteLine("Here" + element);
            elements.Add(element);

            // Return the updated list to the view
            return View("Planner",  elements);
        }
        [HttpPost]
        public IActionResult UpdateRotationRuleTest(Elements element, List<Elements> elements)
        {
            foreach(var x in elements)
            {
                System.Diagnostics.Debug.WriteLine("Foobar " + x);
            }
            System.Diagnostics.Debug.WriteLine("Foobar " + elements);
            element.RotationRules = RotationOptions.NeedToRotate;
            System.Diagnostics.Debug.WriteLine("Foobar " + element.RotationRules);
            return View("Planner", elements);
        }

        [HttpPost] 
        public void SavePalletSettings() //IT FUCKING WORKS BOI
        {
            
            Pallets.MaxHeight             = int.Parse(Request.Form["MaxHeight"]);
            Pallets.MaxWeight             = int.Parse(Request.Form["MaxWeight"]);
            Pallets.MaxOverhang           = int.Parse(Request.Form["Overhang"]);
            Pallets.SpaceBetweenElements  = int.Parse(Request.Form["SpaceBetween"]);
            Pallets.StackingMaxHeight     = int.Parse(Request.Form["StackingHeight"]);
            Pallets.StackingMaxWeight     = int.Parse(Request.Form["StackingWeight"]);
            Pallets.Endplate              = int.Parse(Request.Form["AddedPlate"]);
            Pallets.MaxHeight             = int.Parse(Request.Form["MaxPalletSpace"]);
        }
        

    }
}
