namespace PalletOptimization.Models
{
    public static class Pallets
    {
        // Terrible naming from the assignment means that this is the PACKED pallet, and not the empty one.
        // Length, width, and height include the values of the empty pallet plus any overhang.
        // Height includes the total height of everything stacked on the pallet.

        public static int Id { get; set; } 
                
        // Descstatic ription of the packed pallet
        public static string? PalletDescription { get; set; }

        public static int Length { get; set; } = 1000;// Total length (mm)
        public static int Width { get; set; } // Total width (mm)
        public static int Height { get; set; } // Total height (mm, including contents)
        public static int Weight { get; set; } // Total weight (kg)
                
        public static int MaxHeight { get; set; } // Max allowable height (mm)
        public static int MaxWeight { get; set; } // Max allowable weight (kg)
        public static int MaxOverhang { get; set; } = 0; // Allowable overhang (mm)
        public static int SlotsOnPallet { get; set; } = 5; // Number of slots for elements
        public static int SpaceBetweenElements { get; set; } // Spacing between elements (mm)
                
        // Groustatic p, Type, Special details
        public static bool SpecialPallet { get; set; } = false; // Is it a special pallet?
        public static string? PalletGroup { get; set; } 
        public static string PalletType { get; set; } = string.Empty; 
                
        public static bool Active { get; set; } = false; 
                
        // Liststatic  of elements stacked on the pallet
        public static List<Elements> Elements { get; set; } = new List<Elements>();
    }
}