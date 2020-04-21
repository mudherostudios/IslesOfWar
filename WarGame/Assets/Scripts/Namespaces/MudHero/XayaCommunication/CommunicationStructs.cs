namespace MudHero.XayaCommunication
{
    public struct StateProcessorPathInfo
    {
        public string basePath { get; set; }
        private string libraryPath;
        private string databasePath;
        private string errorLogPath;

        public StateProcessorPathInfo(string _basePath, string libPath, string dbPath, string logPath)
        {
            basePath = _basePath;
            libraryPath = libPath;
            databasePath = dbPath;
            errorLogPath = logPath;
        }

        public string library { get { return basePath + libraryPath; } }
        public string database { get { return basePath + databasePath; } }
        public string logs { get { return basePath + errorLogPath; } }
    }
}
