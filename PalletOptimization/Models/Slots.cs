namespace PalletOptimization.Models
{
    public class Slots
    {
        public int SlotNumber { get; set; } // 1 to 5
        public double Width { get; set; } // Calculated from pallet width
        public double RemainingWidth { get; set; }
        public List<Elements> Elements { get; set; } = new List<Elements>(); // Elements in this slot
        public double TotalWeight => Elements.Sum(e => e.Weight);
        public int TotalHeight => Elements.Sum(e => e.Height);
    }   
}