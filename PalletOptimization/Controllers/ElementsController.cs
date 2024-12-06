using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;
using System.Security.Cryptography;
using System.Text.Json;
using PalletOptimization.Enums;
using System.Diagnostics;

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
            
            Debug.WriteLine("Elements in Planner:");
                foreach (var element in elements)
                {
                        Debug.WriteLine($"InstanceId: {element.InstanceId}, RotationRules: {element.RotationRules}");
                }

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
                        HeightWidthFactor = element.HeightWidthFactor,
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
            Debug.WriteLine("Received Elements for Update:");
            foreach (var updatedElement in elements)
            {
                Debug.WriteLine($"Updated Element InstanceId (Before Lookup): {updatedElement.Key}");

                Debug.WriteLine($"InstanceId: {updatedElement.Key}, RotationRules: {updatedElement.Value.RotationRules}, IsSpecial: {updatedElement.Value.IsSpecial}, MaxElementsPerPallet: {updatedElement.Value.MaxElementsPerPallet}");
            }

            


            // Get the current elements from the session
            var elementsJson = HttpContext.Session.GetString("Elements");
            var currentElements = string.IsNullOrEmpty(elementsJson)
                ? new List<Elements>()
                : JsonSerializer.Deserialize<List<Elements>>(elementsJson);

            // Make sure currentElements is never null by initializing it if necessary
            currentElements = currentElements ?? new List<Elements>();

            // Create a lookup dictionary for current elements by InstanceId for faster access
            var elementsLookup = currentElements.ToDictionary(e => e.InstanceId);


            Debug.WriteLine($"Elements count: {elements.Count}");
            Debug.WriteLine($"Elements Lookup Count: {elementsLookup.Count}");

            //To check the InstanceId attached to each element
            foreach (var lookupElement in elementsLookup)
            {
                Debug.WriteLine($"InstanceId: {lookupElement.Key}");
            }

            // Update the elements based on the received data
            Debug.WriteLine($"Elements count: {elements.Count}");
            foreach (var updatedElement in elements.Values)
            {

                Debug.WriteLine($"Updated Element InstanceId: {updatedElement.InstanceId}");

                if (elementsLookup.TryGetValue(updatedElement.InstanceId, out var existingElement))
                {
                    Debug.WriteLine($"Before Update - InstanceId: {existingElement.InstanceId}, RotationRules: {existingElement.RotationRules}");

                    // Update the element's properties
                    existingElement.RotationRules = updatedElement.RotationRules;
                    existingElement.IsSpecial = updatedElement.IsSpecial;
                    existingElement.MaxElementsPerPallet = updatedElement.MaxElementsPerPallet;

                    Debug.WriteLine($"After Update - InstanceId: {existingElement.InstanceId}, RotationRules: {existingElement.RotationRules}");
                }
                else
                {
                    Debug.WriteLine($"Element with InstanceId {updatedElement.InstanceId} not found.");
                }
            }
        
            // Save the updated elements back to the session
            Debug.WriteLine($"Session data before saving: {JsonSerializer.Serialize(currentElements)}");

            HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(currentElements));
        
            Debug.WriteLine("Updated Elements List:");
            foreach (var element in currentElements)
            {
                Debug.WriteLine($"InstanceId: {element.InstanceId}, RotationRules: {element.RotationRules}, IsSpecial: {element.IsSpecial}, MaxElementsPerPallet: {element.MaxElementsPerPallet}");
            }
        
            return Json(new { success = true, message = "Elements updated successfully!" });
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

