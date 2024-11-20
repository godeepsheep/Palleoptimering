

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace PalletOptimization.Models
{
    public class Pallets
    {
        //terrible naming from the assignment means that this is the PACKED pallet, and not the empty one.
        //Length, width and height would then be the values of the empty pallet, plus any overhang.
        //And the height would be the total height of everything on it.
        public int id { get; set; }
        
        [StringLength(200)]
        public string? PalletDescription { get; set; }
        [Required]
        [Range(0, 5000)] //mm
        public required int Length { get; set; }
        [Required]
        [Range(0, 5000)] //mm
        public required int Width { get; set; }
        [Required]
        [Range(0, 5000)] //mm
        public required int Height { get; set; }
        [Required]
        [Range(0, 1000)] //kg
        public required int Weight { get; set; }
        [Required]
        [Range(0, 3000)] //mm
        public required int MaxHeight { get; set; }
        [Required]
        [Range(0, 2500)] //kg
        public required int MaxWeight { get; set; }
        [Required]
        [Range(0, 1000)] // 1000 mm = 1 m
        public required int MaxOverhang { get; set; } = 0;
        [Required]
        [Range(0,10)]
        public required int SlotsOnPallet { get; set; } = 5;
        [Required]
        [Range(0,1000)]
        public required int SpaceBetweenElements { get; set; }
        public bool SpecialPallet { get; set; } = false;
        [StringLength(50)]
        public string? PalletGroup { get; set; }
        [Required]
        [StringLength(50)]
        public required string PalletType { get; set; }
        public bool Active { get; set; } = false;

    }
}
