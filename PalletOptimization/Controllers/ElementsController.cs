﻿using System;
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
        public List<Elements> unfitElements = new();

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
            var groupedElements = GroupElementsByTag(elements);
            
            var palletGroups = GetPalletGroupsFromDb();

            foreach (var (tag, group) in groupedElements)
            {
                var sortedElements = SortElements(group);
                packedPallets.AddRange(PackElements(sortedElements, tag, palletGroups));
            }
            

        }
        
        private Dictionary<string, List<Elements>> GroupElementsByTag(List<Elements> elements)
        {
            var taggedGroups = elements
                .Where(e => !string.IsNullOrEmpty(e.Tag))
                .GroupBy(e => e.Tag)
                .ToDictionary(g => g.Key, g => g.ToList());

            var untaggedGroup = elements
                .Where(e => string.IsNullOrEmpty(e.Tag))
                .ToList();

            if (untaggedGroup.Any())
            {
                taggedGroups["Untagged"] = untaggedGroup;
            }

            return taggedGroups;
        }

        private List<Elements> SortElements(List<Elements> elements)
        {
            // Default sorting: by weight (descending)
            return elements.OrderByDescending(e => e.Weight).ThenByDescending(e => e.Height).ToList();
        }
        

        private List<PackedPallet> PackElements(List<Elements> elements, string? tag, List<PalletGroup> palletGroups)
        {
            var pallets = new List<PackedPallet>();
            PackedPallet currentPallet = InitializeNewPallet(tag, palletGroups);
            var sortedElements = elements
                .OrderByDescending(e => e.Weight)
                .ThenByDescending(e => e.Height)
                .ToList();
            foreach (var element in sortedElements)
            {
                
                if (!ValidateAndApplyRotation(element, currentPallet.Group.Width, currentPallet.Group.Length))
                {
                    unfitElements.Add(element);
                    Debug.WriteLine($"Element {element.Name} cannot fit even with rotation.");
                    continue;
                }

                var targetPallet = pallets.FirstOrDefault(p => CanFitOnPallet(p, element));
                if (targetPallet != null)
                {
                    AssignElementToOptimalSlotAndLayer(element, targetPallet);
                }
                else
                {
                    if (currentPallet.elementsOnPallet.Any())
                    {
                        pallets.Add(currentPallet);
                    }
                    currentPallet = InitializeNewPallet(tag, palletGroups);
                    
                    if (CanFitOnPallet(currentPallet, element))
                    {
                        AssignElementToOptimalSlotAndLayer(element, currentPallet);
                    }
                    else
                    {
                        Debug.WriteLine($"Skipping element {element.Name} - Cannot fit on any pallet.");
                        packedPallets.Add(currentPallet);
                        currentPallet = InitializeNewPallet(tag, palletGroups);
                        AssignElementToOptimalSlotAndLayer(element, currentPallet);
                    }
                }
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
                if (element.RotationRules == RotationOptions.CanRotate || element.RotationRules == RotationOptions.NeedToRotate)
                {
                    (element.Length, element.Width) = (element.Width, element.Length);
                    element.IsRotated = true;
                    Debug.WriteLine($"Element {element.Name} fits with rotation.");
                    return true;
                }
            }

            Debug.WriteLine($"Element {element.Name} cannot fit even with rotation.");
            return false;
        }

        private bool CanFitOnPallet(PackedPallet pallet, Elements element)
        {
            return pallet.TotalWeight + element.Weight <= Pallets.MaxWeight &&
                   pallet.TotalHeight + element.Height <= Pallets.MaxHeight;
        }

        private void AssignElementToOptimalSlotAndLayer(Elements element, PackedPallet pallet)
        {
            int slotWidth = pallet.Group.Width / Pallets.SlotsOnPallet;
            bool needsMultiSlot = element.Width > slotWidth;

            int selectedSlot = DetermineOptimalSlot(pallet, element, needsMultiSlot);
            element.Slot = selectedSlot;

            if (needsMultiSlot)
            {
                element.Slot = selectedSlot == 1 ? 1 :
                    selectedSlot == 5 ? 5 : 3;
            }

            element.LayerNumber = DetermineLayerNumber(pallet, element);

            PlaceElementOnPallet(pallet, element);
        }
        private int DetermineOptimalSlot(PackedPallet pallet, Elements element, bool needsMultiSlot)
        {
            
            if (pallet.elementsOnPallet.Count < 2 && element.Weight > 50)
            {
                return pallet.elementsOnPallet.Count == 0 ? 1 : 5;
            }

            // Find an available slot
            for (int slot = 1; slot <= Pallets.SlotsOnPallet; slot++)
            {
                bool isSlotAvailable = !pallet.elementsOnPallet
                    .Any(e => e.Slot == slot || (needsMultiSlot && e.Slot == (slot % Pallets.SlotsOnPallet) + 1));

                if (isSlotAvailable)
                {
                    return slot;
                }
            }

            return 3; // Fallback to middle slot
        }
        private int DetermineLayerNumber(PackedPallet pallet, Elements element)
        {
            // If adding this element would exceed max weight or height, start a new layer
            if (pallet.TotalWeight + element.Weight > Pallets.StackingMaxWeight || 
                pallet.TotalHeight + element.Height > Pallets.StackingMaxHeight)
            {
                pallet.TotalWeight = 0;
                pallet.TotalHeight = 0;
        
                return pallet.elementsOnPallet.Max(e => e.LayerNumber) + 1;
            }

            return pallet.elementsOnPallet.Any() 
                ? pallet.elementsOnPallet.Max(e => e.LayerNumber) 
                : 1;
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
                unfitElements.Clear();
                OptimizePacking(elements);

                var output = packedPallets.GroupBy(p => p.Group.Name)
                    .Select(group => new
                    {
                        Tag = group.Key ?? "Untagged",
                        Pallets = group.Select(p => new
                        {
                            PalletType = p.PalletType.ToString(),
                            PalletGroup = p.Group.Name,
                            p.TotalWeight,
                            p.TotalHeight,
                            Layers = p.elementsOnPallet.GroupBy(e => e.LayerNumber)
                                .Select(layer => new
                                {
                                    LayerNumber = layer.Key,
                                    Slots = layer.Select(e => new
                                    { 
                                        e.Slot,
                                        e.Name,
                                        Rotation = e.IsRotated ? "Rotated" : "Original",
                                        e.Weight,
                                        e.Height
                                    }).ToList()
                                }).ToList()
                        }).ToList()
                    }).ToList();
                
                var unfitElementsOutput = unfitElements.Select(e => new
                {
                    e.Name,
                    e.Length,
                    e.Width,
                    e.Height,
                    e.Weight,
                    RotationRules = e.RotationRules.ToString(),
                    e.Tag
                }).ToList();
                var finalOutput = new
                {
                    PackedPallets = output,
                    UnfitElements = unfitElementsOutput
                };

                Debug.WriteLine("Generated JSON Output:");
                Debug.WriteLine(JsonSerializer.Serialize(finalOutput, new JsonSerializerOptions { WriteIndented = true }));

                return Json(finalOutput);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                return Json(new { success = false, message = "Unexpected error occurred." });
            }
        }
    }
}