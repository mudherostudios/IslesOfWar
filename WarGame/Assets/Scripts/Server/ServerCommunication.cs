using System;
using System.IO;
using MudHero.XayaCommunication;
using UnityEngine;

public class ServerCommunication : CommunicationInterface
{
    string cookieFolder, iowConfigFolder, xayaFolderParent, cookieFilePath, iowConfigFilePath;

    public void Start()
    {
        cookieFolder = "Xaya/.cookie";
        iowConfigFolder = "Xaya/iow.config";
        xayaFolderParent = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        cookieFilePath = Path.Combine(xayaFolderParent, cookieFolder);
        iowConfigFilePath = Path.Combine(xayaFolderParent, iowConfigFolder);
        server = true;

#if UNITY_SERVER
        Console.WriteLine("File Paths Set");
        Console.WriteLine(string.Format("Cookie Filepath: {0}",cookieFilePath));
        Console.WriteLine(string.Format("IOW Config Filepath: {0}",iowConfigFilePath));
#elif UNITY_EDITOR
        Debug.Log("File Paths Set");
        Debug.Log(string.Format("Cookie Filepath: {0}", cookieFilePath));
        Debug.Log(string.Format("IOW Config Filepath: {0}", iowConfigFilePath));
#endif
        Connect();
    }

    public void Connect()
    {
        ConnectionLog log = new ConnectionLog(false, "Could not even attempt to connect.");
        string user = GetXayaDaemonUsername();
        string pass = GetXayaDaemonUserPassword();
#if UNITY_SERVER
        Console.WriteLine(string.Format("Username: {0}",user));
        Console.WriteLine(string.Format("Password: {0}",pass));
#elif UNITY_EDITOR
        Debug.Log(string.Format("Username: {0} \n Password: {1} \n", user, pass));
#endif

        if (user != null && pass != null)
            log = Connect(user, pass, GetXayaDaemonWalletPassword());
        else
            log.message = "Your username or password returned null.";

        progressMessage = log.message;
#if UNITY_SERVER
        Console.WriteLine(progressMessage);
#elif UNITY_EDITOR
        Debug.Log(log.message);
#endif
    }

    string GetXayaDaemonUsername()
    {
        if (File.Exists(cookieFilePath))
            return GetConfigCredentials(cookieFilePath,0);
        else if (File.Exists(iowConfigFilePath))
            return GetConfigCredentials(iowConfigFilePath,0);
        else
            return null;
    }

    string GetXayaDaemonUserPassword()
    {
        if (File.Exists(cookieFilePath))
            return GetConfigCredentials(cookieFilePath, 1);
        else if (File.Exists(iowConfigFilePath))
            return GetConfigCredentials(iowConfigFilePath,1);
        else
            return null;
    }

    string GetXayaDaemonWalletPassword()
    {
        if (File.Exists(cookieFilePath))
            return null;
        else if (File.Exists(iowConfigFilePath))
            return GetConfigCredentials(iowConfigFilePath, 2);
        else
            return null;
    }

    string GetConfigCredentials(string filePath, int index)
    {
        string cookiePassword = "";
        string[] parsedValue = File.ReadAllText(filePath).Split(':');
        if (parsedValue.Length > index)
            cookiePassword = parsedValue[index];

        return cookiePassword;
    }
}
