using PalletOptimization.Enums;
using PalletOptimization.Models;
using PalletOptimization.Utilities;

namespace PalletOptimization.Controllers;

public class PalletsController
{

    private DatabaseAccess DB;

    public PalletsController()
    {
        DB = DatabaseAccess.GetInstance();
    }
    
    public void CreatePallet(int palletLength, int palletWidth,
                                int palletHeight, int palletWeight, int MaxWeight)
    {
        string SQLQuery = $"INSERT INTO PalletGroups VALUES (,{palletLength}," +
            $"{palletWidth},{palletHeight},{palletWeight}, {MaxWeight})";
        DB.ExecuteQuery( SQLQuery );
    }

    public List<PalletGroupModel> GetPallets()
    {
        string SQLQuery = "SELECT * FROM PalletGroups";
        DB.ExecuteQuery( SQLQuery );
    }


}