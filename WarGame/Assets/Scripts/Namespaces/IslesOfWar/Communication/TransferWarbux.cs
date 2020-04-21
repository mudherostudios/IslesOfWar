namespace IslesOfWar.Communication
{
    public class TransferWarbux
    {
        public string plyr; //Player to send to.
        public float amnt; //Amount to send.

        public TransferWarbux() { }
        public TransferWarbux(string player, float amount)
        {
            plyr = player;
            amnt = amount;
        }
    }
}
