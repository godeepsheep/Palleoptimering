using PalletOptimization.Enums;


namespace PalletOptimization.Models
{
    public class PalletGroupModel
    {
        public PalletGroup Group { get; set; }
        public int PalletLength { get; set; }
        public int PalletWidth { get; set; }
        public int PalletHeight { get; set; }
        public int PalletWeight { get; set; }
    }
}
