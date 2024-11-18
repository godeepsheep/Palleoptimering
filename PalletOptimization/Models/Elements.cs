using PalletOptimization.Enums;
namespace PalletOptimization.Models
{
    public class Elements
    {
        public RotationOptions RotationOptions { get; set; }
        public bool SpecialPallet { get; set; }
        public PalletType PalletType { get; set; }
        public int? MaxElementsPerPallet { get; set; } = null;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; } 
    }
}
