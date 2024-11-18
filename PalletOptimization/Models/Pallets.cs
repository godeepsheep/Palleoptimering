

namespace PalletOptimization.Models
{
    public class Pallets
    {
        //terrible naming from the assignment means that this is the PACKED pallet, and not the empty one.
        //Length, width and height would then be the values of the empty pallet, plus any overhang.
        //And the height would be the total height of everything on it.
        public int id { get; set; }
        public string? PalletDescription { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int MaxHeight { get; set; }
        public int MaxWeight { get; set; }
        public int MaxOverhang { get; set; } = 0;
        public int SlotsOnPallet { get; set; } = 5;
        public int SpaceBetweenElements { get; set; }
        public bool SpecialPallet { get; set; }
        public string? PalletGroup { get; set; }
        public required string PalletType { get; set; }
        public bool Active { get; set; } = false;

    }
}
