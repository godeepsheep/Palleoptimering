using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PalletOptimization.Data;
using PalletOptimization.Enums;

using PalletOptimization.Models;
using System.Text.Json;
using System.Xml.Linq;

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace PalletOptimization.Controllers
{
    public class ElementsController : Controller
    {
        private readonly AppDbContext _context;
        public List<PackedPallet> packedPallets = new();
        private List<TaggedItem> taggedItemsList = new();

        public ElementsController(AppDbContext context)
        {
            _context = context;
        }

        //______________________________________________________________________________________
        // UI Stuff
        //______________________________________________________________________________________

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
                        Tag = element.Tag,
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
            try
            {
                // Retrieve current elements from session
                var elementsJson = HttpContext.Session.GetString("Elements");
                var currentElements = string.IsNullOrEmpty(elementsJson)
                    ? new List<Elements>()
                    : JsonSerializer.Deserialize<List<Elements>>(elementsJson);
                
                if (currentElements == null)
                {
                    Debug.WriteLine("No elements found in session.");
                    return Json(new { success = false, message = "No elements found in session." });
                }
                Debug.WriteLine($"Current Elements in Session (Before Update): {JsonSerializer.Serialize(currentElements)}");

                // Update elements in session
                foreach (var updatedElement in elements.Values)
                {
                    var existingElement = currentElements.FirstOrDefault(e => e.InstanceId == updatedElement.InstanceId);
                    if (existingElement != null)
                    {
                        Debug.WriteLine($"Updating Element: {existingElement.InstanceId}");
                        existingElement.RotationRules = updatedElement.RotationRules;
                        existingElement.IsSpecial = updatedElement.IsSpecial;
                        existingElement.Tag = updatedElement.Tag;
                        Debug.WriteLine($"Updated Element: {JsonSerializer.Serialize(existingElement)}");
                    }
                    else
                    {
                        var newElement = new Elements
                        {
                            InstanceId = updatedElement.InstanceId,
                            RotationRules = updatedElement.RotationRules,
                            IsSpecial = updatedElement.IsSpecial,
                            Tag = updatedElement.Tag
                        };
                        currentElements.Add(newElement);
                        Debug.WriteLine($"Added New Element: {JsonSerializer.Serialize(newElement)}");
                    }
                }

                // Save updated elements back to session
                HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(currentElements));

                Debug.WriteLine($"Updated Elements in Session (After Update): {JsonSerializer.Serialize(currentElements)}");

                return Json(new { success = true, message = "Elements updated successfully!" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveAllElements: {ex.Message}");
                return Json(new { success = false, message = "Error updating elements." });
            }
        }


        [HttpPost]
        public IActionResult SavePalletSettings()
        {
            try
            {
                Pallets.MaxHeight = int.Parse(Request.Form["MaxHeight"]);
                Pallets.MaxWeight = int.Parse(Request.Form["MaxWeight"]);
                Pallets.MaxOverhang = int.Parse(Request.Form["Overhang"]);
                Pallets.SpaceBetweenElements = int.Parse(Request.Form["SpaceBetween"]);
                Pallets.StackingMaxHeight = int.Parse(Request.Form["StackingHeight"]);
                Pallets.StackingMaxWeight = int.Parse(Request.Form["StackingWeight"]);
                Pallets.Endplate = int.Parse(Request.Form["AddedPlate"]);
                Pallets.SlotsOnPallet = int.Parse(Request.Form["MaxPalletSpace"]);

                // Retrieve existing session data
                var sessionData = HttpContext.Session.GetString("Elements");
                if (!string.IsNullOrEmpty(sessionData))
                {
                    Debug.WriteLine("Existing session data for elements found.");
                }
                else
                {
                    Debug.WriteLine("No existing session data for elements.");
                }

                Debug.WriteLine("Pallet settings saved successfully.");
                return Json(new { success = true, message = "Pallet settings saved successfully!" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SavePalletSettings: {ex.Message}");
                return Json(new { success = false, message = "Error saving pallet settings." });
            }
        }



        //______________________________________________________________________________________
        //The Algorithm
        //______________________________________________________________________________________

        public void OptimizePacking(List<Elements> elements)
        {
            // Group elements by tag
            var taggedGroups = elements
                .Where(e => !string.IsNullOrEmpty(e.Tag))
                .GroupBy(e => e.Tag)
                .ToDictionary(g => g.Key, g => g.ToList());

            var untaggedItems = elements.Where(e => string.IsNullOrEmpty(e.Tag)).ToList();

            // Process each tagged group
            foreach (var (tag, groupElements) in taggedGroups)
            {
                packedPallets.AddRange(PackElements(groupElements, tag));
            }

            // Process untagged items
            packedPallets.AddRange(PackElements(untaggedItems, null));
        }

        private List<PackedPallet> PackElements(List<Elements> elements, string? tag)
        {
            var pallets = new List<PackedPallet>();
            PackedPallet currentPallet = InitializeNewPallet(tag);

            foreach (var element in elements.OrderByDescending(e => e.Height))
            {
                if (!CanFitOnPallet(currentPallet, element))
                {
                    // Finalize current pallet
                    ApplyLayering(currentPallet);
                    pallets.Add(currentPallet);

                    // Start a new pallet
                    currentPallet = InitializeNewPallet(tag);
                }

                PlaceElementOnPallet(currentPallet, element);
            }

            // Finalize the last pallet
            if (currentPallet.elementsOnPallet.Any())
            {
                ApplyLayering(currentPallet);
                pallets.Add(currentPallet);
            }

            return pallets;
        }

        private PackedPallet InitializeNewPallet(string? tag)
        {
            return new PackedPallet
            {
                elementsOnPallet = new List<Elements>(),
                TotalWeight = 0,
                TotalHeight = 0,
                Group = new PalletGroup { Name = tag ?? "Default" },
                specialPallet = !string.IsNullOrEmpty(tag)
            };
        }

        private bool CanFitOnPallet(PackedPallet pallet, Elements element)
        {
            return pallet.TotalWeight + element.Weight <= Pallets.MaxWeight &&
                   pallet.TotalHeight + element.Height <= Pallets.MaxHeight;
        }

        private void PlaceElementOnPallet(PackedPallet pallet, Elements element)
        {
            pallet.elementsOnPallet.Add(element);
            pallet.TotalWeight += element.Weight;
            pallet.TotalHeight += element.Height;
        }

        private void ApplyLayering(PackedPallet pallet)
        {
            int currentLayerHeight = 0;
            int currentLayerWeight = 0;
            int currentLayer = 1;

            foreach (var element in pallet.elementsOnPallet.OrderByDescending(e => e.Height))
            {
                if (currentLayerHeight + element.Height > Pallets.StackingMaxHeight ||
                    currentLayerWeight + element.Weight > Pallets.StackingMaxWeight)
                {
                    currentLayer++;
                    currentLayerHeight = 0;
                    currentLayerWeight = 0;
                }

                element.LayerNumber = currentLayer;
                currentLayerHeight += element.Height;
                currentLayerWeight += element.Weight;
            }
        }

        //______________________________________________________________________________________
        // JSON Output
        //______________________________________________________________________________________
        
        [HttpPost]
        public IActionResult OptimizeAndGenerateJson()
        {
            // Retrieve the current elements from the session
            var elementsJson = HttpContext.Session.GetString("Elements");
            if (string.IsNullOrEmpty(elementsJson))
            {
                return Json(new { success = false, message = "No elements found to optimize." });
            }
        
            var elements = JsonSerializer.Deserialize<List<Elements>>(elementsJson);
        
            if (elements == null || !elements.Any())
            {
                return Json(new { success = false, message = "No elements found to optimize." });
            }
        
            // Clear previously packed pallets
            packedPallets.Clear();
        
            // Run the optimization logic
            OptimizePacking(elements);
        
            // Store the packed pallets result (optional: to session, if needed for later)
            HttpContext.Session.SetString("PackedPallets", JsonSerializer.Serialize(packedPallets));
        
            // Generate the JSON output
            var output = packedPallets.GroupBy(p => p.Group.Name)
                .Select(group => new
                {
                    Tag = group.Key,
                    Pallets = group.Select(p => new
                    {
                        PalletType = p.PalletType.ToString(),
                        PalletGroup = p.Group.Name,
                        Layers = p.elementsOnPallet.GroupBy(e => e.LayerNumber)
                            .Select(layer => new
                            {
                                LayerNumber = layer.Key,
                                Slots = layer.Select(e => new
                                {
                                    Slot = e.Slot,
                                    Name = e.Name,
                                    Rotation = e.IsRotated ? "Rotated" : "Original"
                                })
                            })
                    })
                });
        
            return Json(output);
        }
        
    }
}
