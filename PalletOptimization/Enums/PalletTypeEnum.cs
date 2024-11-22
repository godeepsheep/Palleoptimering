namespace PalletOptimization.Enums
{
    public enum PalletTypeEnum
    {
        Wood = 0,
        Metal = 1,
        Plastic = 2
    }

    public static class PalletTypeHelper
    {
        public static double GetMultiplier(PalletTypeEnum type)
        {
            return type switch
            {
                PalletTypeEnum.Wood => 1.0,
                PalletTypeEnum.Metal => 1.5,
                PalletTypeEnum.Plastic => 0.9,
                
                //_ Means discard
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid pallet type")
            };
        }
    }
}
