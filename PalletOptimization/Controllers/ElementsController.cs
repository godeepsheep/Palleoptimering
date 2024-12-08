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

        
        

        //______________________________________________________________________________________
        //The Algorithm
        //______________________________________________________________________________________
        public void PreSortElements(List<Elements> inputList)
        {
            foreach (Elements element in inputList)
            {
                //get a list of all tags to be used later.
                //TODO: use it later.
                if (element.Tag != null)
                {
                    if (element.IsSpecial)
                    {
                        taggedItemsList.Add(new TaggedItem { tag = element.Tag, taggedElement = element, special = true });
                        inputList.Remove(element);
                    }
                    else
                    {
                        taggedItemsList.Add(new TaggedItem { tag = element.Tag, taggedElement = element });
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

            PlaceEOnPallets(PalletSizes);
        }

        public void PlaceEOnPallets(List<PalletSizeList> PalletSizes)
        {
            List<PackedPallet> PackedPallets = new();
            PackedPallet CurrentPallet = new();
            int currentWidth = 0;
            foreach (PalletSizeList elementList in PalletSizes)
            {
                CurrentPallet.Group = elementList.group;
                int maxWidth = elementList.group.Width;
                //Inside this foreach, we loop over each list of pallet sizes, and the lists of elements that can fit on those pallets.
                foreach (Elements element in elementList.ElementsFitOnPallet)
                {
                    //inside this foreach, we loop over the list inside the palletgroup. 
                    //just checks if the incoming element can fit on current pallet, if it can it get's added. if it can't, it will finish that pallet.
                    //this isn't perfect, but should be good enough.
                    if (currentWidth + element.Width < maxWidth)
                    {
                        RotateElementsOnPallets(element, CurrentPallet);
                        CurrentPallet.elementsOnPallet.Add(element);
                        currentWidth += element.Width;
                    }
                    //no more room on pallet. 
                    else
                    {
                        PackedPallets.Add(CurrentPallet);
                        CurrentPallet = new();
                        currentWidth = 0;
                        //try again
                        if (currentWidth + element.Width < maxWidth)
                        {
                            RotateElementsOnPallets(element, CurrentPallet);
                            CurrentPallet.elementsOnPallet.Add(element);
                            currentWidth += element.Width;
                        }

                    }
                }
                //this is to start fresh with the new batch. We could make it better by sorting the list first, so we start with the smallest items.
                //That way we can keep a "rest" element and check on each pallet. But since this is un-sorted, we'll just have a pallet with few items
                //on it if there's any remaning.
                if (CurrentPallet.elementsOnPallet.Count > 0)
                {
                    PackedPallets.Add(CurrentPallet);
                    CurrentPallet = new();
                    currentWidth = 0;
                }
            }

        }

        public void RotateElementsOnPallets(Elements element, PackedPallet currentPallet)
        {
            if (element.RotationRules == RotationOptions.NeedToRotate && element.Height != currentPallet.Group.Length + Pallets.MaxOverhang)
            {
                //switching values approach
                int temp = element.Height;
                element.Height = element.Length;
                element.Length = temp;

                /*
                 * bool approach
                 * element.upright = false;
                 * 
                 * string approach
                 * element.orientation = "Sideways";
                 */

            }
            if (element.RotationRules == RotationOptions.CanRotate &&
                element.Height < currentPallet.Group.Length + Pallets.MaxOverhang &&
                (float)element.Height / (float)element.Length > 1) //HWF
            {
                //rotate element
            }
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

