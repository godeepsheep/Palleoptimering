﻿using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PalletOptimization.Data;
using PalletOptimization.Enums;
using PalletOptimization.Models;
using System.Text.Json;
using System.Xml.Linq;

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






        //______________________________________________________________________________________
        //The Algorithm
        //______________________________________________________________________________________
        public void PreSortElements(List<Elements> inputList)
        {
            foreach (Elements element in inputList)
            {
                //get a list of all tags to be used later.
                //TODO: use it later.
                if (element.tag != null)
                {
                    if (element.IsSpecial)
                    {
                        taggedItemsList.Add(new TaggedItem { tag = element.tag, taggedElement = element, special = true });
                        inputList.Remove(element);
                    }
                    else
                    {
                        taggedItemsList.Add(new TaggedItem { tag = element.tag, taggedElement = element });
                        inputList.Remove(element);
                    }
                    continue;
                }
                //if an element is special, remove it from the list of pallets to be sorted, as it will get its own.
                if (element.IsSpecial)
                {
                    packedPallets.Add(new PackedPallet { elementsOnPallet = new List<Elements> { element }, specialPallet = true });
                    inputList.Remove(element);
                    continue;
                }
            }
            SortIntoSizes(inputList);
        }

        public void SortIntoSizes(List<Elements> PreSortedList)
        {
            //make the list containing lists. there's a list on the class "PalletSizeList".
            List<PalletSizeList> PalletSizes = new();
            _context.PalletGroups.ToList().ForEach(p => PalletSizes.Add(new PalletSizeList { group = p }));

            //sort the inputlist into the different smallest pallet it can be on.
            foreach (Elements element in PreSortedList)
            {
                //start with high number, so the first call will set it to the first pallets length.
                //The default value is 0, and nothing is smaller than that, so we need to set it high first.
                int SmallestLength = 100000;
                int index = -1;

                for (int j = 0; j < PalletSizes.Count; j++)
                {
                    //max overhang added on the pallet length, to get the longest the element can stick out over the pallet.
                    if (element.Length < PalletSizes[j].group.Length + Pallets.MaxOverhang && SmallestLength > PalletSizes[j].group.Length)
                    {
                        SmallestLength = PalletSizes[j].group.Length;
                        index = j;
                    }
                }
                //if the if statement didn't get run in the for loop, then dont add the element, and just print out an error.
                if (index != -1)
                    PalletSizes[index].ElementsFitOnPallet.Add(element);
                else
                    System.Diagnostics.Debug.WriteLine("No pallet fit the element");
            }
            //from here, the inputs are sorted into lists of the pallets they fit on.

            //next step is sorting them into pallets, hard part is the maxElementsPerPallet (MEPP) rule should be respected, but also the total width of the pallet.
            //also, how does an element with 5 MEPP work with a 2 MEPP element? Need to talk about how we handle this coming part.
            //should we sort the duplicate elements first, and look at the MEPP, or do we mix elements with different MEPP. And in that case, how would that work?
            //if we have 2 MEPP and 5 MEPP, do we have one of the 2 MEPP, and just one of the 5 MEPP. so in other words, respect the lowest number?
            
            NextStep(PalletSizes);
        }

        public void NextStep(List<PalletSizeList> PalletSizes)
        {
            //TODO: look at how many elements can be on one pallet. Rotation will come after they have their pallets, as the width wont change with rotation.
           
        }


        public void Output()
        {
            //This is a very rough draft of how the output method would look. Writing some of it now to visualise how we will handle the special pallets.
            //normal pallets first,then special pallets later
            List<PackedPallet> specialPallets = new();
            foreach (PackedPallet packedPallet in packedPallets)
            {
                if (packedPallet.specialPallet)
                {
                    specialPallets.Add(packedPallet);
                    continue;
                }

                //TODO: write out the pallets
                /*
                 * 
                 * write the output
                 * end first foreach loop
                 * 
                 * then
                 * 
                 * foreach(PackedPallet special in specialPallets){
                 * write output 
                 * }
                 * 
                 */

            }





        }


    }
}

