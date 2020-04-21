namespace IslesOfWar.Combat
{
    public struct SquadMessage
    {
        public Squad squad;
        public bool success;

        public SquadMessage(Squad _squad, bool s)
        {
            squad = _squad;
            success = true;
        }
    }
}
