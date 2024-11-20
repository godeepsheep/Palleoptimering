using System.ComponentModel.DataAnnotations;

namespace PalletOptimization.Models
{
    public class PalletType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)] 
        public string Name { get; set; } = string.Empty; 
    }
}