using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XAYAMoverGame;
using UnityEngine.UI;
using MudHero.XayaCommunication;

public class WorldManager : MonoBehaviour
{
    public GameState gameState;
    public Text blockProgression;
    public Text playerCount;
    public GameObject moverPrefab;
    public XAYAClient client;
    public XAYAConnector connector;
    public XayaClient mudHeroClient;
    private bool needsUpdate = false;

    [SerializeField]
    string hostIP, hostPort, gameIP, gamePort, username, password, endPoint;

    void Start()
    {
        ConnectionInfo daemonInfo = new ConnectionInfo()
        {
            ip = hostIP,
            port = hostPort,
            username = username,
            userpassword = password,
            walletPassword = "test",
            endpointPath = endPoint
        };

        ConnectionInfo wrapperInfo = new ConnectionInfo()
        {
            ip = gameIP,
            port = gamePort,
            username = username,
            userpassword = password,
            walletPassword = "test",
            endpointPath = endPoint
        };

        string basePath = Application.dataPath + "\\..\\XayaStateProcessor\\";
        StateProcessorPathInfo pathInfo = new StateProcessorPathInfo(basePath, "", "database",
                                                                     "glogs");

        WrapperInitInputs wrapInputs = new WrapperInitInputs()
        {
            initCallback = CallbackFunctions.initialCallbackResult,
            forwardCallback = CallbackFunctions.forwardCallbackResult,
            rewindCallback = CallbackFunctions.backwardCallbackResult,
            pathInfo = pathInfo,
            daemonInfo = daemonInfo,
            wrapperInfo = wrapperInfo,
            storageType = "sqlite",
            gameNamespace = "mv",
            network = 0
        };
        mudHeroClient = new XayaClient();
        mudHeroClient.lastProcessedBlock = "e5062d76e5f50c42f493826ac9920b63a8def2626fd70a5cec707ec47a4c4651";
        try { mudHeroClient.ConnectClient(wrapInputs, Logger); }
        catch (Exception e) { Debug.Log(e.Message); }
        Debug.Log(mudHeroClient.IsConnected);
    }

    private void OnApplicationQuit()
    {
        mudHeroClient.Disconnect();
    }

    void Logger(string log) { File.AppendAllLines("DebugLog.txt", new string[] { log }); }

    public void SetGameState(GameState state, int currentHeight, int blockCount)
    {
        gameState = state;
        blockProgression.text = string.Format("{0}/{1}", currentHeight, blockCount);
        playerCount.text =state.players.Count.ToString();
        needsUpdate = true;
    }

    public void Update()
    {
        if (needsUpdate) 
            UpdatePlayers();

        /*if (Input.GetKeyDown(KeyCode.I))
            connector.LaunchMoverStateProcessor();
        else if (Input.GetKeyDown(KeyCode.C))
            client.Connect();*/
        if (Input.GetKeyDown(KeyCode.Escape))
            mudHeroClient.Disconnect();
        if (Input.GetKeyDown(KeyCode.N))
            Debug.Log(mudHeroClient.gameState);
    }

    void UpdatePlayers()
    {
        GameObject[] movers = GameObject.FindGameObjectsWithTag("Mover");

        PlayerState playerInfo;

        List<string> existingPlayers = new List<string>();

        if (movers != null)
        {
            for (int m = 0; m < movers.Length; m++)
            {
                if (gameState.players.TryGetValue(movers[m].GetComponent<Mover>().owner, out playerInfo))
                {
                    existingPlayers.Add(movers[m].GetComponent<Mover>().owner);
                    movers[m].GetComponent<Mover>().UpdatePosition(playerInfo.x, playerInfo.y);
                }
            }

            foreach(KeyValuePair<string, PlayerState> player in gameState.players)
            {
                if(!existingPlayers.Contains(player.Key))
                {
                    Mover spawnMover = Instantiate(moverPrefab).GetComponent<Mover>();
                    spawnMover.owner = player.Key;
                    spawnMover.UpdatePosition(player.Value.x, player.Value.y);
                }
            }
        }
    }
}
