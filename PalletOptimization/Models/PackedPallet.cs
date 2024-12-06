using PalletOptimization.Enums;

namespace PalletOptimization.Models
{
    public class PackedPallet
    {
        // List of elements stacked on the pallet
        public List<Elements> elementsOnPallet { get; set; } = new();
        public int TotalWeight { get; set; }
        public int TotalHeight { get; set; }
        public bool specialPallet { get; set; }
        public PalletGroupEnum PalletGroup { get; set; }
        public PalletTypeEnum PalletType { get; set; }

    }
}
