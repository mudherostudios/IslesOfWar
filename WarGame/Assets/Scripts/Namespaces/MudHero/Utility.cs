
namespace MudHero
{
    public static class HexToInt 
    {
        public static int Get(string hex)
        {
            bool canConvert = hex.Length <= 16 && hex.Length > 0;
            int value = 0;

            if (canConvert)
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            return value;
        }
    }
}
