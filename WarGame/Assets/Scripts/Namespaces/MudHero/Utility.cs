using System.Collections.Generic;
using Newtonsoft.Json;

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
        public static T CopyObject<T>(object original)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(original));
        }

        public static T[][] Convert<T>(List<List<T>> original)
        {
            T[][] converted = new T[original.Count][];

            for (int c = 0; c < converted.Length; c++)
            {
                converted[c] = original[c].ToArray();
            }

            return converted;
        }

        public static int[] Merge(int[] a, int[] b)
        {
            int[] biggest = CopyObject<int[]>(a);
            int[] smallest = CopyObject<int[]>(b);

            if (a.Length < b.Length)
            {
                biggest = CopyObject<int[]>(b);
                smallest = CopyObject<int[]>(a);
            }

            for (int i = 0; i < smallest.Length; i++)
            {
                biggest[i] += smallest[i];
            }

            return biggest;
        }
    }
}
