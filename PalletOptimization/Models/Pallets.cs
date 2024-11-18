

namespace PalletOptimization.Models
{
    public class Pallets
    {
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
        public bool SpecialPalelt { get; set; }
        public string? PalletGroup { get; set; }
        public required string PalletType { get; set; }
        public bool Active { get; set; } = false;

    }
}
