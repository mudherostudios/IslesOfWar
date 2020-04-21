namespace MudHero.XayaCommunication
{
    public struct ConnectionInfo
    {
        public string ip;
        public string port;
        public string endpointPath;
        public string username;
        public string userpassword;
        public string walletPassword;

        public ConnectionInfo(string _ip, string _port, string path, string user, string password, string _walletPassword)
        {
            ip = _ip;
            port = _port;
            username = user;
            userpassword = password;
            walletPassword = _walletPassword;
            endpointPath = path;
        }

        public string GetHTTPCompatibleIP()
        {
            return string.Format("http://{0}", ip);
        }

        public string GetCURL()
        {
            return string.Format("http://{0}:{1}@{2}:{3}", username, userpassword, ip, port);
        }

        public string GetHTTPCompatibleURL(bool withPath)
        {
            if (withPath)
                return string.Format("http://{0}:{1}{2}", ip, port, endpointPath);
            else
                return string.Format("http://{0}:{1}", ip, port);
        }
    }
}
