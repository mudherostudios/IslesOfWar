namespace MudHero.XayaCommunication
{
    public struct ConnectionLog
    {
        public bool success;
        public string message;

        public ConnectionLog(bool _success, string _message)
        {
            success = _success;
            message = _message;
        }

        public ConnectionLog(bool _success)
        {
            success = _success;
            message = string.Format("Success:{0}", success);
        }
    }
}
