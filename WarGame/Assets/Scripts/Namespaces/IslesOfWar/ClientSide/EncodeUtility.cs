using MudHero;

namespace IslesOfWar.ClientSide
{
    public static class EncodeUtility
    {
        private static int[][] decodeTable = new int[][]
        {
                new int[] {0, 0, 0},
                new int[] {1, 0, 0},
                new int[] {0, 2, 0},
                new int[] {0, 0, 3},
                new int[] {1, 2, 0},
                new int[] {1, 0, 3},
                new int[] {0, 2, 3},
                new int[] {1, 2, 3}
        };

        public static char[,] encodeTable = new char[,]
        {
                { ')', '!', '@', '#', '$', '%', '^', '&' },
                { '0', '1', '2', '3', '4', '5', '6', '7' },
                { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' },
                { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' },
        };


        public static char GetFeatureCode(int tileType, int resourceType)
        {
            if (tileType == -1 || resourceType == -1)
                return 'Z';

            return encodeTable[tileType + 1, resourceType];
        }

        public static char GetDefenseCode(int blockerType, int bunkerCombo)
        {
            if (blockerType == -1 || bunkerCombo == -1)
                return 'Z';

            return encodeTable[blockerType, bunkerCombo];
        }

        public static int[][] GetDefenseTypes(int blocker, int bunkers)
        {
            int[] blockerPart = new int[1];
            blockerPart[0] = blocker;

            int[] bunkerPart = GetBaseTypes(bunkers);

            int[][] defenses = new int[][] { blockerPart, bunkerPart };

            return defenses;
        }

        public static int[] GetBaseTypes(int type)
        {
            return Deep.CopyObject<int[]>(decodeTable[type]);
        }

        public static int GetXType(char type)
        {
            return GetXType(type.ToString());
        }

        public static int GetXType(string type)
        {
            int converted = -1;

            if (")0aA".Contains(type))
                converted = 0;
            else if ("!1bB".Contains(type))
                converted = 1;
            else if ("@2cC".Contains(type))
                converted = 2;
            else if ("#3dD".Contains(type))
                converted = 3;
            else if ("$4eE".Contains(type))
                converted = 4;
            else if ("%5fF".Contains(type))
                converted = 5;
            else if ("^6gG".Contains(type))
                converted = 6;
            else if ("&7hH".Contains(type))
                converted = 7;
            else
                converted = -1;

            return converted;
        }

        public static int GetYType(char type)
        {
            return GetYType(type.ToString());
        }

        public static int GetYType(string type)
        {

            if (")!@#$%^&".Contains(type))
                return 0;
            else if ("01234567".Contains(type))
                return 1;
            else if ("abcdefgh".Contains(type))
                return 2;
            else if ("ABCDEFGH".Contains(type))
                return 3;

            return -1;
        }

        public static int GetDecodeIndex(int[] set)
        {
            for (int i = 0; i < decodeTable.Length; i++)
            {
                bool foundIndex = true;

                for (int s = 0; s < set.Length; s++)
                {
                    if (decodeTable[i][s] != set[s])
                    {
                        s = set.Length;
                        foundIndex = false;
                    }
                }

                if (foundIndex)
                    return i;
            }

            return -1;
        }
    }
}
