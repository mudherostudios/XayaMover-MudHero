using System.IO;
using System.Collections;
using System.Collections.Generic;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;
using Newtonsoft.Json;
using CielaSpike;
using UnityEngine;
using XAYAWrapper;
using Vuteka;

public class GameStateProcessor : MonoBehaviour
{
    public DaemonClient xayaDaemon;
    public VutekaManager vutekaManager;
    public XayaWrapper wrapper;
    public ConnectionInfo daemonInfo;
    public ConnectionInfo gspInfo;
    public StateProcessorPathInfo pathInfo;
    bool fatalCheckPending = false;
    bool connected = false;
    int network = 0;
    string lastProcessedBlock = "e5062d76e5f50c42f493826ac9920b63a8def2626fd70a5cec707ec47a4c4651"; //First main net block hash

    private void Update()
    {
        if (fatalCheckPending)
        {
            fatalCheckPending = false;
            //Wait until logs have been checked;
        }
    }

    public void SetInfoVariables(ConnectionInfo _daemonConnectionInfo, ConnectionInfo _gspConnectionInfo , StateProcessorPathInfo _pathInfo, int net)
    {
        daemonInfo = _daemonConnectionInfo;
        gspInfo = _gspConnectionInfo;
        pathInfo = _pathInfo;
        network = net;
    }

    public void LaunchGSP()
    {
        CleanLogFolder();
        StartCoroutine(ForkGSP());
    }

    IEnumerator ForkGSP()
    {
        Task task;
        this.StartCoroutineAsync(GSPAsync(), out task);
        yield return StartCoroutine(task.Wait());

        if (task.State == TaskState.Error)
            Debug.LogError(task.Exception.ToString());
    }

    IEnumerator GSPAsync()
    {
        string result = "";
        wrapper = new XayaWrapper(pathInfo.library, ref result, Callbacks.initialCallback, Callbacks.forwardCallback, Callbacks.backwardCallback);
        result += "\n" + wrapper.SetConnectInfo(daemonInfo.ip, daemonInfo.port, gspInfo.ip, gspInfo.port, daemonInfo.username, daemonInfo.userpassword);

        yield return Ninja.JumpToUnity;
        Debug.Log(result);
        yield return Ninja.JumpBack;

        connected = true;
        result = wrapper.Connect(network.ToString(), "memory", "vuteka", pathInfo.database, pathInfo.logs);

        //This is blocked until disconnected.
        connected = false;

        yield return Ninja.JumpToUnity;
        Debug.Log(result);
        yield return Ninja.JumpBack;
    }

    public void SubscribeForBlockUpdates()
    {
        StartCoroutine(WaitForChanges());
    }

    IEnumerator WaitForChangesInner()
    {
        while (true)
        {
            if (xayaDaemon.connected && wrapper != null)
            {
                wrapper.xayaGameService.WaitForChange(lastProcessedBlock);

                GameStateResult actualState = wrapper.xayaGameService.GetCurrentState();

                if (actualState != null)
                {
                    if (actualState.gamestate != null)
                    {
                        lastProcessedBlock = actualState.blockhash;
                        WorldState worldState = JsonConvert.DeserializeObject<WorldState>(actualState.gamestate);
                        uint totalBlocks = xayaDaemon.xayaService.GetBlockCount();
                        int processedBlock = xayaDaemon.xayaService.GetBlock(actualState.blockhash).Height;

                        //I don't know if I need to actually jump out to set these.
                        yield return Ninja.JumpToUnity;
                        vutekaManager.UpdateState(worldState, processedBlock, totalBlocks);
                        yield return Ninja.JumpBack;
                    }
                    else
                    {
                        Debug.LogError("Returned state is not valid? We had some error with JSON.");
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
                yield return new WaitForSeconds(0.5f);
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

            foreach(FileInfo file in info.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
