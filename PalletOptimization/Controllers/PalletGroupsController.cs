using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Data;
using PalletOptimization.Models;

namespace PalletOptimization.Controllers
{
    public class PalletGroupsController : Controller
    {
        private readonly AppDbContext _context;

        public PalletGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PalletGroups
        public async Task<IActionResult> Index()
        {
            return View(await _context.PalletGroups.ToListAsync());
        }
        public IActionResult Planner()
        {
            return View();
        }

        // GET: PalletGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var palletGroup = await _context.PalletGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (palletGroup == null)
            {
                return NotFound();
            }

            return View(palletGroup);
        }

        // GET: PalletGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PalletGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Length,Width,Height,BaseWeight,MaxWeight")] PalletGroup palletGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(palletGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(palletGroup);
        }

        // GET: PalletGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var palletGroup = await _context.PalletGroups.FindAsync(id);
            if (palletGroup == null)
            {
                return NotFound();
            }
            return View(palletGroup);
        }

        // POST: PalletGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Length,Width,Height,BaseWeight,MaxWeight")] PalletGroup palletGroup)
        {
            if (id != palletGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(palletGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PalletGroupExists(palletGroup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(palletGroup);
        }

        // GET: PalletGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var palletGroup = await _context.PalletGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (palletGroup == null)
            {
                return NotFound();
            }

            return View(palletGroup);
        }

        // POST: PalletGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var palletGroup = await _context.PalletGroups.FindAsync(id);
            if (palletGroup != null)
            {
                _context.PalletGroups.Remove(palletGroup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PalletGroupExists(int id)
        {
            return _context.PalletGroups.Any(e => e.Id == id);
        }
    }
}
