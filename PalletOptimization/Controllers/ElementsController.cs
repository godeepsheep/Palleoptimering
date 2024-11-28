﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;
using System.Security.Cryptography;
using System.Xml.Linq;
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
            // Initialize an empty list of elements if no model is passed
            var elements = new List<Elements>();
            return View(elements);
        }

        public async Task<IActionResult> MakeOrder()
        {
            var elements = new List<Elements>();
            var random = new Random();

            int count = random.Next(1, 15); // Generate 1-15 random elements
            for (int i = 0; i < count; i++)
            {
                int id = random.Next(21, 40); // Random IDs (assumes 1-10 range)
                var element = await _context.Elements.FirstOrDefaultAsync(m => m.Id == id);
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return View("Planner", elements); // Pass the list of elements to the view
        }

        [HttpPost]
        public IActionResult UpdateRotationRule(int ElementId, RotationOptions SelectedRotationRule, List<Elements> elements)
        {
            // Find the element to update
            var element = elements.FirstOrDefault(e => e.Id == ElementId);
            if (element != null)
            {
                // Update the RotationRules value
                element.RotationRules = SelectedRotationRule;
            }

            // Return the updated list to the view
            return RedirectToAction("Planner", new { elements });
        }

       
        

    }
}
