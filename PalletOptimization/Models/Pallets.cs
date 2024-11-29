namespace PalletOptimization.Models
{
    public static class Pallets
    {
        // Terrible naming from the assignment means that this is the PACKED pallet, and not the empty one.
        // Length, width, and height include the values of the empty pallet plus any overhang.
        // Height includes the total height of everything stacked on the pallet.

        public static int Id { get; set; } 
                
        // Description of the packed pallet
        public static string? PalletDescription { get; set; }

        public static int Length { get; set; } = 1200;// Total length (mm)
        public static int Width { get; set; } = 1200; // Total width (mm)
        public static int Height { get; set; } = 160; // Total height (mm, including contents)
        public static int Weight { get; set; } = 50;// Total weight (kg)

        public static int MaxHeight { get; set; } = 3000; // Max allowable height (mm)
        public static int MaxWeight { get; set; } = 4000;// Max allowable weight (kg)
        public static int MaxOverhang { get; set; } = 0; // Allowable overhang (mm)
        public static int SlotsOnPallet { get; set; } = 5; // Number of slots for elements
        public static int SpaceBetweenElements { get; set; } = 1; // Spacing between elements (mm)
        public static bool SpecialPallet { get; set; } = false; // Is it a special pallet?
        public static string? PalletGroup { get; set; } = string.Empty;
        public static string PalletType { get; set; } = string.Empty;
        public static int StackingMaxHeight { get; set; } = 2000; //mm 2 meter default
        public static int StackingMaxWeight { get; set; } = 50; //kg
        public static int Endplate { get; set; } = 1;
        public static bool Active { get; set; } = false; 
        
        // List of elements stacked on the pallet

        public static List<Elements> Elements { get; set; } = new List<Elements>();
    }
}