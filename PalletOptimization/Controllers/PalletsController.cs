using PalletOptimization.Data;
using PalletOptimization.Enums;
using PalletOptimization.Models;
using PalletOptimization.Utilities;
using PalletGroup = PalletOptimization.Models.PalletGroup;

namespace PalletOptimization.Controllers;

public class PalletsController
{

    public void Read()
    {
        //List all PalletGroups
        using var context = new AppDbContext();
        var PalletGroup = context.PalletGroups.ToList();
        //TODO: Exchange this with view
        foreach (var pallet in PalletGroup)
        {
            Console.WriteLine(pallet.Height);
        }
        
        

    }



    public void Delete(string Name)
    {
        using AppDbContext x = AppDbContext;
        var palletGroup = db.PalletGroups.Remove(p => p.ID = Enum.TryParse<PalletGroup>(Name));
    }
}