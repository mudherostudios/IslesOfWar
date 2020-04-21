namespace IslesOfWar.ClientSide
{
    public static class IslandBuildUtility
    {
        public static bool CanBuildCollectorOnFeature(char feature, char existing, char ordered)
        {
            int featureType = EncodeUtility.GetXType(feature);
            int existingType = EncodeUtility.GetXType(existing);
            int orderedType = EncodeUtility.GetXType(ordered);

            if (featureType > 0 && orderedType > 0 && existingType != orderedType)
            {
                int[] possible = EncodeUtility.GetBaseTypes(featureType);
                int[] exists = EncodeUtility.GetBaseTypes(existingType);
                int[] orders = EncodeUtility.GetBaseTypes(orderedType);

                bool canBuild = true;

                for (int b = 0; b < possible.Length && canBuild; b++)
                {
                    canBuild = possible[b] == orders[b] && exists[b] != orders[b] || orders[b] == 0;
                }

                return canBuild;
            }
            else if (orderedType == 0)
                return true;

            return false;
        }

        public static bool CanBuildDefenses(char existing, char ordered)
        {
            return CanBuildBlocker(existing, ordered) && CanBuildBunkers(existing, ordered);
        }

        public static bool CanBuildBunkers(char existing, char ordered)
        {
            int existingBunkerType = EncodeUtility.GetXType(existing);
            int orderedBunkerType = EncodeUtility.GetXType(ordered);

            bool canBuild = true;
            int bunkers = 0;

            int[] existingBunkers = EncodeUtility.GetBaseTypes(existingBunkerType);
            int[] orderedBunkers = EncodeUtility.GetBaseTypes(orderedBunkerType);

            for (int d = 0; d < existingBunkers.Length && canBuild; d++)
            {
                if (existingBunkers[d] > 0 || orderedBunkers[d] > 0)
                    bunkers++;

                canBuild = bunkers <= 2 && (orderedBunkers[d] != existingBunkers[d] || orderedBunkers[d] == 0);
            }

            return canBuild;
        }

        public static bool CanBuildBlocker(char existing, char ordered)
        {
            int existingBlockerType = EncodeUtility.GetYType(existing);
            int orderedBlockerType = EncodeUtility.GetYType(ordered);

            return (existingBlockerType == 0 && orderedBlockerType > 0) || orderedBlockerType == 0;
        }
    }
}
