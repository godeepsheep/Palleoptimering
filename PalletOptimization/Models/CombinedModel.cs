namespace PalletOptimization.Models
{
    public class CombinedModel
    {
        public Elements Elements { get; set; }
        public int MaxHeight { get; set; } = 3000; // Max allowable height (mm)
        public int MaxWeight { get; set; } = 4000;// Max allowable weight (kg)
        public int MaxOverhang { get; set; } = 0; // Allowable overhang (mm)
        public int SlotsOnPallet { get; set; } = 5; // Number of slots for elements
        public int SpaceBetweenElements { get; set; } = 1; // Spacing between elements (mm)
        public int StackingMaxHeight { get; set; } = 2000; //mm 2 meter default
        public int StackingMaxWeight { get; set; } = 50; //kg
        public int Endplate { get; set; } = 1;
    
    }
}
