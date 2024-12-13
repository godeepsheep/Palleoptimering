using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Enums;
using PalletOptimization.Models;
using System.Text.Json;
using System.Diagnostics;

namespace PalletOptimization.Controllers
{
    public class ElementsController : Controller
    {
        private readonly AppDbContext _context;
        public List<PackedPallet> packedPallets = new();

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
                Debug.WriteLine($"InstanceId: {element.InstanceId}, RotationRules: {element.RotationRules}, Tag: {element.Tag}");
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

            HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(elements));

            return RedirectToAction("Planner");
        }

        [HttpPost]
        public IActionResult SaveAllElements(Dictionary<Guid, ElementsDto> elements)
        {
            try
            {
                var elementsJson = HttpContext.Session.GetString("Elements");
                var currentElements = string.IsNullOrEmpty(elementsJson)
                    ? new List<Elements>()
                    : JsonSerializer.Deserialize<List<Elements>>(elementsJson);

                if (currentElements == null)
                {
                    Debug.WriteLine("No elements found in session.");
                    return Json(new { success = false, message = "No elements found in session." });
                }

                foreach (var updatedElement in elements.Values)
                {
                    var existingElement =
                        currentElements.FirstOrDefault(e => e.InstanceId == updatedElement.InstanceId);
                    if (existingElement != null)
                    {
                        existingElement.RotationRules = updatedElement.RotationRules;
                        existingElement.IsSpecial = updatedElement.IsSpecial;
                        existingElement.Tag = updatedElement.Tag;
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
                    }
                }

                HttpContext.Session.SetString("Elements", JsonSerializer.Serialize(currentElements));

                return Json(new { success = true, message = "Elements updated successfully!" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveAllElements: {ex.Message}");
                return Json(new { success = false, message = "Error updating elements." });
            }
        }

        //______________________________________________________________________________________
        // Algorithm
        //______________________________________________________________________________________

        private List<PalletGroup> GetPalletGroupsFromDb()
        {
            try
            {
                var palletGroups = _context.PalletGroups.ToList();

                if (!palletGroups.Any())
                {
                    throw new Exception("No pallet groups found in the database.");
                }

                Debug.WriteLine($"Fetched {palletGroups.Count} pallet groups from the database.");
                return palletGroups;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching pallet groups: {ex.Message}");
                throw;
            }
        }

        public void OptimizePacking(List<Elements> elements)
        {
            var palletGroups = GetPalletGroupsFromDb();

            var taggedGroups = elements
                .Where(e => !string.IsNullOrEmpty(e.Tag))
                .GroupBy(e => e.Tag)
                .ToDictionary(g => g.Key, g => g.ToList());

            var untaggedItems = elements.Where(e => string.IsNullOrEmpty(e.Tag)).ToList();

            foreach (var (tag, groupElements) in taggedGroups)
            {
                packedPallets.AddRange(PackElements(groupElements, tag, palletGroups));
            }

            packedPallets.AddRange(PackElements(untaggedItems, null, palletGroups));
        }

        private List<PackedPallet> PackElements(List<Elements> elements, string? tag, List<PalletGroup> palletGroups)
        {
            var pallets = new List<PackedPallet>();
            PackedPallet currentPallet = InitializeNewPallet(tag, palletGroups);

            foreach (var element in elements.OrderByDescending(e => e.Height))
            {
                if (!ValidateAndApplyRotation(element, currentPallet.Group.Width, currentPallet.Group.Length))
                {
                    Debug.WriteLine($"Element {element.Name} cannot fit even with rotation.");
                    continue;
                }

                if (!CanFitOnPallet(currentPallet, element))
                {
                    pallets.Add(currentPallet);
                    currentPallet = InitializeNewPallet(tag, palletGroups);
                }

                PlaceElementOnPallet(currentPallet, element);
            }

            if (currentPallet.elementsOnPallet.Any())
            {
                pallets.Add(currentPallet);
            }

            return pallets;
        }

        private PackedPallet InitializeNewPallet(string? tag, List<PalletGroup> palletGroups)
        {
            var bestMatch = palletGroups
                .Where(pg => pg.Length >= Pallets.Length && pg.Width >= Pallets.Width)
                .OrderBy(pg => pg.MaxWeight)
                .FirstOrDefault();

            if (bestMatch == null)
            {
                throw new Exception("No suitable pallet configuration found.");
            }

            return new PackedPallet
            {
                elementsOnPallet = new List<Elements>(),
                TotalWeight = 0,
                TotalHeight = 0,
                Group = bestMatch,
                specialPallet = !string.IsNullOrEmpty(tag),
                PalletType = (PalletTypeEnum)bestMatch.Id
            };
        }

        private bool ValidateAndApplyRotation(Elements element, int palletWidth, int palletLength)
        {
            Debug.WriteLine($"Validating rotation for element {element.Name} with dimensions (LxW): {element.Length} x {element.Width}");

            if (element.Length <= palletWidth && element.Width <= palletLength)
            {
                element.IsRotated = false;
                Debug.WriteLine($"Element {element.Name} fits without rotation.");
                return true;
            }

            if (element.Width <= palletWidth && element.Length <= palletLength)
            {
                (element.Length, element.Width) = (element.Width, element.Length);
                element.IsRotated = true;
                Debug.WriteLine($"Element {element.Name} fits with rotation.");
                return true;
            }

            Debug.WriteLine($"Element {element.Name} cannot fit even with rotation.");
            return false;
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

            Debug.WriteLine($"Placed element {element.Name} on pallet. Total weight: {pallet.TotalWeight}, Total height: {pallet.TotalHeight}");
        }

        //______________________________________________________________________________________
        // JSON Output
        //______________________________________________________________________________________

        [HttpPost]
        public IActionResult OptimizeAndGenerateJson()
        {
            try
            {
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

                packedPallets.Clear();
                OptimizePacking(elements);

                var output = packedPallets.GroupBy(p => p.Group.Name)
                    .Select(group => new
                    {
                        Tag = group.Key ?? "Untagged",
                        Pallets = group.Select(p => new
                        {
                            PalletType = p.PalletType.ToString(),
                            PalletGroup = p.Group.Name,
                            TotalWeight = p.TotalWeight,
                            TotalHeight = p.TotalHeight,
                            Layers = p.elementsOnPallet.GroupBy(e => e.LayerNumber)
                                .Select(layer => new
                                {
                                    LayerNumber = layer.Key,
                                    Slots = layer.Select(e => new
                                    {
                                        Slot = e.Slot,
                                        Name = e.Name,
                                        Rotation = e.IsRotated ? "Rotated" : "Original",
                                        Weight = e.Weight,
                                        Height = e.Height
                                    }).ToList()
                                }).ToList()
                        }).ToList()
                    }).ToList();

                Debug.WriteLine("Generated JSON Output:");
                Debug.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));

                return Json(output);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                return Json(new { success = false, message = "Unexpected error occurred." });
            }
        }
    }
}
