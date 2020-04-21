namespace IslesOfWar.ClientSide
{
    public struct IslandMessage
    {
        public Island island;
        public bool success;

        public IslandMessage(Island i, bool s)
        {
            island = i;
            success = s;
        }
    }
}
