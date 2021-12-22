using System.Collections;
using UnityEngine;
using CielaSpike;
using System.IO;
using System;
using Newtonsoft.Json;
using BitcoinLib.Responses;
using XAYAMoverGame;
using XAYAWrapper;

public class XAYAConnector : MonoBehaviour
{
    public WorldManager manager;

    public string dPath = "";
    public string userCURL = "";
    public string user = "xaya";
    public string password = "bm4rxrmr8b";
    public string host = "http://127.0.0.1";
    public string hostPort = "8396";
    public string gameport = "8900";
    public int chain = 0;
    public string dataType = "lmdb";
    string lastProcessedBlock = "e5062d76e5f50c42f493826ac9920b63a8def2626fd70a5cec707ec47a4c4651"; //First main net block hash.

    bool fatalCheckPending = false;
    public XayaWrapper wrapper;
    public XAYAClient client;

    public static XAYAConnector Instance;

    public void LaunchMoverStateProcessor()
    {
        Instance = this;
        dPath = Application.dataPath;
        userCURL = "xaya:bm4rxrmr8b@127.0.0.1:8396";

        //Clean last session logs
        if (Directory.Exists(dPath + "\\..\\XayaStateProcessor\\glogs\\"))
        {
            DirectoryInfo di = new DirectoryInfo(dPath + "\\..\\XayaStateProcessor\\glogs\\");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        StartCoroutine(StartEnum());
    }

    /* We run daemon as coroutine on seperate thread, 
     * because else it will block the Unity main thread 
     * completely */
    IEnumerator StartEnum()
    {
        Task task;
        this.StartCoroutineAsync(DaemonAsync(), out task);
        yield return StartCoroutine(task.Wait());

        if (task.State == TaskState.Error)
        {
            if (MoveGUIAndGameController.Instance != null)
                MoveGUIAndGameController.Instance.ShowError(task.Exception.ToString());
            Debug.LogError(task.Exception.ToString());
        }

    }

    IEnumerator DaemonAsync()
    {
        string result = "";

        wrapper = new XayaWrapper(dPath + "\\..\\XayaStateProcessor\\", ref result, CallbackFunctions.initialCallbackResult, CallbackFunctions.forwardCallbackResult, CallbackFunctions.backwardCallbackResult);
        result += "\n" + wrapper.SetConnectInfo("127.0.0.1", "8396", "127.0.0.1", "8900", "xaya", "bm4rxrmr8b");

        yield return Ninja.JumpToUnity;
        Debug.Log(result);
        yield return Ninja.JumpBack;

        result = wrapper.Connect(chain.ToString(), dataType, "mv", dPath + "\\..\\XayaStateProcessor\\database\\", dPath + "\\..\\XayaStateProcessor\\glogs\\");

        yield return Ninja.JumpToUnity;
        Debug.Log(result);
        yield return Ninja.JumpBack;

        Debug.Log("Check if fatal?");

        CheckIfFatalError();
    }

    public void SubscribeForBlockUpdates()
    {
        StartCoroutine(WaitForChanges());
    }

    IEnumerator WaitForChangesInner()
    {
        while (true)
        {
            if (client.connected && wrapper != null)
            {
                wrapper.xayaGameService.WaitForChange(lastProcessedBlock);

                GameStateResult actualState = wrapper.xayaGameService.GetCurrentState();

                if (actualState != null)
                {
                    if (actualState.gamestate != null)
                    {
                        lastProcessedBlock = actualState.blockhash;
                        GameState state = JsonConvert.DeserializeObject<GameState>(actualState.gamestate);

                        if (MoveGUIAndGameController.Instance != null)
                        {
                            MoveGUIAndGameController.Instance.state = state;
                            MoveGUIAndGameController.Instance.totalBlock = client.GetTotalBlockCount();
                            var currentBlock = client.xayaService.GetBlock(actualState.blockhash);
                            MoveGUIAndGameController.Instance._sVal = currentBlock.Height;

                            MoveGUIAndGameController.Instance.needsRedraw = true;
                        }

                        if (manager != null)
                        {
                            int totalBlocks = client.GetTotalBlockCount();
                            int currentBlock = client.xayaService.GetBlock(actualState.blockhash).Height;
                            yield return Ninja.JumpToUnity;
                            manager.SetGameState(state, currentBlock, totalBlocks);
                            yield return Ninja.JumpBack;
                        }
                    }
                    else
                    {
                        Debug.LogError("Retuned state is not valid? We had some error with JSON");
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

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        StartCoroutine(TryAndStop());
        Instance = null;
    }

    // We check the glog files to see what the problem was and then we output the error to screen. No error handling in here.
    void WaitForFileAndCheck()
    {
        /* LETS EXTRACT TH INFO WE NEED FROM GLOG FILES*/
        string[] files = Directory.GetFiles(dPath + "\\..\\XayaStateProcessor\\glogs\\");
        for (int s = 0; s < files.Length; s++)
        {
            /* Glog might keep access control,
            *  so we must check using shared
            * access */

            if (files[s].Contains("FATAL"))
            {
                using (var fileStream = new FileStream(files[s], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    var content = textReader.ReadToEnd();
                    string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    if(MoveGUIAndGameController.Instance != null)
                        MoveGUIAndGameController.Instance.ShowError(lines[3]); // This sends the error message to the Unity error message box.
                    Debug.Log(lines[3]); // Wild guess, but seems like glog will keep error we ned right there always SO FAR // This goes to the Unity console.
                }
            }

        }
    }

    public void CheckIfFatalError()
    {
        fatalCheckPending = true; // 
    }

    private void Update()
    {
        if (fatalCheckPending)
        {
            fatalCheckPending = false;
            if (MoveGUIAndGameController.Instance != null)
                MoveGUIAndGameController.Instance.LaunchAborted();
            WaitForFileAndCheck();
        }
    }

    IEnumerator TryAndStop()
    {
        if (wrapper != null)
        {
            wrapper.Stop();
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(TryAndStop());
        }
    }

}
