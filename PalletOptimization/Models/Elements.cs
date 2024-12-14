using System.ComponentModel.DataAnnotations;
using PalletOptimization.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PalletOptimization.Models
{
    public class Elements
    {
        [Key] 
        public int Id { get; set; }
        
        [NotMapped]
        public Guid InstanceId { get; set; } = Guid.NewGuid(); // Unique id for each instance in an order (saved in memory) 
        
        public string Name { get; set; }
        public RotationOptions RotationRules { get; set; }
        public bool IsSpecial { get; set; }
        public string? Tag { get; set; } = null;
        public int? MaxElementsPerPallet { get; set; } = null;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        [NotMapped]
        public bool IsRotated { get; set; }
        [NotMapped]
        public int LayerNumber { get; set; } = 0;
        [NotMapped]
        public int Slot { get; set; } = 0;
        public double HeightWidthFactor { get; set; } = 1.0;
        
        public (int RotatedLength, int RotatedWidth) GetRotatedDimensions() 
            => RotationRules == RotationOptions.CanRotate 
                ? (Width, Length) 
                : (Length, Width);
    }
}