using System.IO;
using System.Collections;
using BitcoinLib.Responses;
using CielaSpike;
using UnityEngine;
using IslesOfWar.GameStateProcessing;

namespace MudHero
{
    namespace XayaCommunication
    {
        public class GameStateRetriever : MonoBehaviour
        {
            public CommunicationInterface communicator;
            public XayaWrapper wrapper;
            public ConnectionInfo daemonInfo;
            public ConnectionInfo gsrInfo;
            public StateProcessorPathInfo pathInfo;
            bool connected = false;
            int network = 0;
            string gameNamespace = "";
            string storageType = "";

            public void SetInfoVariables(ConnectionInfo _daemonConnectionInfo, ConnectionInfo _gsrConnectionInfo, StateProcessorPathInfo _pathInfo, int net, string storage, string game)
            {
                daemonInfo = _daemonConnectionInfo;
                gsrInfo = _gsrConnectionInfo;
                pathInfo = _pathInfo;
                network = net;
                gameNamespace = game;
                storageType = storage;
            }

            public void LaunchGSR()
            {
                CleanLogFolder();
                StartCoroutine(ForkGSR());
            }

            IEnumerator ForkGSR()
            {
                Task task;
                this.StartCoroutineAsync(GSRAsync(), out task);
                yield return StartCoroutine(task.Wait());

                if (task.State == TaskState.Error)
                    Debug.LogError(task.Exception.ToString());
            }

            IEnumerator GSRAsync()
            {
                string result = "";
                wrapper = new XayaWrapper(pathInfo.library, ref result, Callback.SetGenesisInfo, Callback.PlayState, Callback.RewindState);
                result += "\n" + wrapper.SetConnectInfo(daemonInfo.ip, daemonInfo.port, gsrInfo.ip, gsrInfo.port, daemonInfo.username, daemonInfo.userpassword);

                yield return Ninja.JumpToUnity;
                Debug.Log(result);
                yield return Ninja.JumpBack;

                connected = true;
                result = wrapper.Connect(network.ToString(), storageType, gameNamespace, pathInfo.database, pathInfo.logs);

                //This is blocked until disconnected.
                connected = false;

                yield return Ninja.JumpToUnity;
                Debug.Log(result);
                yield return Ninja.JumpBack;
            }

            public void SubscribeForBlockUpdates()
            {
                Debug.Log("Subscribed.");
                StartCoroutine(WaitForChanges());
            }

            IEnumerator WaitForChangesInner()
            {
                while (true)
                {
                    if (communicator.isConnectedToXayaDaemon && wrapper != null)
                    {
                        wrapper.xayaGameService.WaitForChange();

                        GameStateResult actualState = wrapper.xayaGameService.GetCurrentState();

                        if (actualState != null)
                        {
                            if (actualState.gamestate != null)
                            {
                                yield return Ninja.JumpToUnity;
                                //communicator.UpdateBlockProgress(actualState.blockhash, actualState.gamestate); 
                                yield return Ninja.JumpBack;
                            }

                            yield return null;
                        }
                        else
                        {
                            Debug.LogError("actualState is null");
                        }

                    }
                    else
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                }
            }

            IEnumerator WaitForChanges()
            {
                Task task;
                this.StartCoroutineAsync(WaitForChangesInner(), out task);
                yield return StartCoroutine(task.Wait());
            }

            private void OnApplicationQuit()
            {
                Disconnect();
            }

            public void Disconnect()
            {
                StartCoroutine(TryStop());
            }

            IEnumerator TryStop()
            {
                if (connected)
                {
                    wrapper.Stop();
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(TryStop());
                }
            }

            void CleanLogFolder()
            {
                if (Directory.Exists(pathInfo.logs))
                {
                    DirectoryInfo info = new DirectoryInfo(pathInfo.logs);

                    foreach (FileInfo file in info.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }

            public bool isConnected
            {
                get { return connected; }
            }
        }
    }
}