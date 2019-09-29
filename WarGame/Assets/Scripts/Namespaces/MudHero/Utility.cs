using System.Collections.Generic;
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

    public static class Deep
    {
        public static int[] Copy(int[] original)
        {
            int[] copy = new int[original.Length];

            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = original[i];
            }

            return copy;
        }

        public static int[][] Convert(List<List<int>> original)
        {
            int[][] converted = new int[original.Count][];

            for (int c = 0; c < converted.Length; c++)
            {
                converted[c] = original[c].ToArray();
            }

            return converted;
        }

        public static int[] Merge(int[] a, int[] b)
        {
            int[] biggest = Copy(a);
            int[] smallest = Copy(b);

            if (a.Length < b.Length)
            {
                biggest = Copy(b);
                smallest = Copy(a);
            }

            for (int i = 0; i < smallest.Length; i++)
            {
                biggest[i] += smallest[i];
            }

            return biggest;
        }
    }
}
