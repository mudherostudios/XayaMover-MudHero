using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XAYAWrapper;
using Newtonsoft.Json;
using Vuteka;

public class VutekaManager : MonoBehaviour
{
    public GameObject linePrefab;

    //Connection Objects
    [Header("Connection Info")]
    public DaemonClient client;
    public GameStateProcessor gsp;
    public XayaWrapper wrapper;

    //Connection Info
    [SerializeField]
    private string username = "xaya",
        userpassword = "bm4rxrmr8b",
        daemonIP = "127.0.0.1",
        daemonPort = "8396",
        gspIP = "127.0.0.1",
        gspPort = "8900",
        walletPath = "/wallet/game.dat";

    [SerializeField]
    private int network = 0;


    public ConnectionInfo daemonInfo;
    public ConnectionInfo gspInfo;

    //Data Location Info
    [Header("File Path Information")]
    [SerializeField]
    private string libraryPath = "/../XayaStateProcessor/";
    [SerializeField]
    private string databasePath = "/../XayaStateProcessor/databases/";
    [SerializeField]
    private string errorLogPath = "/../XayaStateProcessor/glogs/";

    public StateProcessorPathInfo pathInfo;

    //Game State and Processing Info
    private WorldState worldState;
    private int processedBlocks;
    private uint networkHeight;

    [Header("GUI Variables")]
    public Text blockProgression;

    List<string> instantiatedIDs;
    
    public LineRenderer[] lines;
    Vector3[][] linePoints;

    private void Start()
    {
        linePoints = new Vector3[lines.Length][];

        for (int l = 0; l < linePoints.Length; l++)
        {
            linePoints[l] = new Vector3[lines[l].positionCount];

            for (int p = 0; p < linePoints[l].Length; p++)
            {
                linePoints[l][p] = lines[l].GetPosition(p);
            }
        }

        Structure structure = Utility.GetStructureFromVectors(linePoints);
        string s = JsonConvert.SerializeObject(structure);
        Debug.Log(s);
        SetVariables();
        InitializeDependents();
        instantiatedIDs = new List<string>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
            gsp.LaunchGSP();
        if (Input.GetKeyDown(KeyCode.Insert))
            client.Connect();
        if (Input.GetKeyDown(KeyCode.Delete))
            gsp.Disconnect();
    }

    public void UpdateState(WorldState processedState, int _processedBlocks, uint currentHeight)
    {
        worldState = processedState;
        processedBlocks = _processedBlocks;
        networkHeight = currentHeight;

        if (worldState.structures.Count != 0)
            Debug.Log(worldState.structures.Count);

        UpdateBlockProgression();
        UpdateStructures();
    }


    //Helper Organization Functions
    void SetVariables()
    {
        daemonInfo = new ConnectionInfo(daemonIP, daemonPort, walletPath, username, userpassword);
        gspInfo = new ConnectionInfo(gspIP, gspPort, walletPath, username, userpassword);
        pathInfo = new StateProcessorPathInfo(Application.dataPath, libraryPath, databasePath, errorLogPath);
    }

    void InitializeDependents()
    {
        client.SetConnectionInfo(daemonInfo);
        gsp.SetInfoVariables(daemonInfo, gspInfo, pathInfo, network);
    }

    void UpdateBlockProgression()
    {
        blockProgression.text = string.Format("{0}/{1}", processedBlocks.ToString(), networkHeight.ToString());
    }

    void UpdateStructures()
    {
        foreach (var pair in worldState.structures)
        {
            string ID = pair.Key;
            Structure structure = pair.Value;

            if (!instantiatedIDs.Contains(ID))
            {
                instantiatedIDs.Add(ID);
                List<GameObject> lineObjects = new List<GameObject>();
                Vector3 totalValues = Vector3.zero;
                int pointCounts = 0;

                foreach (Line line in structure.lines)
                {
                    Vector3[] linePoints = Utility.GetVectorPointsFromLine(line);

                    foreach (Vector3 point in linePoints)
                    {
                        totalValues += point;
                        pointCounts++;
                    }

                    GameObject lineObject = Instantiate(linePrefab, linePoints[0], Quaternion.identity);
                    lineObject.GetComponent<LineRenderer>().SetPositions(linePoints);
                    lineObjects.Add(lineObject);
                }

                totalValues = new Vector3(totalValues.x / pointCounts, totalValues.y / pointCounts, totalValues.z / pointCounts);
                GameObject gameStructure = Instantiate(new GameObject(), totalValues, Quaternion.identity);
                gameStructure.name = ID;
                
                foreach (GameObject obj in lineObjects)
                {
                    obj.transform.SetParent(gameStructure.transform);
                }
            }
        }
    }
}
