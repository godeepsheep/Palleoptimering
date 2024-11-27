using System.ComponentModel.DataAnnotations;

namespace PalletOptimization.Models
{
    public class PalletGroup
    {
        [Key] // Auto-increments in the database
        public int Id { get; set; }
        public string Name { get; set; }

        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BaseWeight { get; set; }
        public int MaxWeight { get; set; }
    }
}