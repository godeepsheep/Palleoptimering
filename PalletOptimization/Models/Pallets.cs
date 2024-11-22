namespace PalletOptimization.Models
{
    public class Pallets
    {
        // Terrible naming from the assignment means that this is the PACKED pallet, and not the empty one.
        // Length, width, and height include the values of the empty pallet plus any overhang.
        // Height includes the total height of everything stacked on the pallet.

        public int Id { get; set; } 

        // Description of the packed pallet
        public string? PalletDescription { get; set; }

        public int Length { get; set; } // Total length (mm)
        public int Width { get; set; } // Total width (mm)
        public int Height { get; set; } // Total height (mm, including contents)
        public int Weight { get; set; } // Total weight (kg)
        
        public int MaxHeight { get; set; } // Max allowable height (mm)
        public int MaxWeight { get; set; } // Max allowable weight (kg)
        public int MaxOverhang { get; set; } = 0; // Allowable overhang (mm)
        public int SlotsOnPallet { get; set; } = 5; // Number of slots for elements
        public int SpaceBetweenElements { get; set; } // Spacing between elements (mm)
        
        // Group, Type, Special details
        public bool SpecialPallet { get; set; } = false; // Is it a special pallet?
        public string? PalletGroup { get; set; } 
        public string PalletType { get; set; } = string.Empty; 

        public bool Active { get; set; } = false; 

        // List of elements stacked on the pallet
        public List<Elements> Elements { get; set; } = new List<Elements>();
    }
}