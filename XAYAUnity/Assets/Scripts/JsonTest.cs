using System.IO;
using System.Collections;
using System.Collections.Generic;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;
using Vuteka;
using UnityEngine;
using XAYAWrapper;
using CielaSpike;

public class JsonTest : MonoBehaviour
{
    public string ip;
    public string port;
    public string gamePort;
    public string walletPath;
    public string user;
    public string password;
    public string libraryPath;
    public string databasePath;
    public string errorLogPath;

    ConnectionInfo xConInfo;
    ConnectionInfo gConInfo;
    StateProcessorPathInfo paths;
    XAYAService xayaService;
    XayaWrapper xayaWrapper;

    bool connected = false;

    private void Start()
    {
        xConInfo = new ConnectionInfo(ip, port, walletPath, user, password);
        gConInfo = new ConnectionInfo(ip, gamePort, "", user, password);
        paths = new StateProcessorPathInfo(Application.dataPath, libraryPath, databasePath, errorLogPath);

    }

    private void Update()
    {

    }

}
