using PalletOptimization.Enums;
using PalletOptimization.Models;
using PalletOptimization.Enums;

namespace PalletOptimization.Models
{
    public class ElementsDto
    {
        public Guid InstanceId { get; set; }
        public RotationOptions RotationRules { get; set; }
        public bool IsSpecial { get; set; }
        public int? MaxElementsPerPallet { get; set; }
        public double HeightWidthFactor { get; set; }
    }
}
