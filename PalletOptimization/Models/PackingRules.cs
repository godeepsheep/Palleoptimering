using System.ComponentModel.DataAnnotations;

namespace PalletOptimization.Models
{
    public class PackingRules
    {
        public int MaxLayers { get; set; } = 1;
        public int MaxWeightForRotation { get; set; }
        public float HeightWidthFactor { get; set; } //if height is larger, lay the element on the side.
        public bool HWFactorForSingleElement { get; set; }
        public int StackingMaxHeight { get; set; }
        public int StackingMaxWeight { get; set; }
        public bool EndPlateAllowed { get; set; } = true;
    }
}
