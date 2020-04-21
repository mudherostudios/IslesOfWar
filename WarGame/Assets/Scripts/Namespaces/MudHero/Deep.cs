using System.Collections.Generic;
using Newtonsoft.Json;

namespace MudHero
{
    public static class Deep
    {
        public static T CopyObject<T>(object original)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(original));
        }

        public static List<double> ConvertToDoubleList<T>(List<T> original)
        {
            if (typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(int))
                return JsonConvert.DeserializeObject<List<double>>(JsonConvert.SerializeObject(original));
            else
                return new List<double>(new double[original.Count]);
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
