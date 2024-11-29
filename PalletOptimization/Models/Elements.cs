using System.ComponentModel.DataAnnotations;
using Microsoft.Build.Framework;
using PalletOptimization.Enums;

namespace PalletOptimization.Models
{
    public class Elements
    {
        [Key] 
        public int Id { get; set; }
        
        public string Name { get; set; }
        public RotationOptions RotationRules { get; set; }
        public bool IsSpecial { get; set; }
        public int? MaxElementsPerPallet { get; set; } = null;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public double? HeightWidthFactor { get; set; }
              
    }
}