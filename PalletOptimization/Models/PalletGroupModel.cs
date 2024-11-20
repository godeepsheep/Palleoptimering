using PalletOptimization.Enums;


namespace PalletOptimization.Models
{
    public class PalletGroupModel
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BaseWeight { get; set; }
        public int BaseMaxWeight { get; set; }
    }
}
