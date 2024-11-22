using System.ComponentModel.DataAnnotations;

namespace PalletOptimization.Models
{
    public class PalletType
    {
        [Key]
        public int Id { get; set; } // Unique ID (0 for wood, 1 Metal, 2 Plastic)

        [Required]
        [MaxLength(50)] 
        public string Name { get; set; } = string.Empty; //Name like wood, etc.
    }
}