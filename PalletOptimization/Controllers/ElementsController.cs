using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;
using System.Security.Cryptography;
using System.Text.Json;
using PalletOptimization.Enums;

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
            var elementsJson = HttpContext.Session.GetString("Elements");
            var elements = string.IsNullOrEmpty(elementsJson)
                ? new List<Elements>()
                : JsonSerializer.Deserialize<List<Elements>>(elementsJson);

            ViewBag.RotationOptions = Enum.GetValues(typeof(RotationOptions)).Cast<RotationOptions>().ToList();
            return View(elements);
        }

        public async Task<IActionResult> MakeOrder()
        {
            var elements = new List<Elements>();
            var random = new Random();

            int count = random.Next(1, 15); // Generate 1-15 random elements
            for (int i = 0; i < count; i++)
            {
                int id = random.Next(41, 60); // Random IDs
                var element = await _context.Elements.FirstOrDefaultAsync(m => m.Id == id);
                if (element != null)
                {
                    // Create a new instance with InstanceId for each element
                    var instanceElement = new Elements
                    {
                        Id = element.Id,
                        Name = element.Name,
                        RotationRules = element.RotationRules,
                        IsSpecial = element.IsSpecial,
                        MaxElementsPerPallet = element.MaxElementsPerPallet,
                        Length = element.Length,
                        Width = element.Width,
                        Height = element.Height,
                        Weight = element.Weight,
                        InstanceId = Guid.NewGuid()
                    };

                    elements.Add(instanceElement);
                }
            }

            // Serialize and store the list in the session
            HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(elements));

            return RedirectToAction("Planner");
        }

        [HttpPost]
        public IActionResult SaveAllElements(Dictionary<Guid, ElementsDto> elements)
        {
            // Log received elements for debugging
            Console.WriteLine("Received Elements for Update:");
            foreach (var updatedElement in elements)
            {
                Console.WriteLine($"InstanceId: {updatedElement.Key}, RotationRules: {updatedElement.Value.RotationRules}, IsSpecial: {updatedElement.Value.IsSpecial}, MaxElementsPerPallet: {updatedElement.Value.MaxElementsPerPallet}, HeightWidthFactor: {updatedElement.Value.HeightWidthFactor}");
            }

            var elementsJson = HttpContext.Session.GetString("Elements");
            var currentElements = string.IsNullOrEmpty(elementsJson)
                ? new List<Elements>()
                : JsonSerializer.Deserialize<List<Elements>>(elementsJson);

            // Log current elements for debugging
            Console.WriteLine("Current Elements before update:");
            foreach (var element in currentElements)
            {
                Console.WriteLine($"InstanceId: {element.InstanceId}, Name: {element.Name}, RotationRules: {element.RotationRules}, IsSpecial: {element.IsSpecial}, MaxElementsPerPallet: {element.MaxElementsPerPallet}, HeightWidthFactor: {element.HeightWidthFactor}");
            }

            // Iterate through the updated elements and update them in the list
            foreach (var updatedElement in elements.Values)
            {
                var existingElement = currentElements.FirstOrDefault(e => e.InstanceId == updatedElement.InstanceId);
                if (existingElement != null)
                {
                    existingElement.RotationRules = updatedElement.RotationRules;
                    existingElement.IsSpecial = updatedElement.IsSpecial;
                    existingElement.MaxElementsPerPallet = updatedElement.MaxElementsPerPallet;
                    existingElement.HeightWidthFactor = updatedElement.HeightWidthFactor;
                }
            }

            // Save the updated list back to the session
            HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(currentElements));

            // Log updated elements for verification
            Console.WriteLine("Updated Elements List:");
            foreach (var element in currentElements)
            {
                Console.WriteLine($"InstanceId: {element.InstanceId}, Name: {element.Name}, RotationRules: {element.RotationRules}, IsSpecial: {element.IsSpecial}, MaxElementsPerPallet: {element.MaxElementsPerPallet}, HeightWidthFactor: {element.HeightWidthFactor}");
            }

            return RedirectToAction("Planner");
        }



        [HttpPost]
        public void SavePalletSettings() //IT FUCKING WORKS BOI
        {

            Pallets.MaxHeight = int.Parse(Request.Form["MaxHeight"]);
            Pallets.MaxWeight = int.Parse(Request.Form["MaxWeight"]);
            Pallets.MaxOverhang = int.Parse(Request.Form["Overhang"]);
            Pallets.SpaceBetweenElements = int.Parse(Request.Form["SpaceBetween"]);
            Pallets.StackingMaxHeight = int.Parse(Request.Form["StackingHeight"]);
            Pallets.StackingMaxWeight = int.Parse(Request.Form["StackingWeight"]);
            Pallets.Endplate = int.Parse(Request.Form["AddedPlate"]);
            Pallets.MaxHeight = int.Parse(Request.Form["MaxPalletSpace"]);
        }
    }
}

