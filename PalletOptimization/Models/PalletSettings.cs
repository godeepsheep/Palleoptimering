namespace PalletOptimization.Models
{
    public class PalletSettings
    {
        public int DefaultLength { get; set; } = 1200;
        public int DefaultWidth { get; set; } = 1200;
        public int MaxHeight { get; set; } = 3000;
        public int MaxWeight { get; set; } = 4000;
        public int SlotsOnPallet { get; set; } = 5;
        public int SpaceBetweenElements { get; set; } = 1;
        public bool AllowOverhang { get; set; } = false;
        public int MaxOverhang { get; set; } = 0;
        public bool EndPlateAllowed { get; set; } = true; 
    }
}

