namespace PalletOptimization.Models
{
    public class PalletSizeList
    {
        public PalletGroup group { get; set; }
        public List<Elements> ElementsFitOnPallet { get; set; } = new List<Elements>();

    }
}
