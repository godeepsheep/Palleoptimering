﻿using PalletOptimization.Data;
using PalletOptimization.Enums;
using PalletOptimization.Models;

namespace PalletOptimization.Controllers;

public class PalletsController
{
    private readonly AppDbContext _context;
    //We registered AppDbContext in Program.cs and told it the following: builder.Services.AddDbContext<AppDbContext>
    //The DI looks at the constructor, sees that it needs an AppDbContext, uses the registration in Program.cs to create an AppDbContext and injects it here.
    // Constructor injection for AppDbContext
    public PalletsController(AppDbContext context)
    {
        //the _ is to indicate that its private. Apparently its better for readability. 
        //So we are storing a private AppDbContext instance in _context that is injected into the controller by the DI. 
        _context = context;
    }

    public void Read()
    {
        // List all PalletGroups
        var palletGroups = _context.PalletGroups.ToList();
        
        // TODO: Exchange this with a view
        foreach (var pallet in palletGroups)
        {
            Console.WriteLine("gay");
        }
    }

    //Exchange ID with ENUM name for readdability, So the string value, matches the id in the database 
    public void DeletePalletGroupById(int id)
    {
        // Find the PalletGroup by ID
        var palletGroup = _context.PalletGroups.Find(id);

        if (palletGroup != null) // Check if the entry exists
        {
            _context.PalletGroups.Remove(palletGroup); // Remove the entry
            _context.SaveChanges(); // Save changes to the database
            Console.WriteLine($"PalletGroup with ID {id} deleted successfully.");
        }
        else
        {
            Console.WriteLine($"PalletGroup with ID {id} not found.");
        }
    }
    

    public void Delete(string Name)
    {
        using AppDbContext x = AppDbContext;
        var palletGroup = db.PalletGroups.Remove(p => p.ID = Enum.TryParse<PalletGroup>(Name));
    }
}