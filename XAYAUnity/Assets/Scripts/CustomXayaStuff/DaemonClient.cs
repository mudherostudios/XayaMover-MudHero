using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitcoinLib.Responses;
using BitcoinLib.Services.Coins.XAYA;
using Vuteka;

public class DaemonClient : MonoBehaviour
{
    public GameStateProcessor gsp;
    public bool connected = false;
    public int current;
    public uint total;

    [HideInInspector]
    public IXAYAService xayaService;

    private ConnectionInfo connectionInfo;
    private string playerName = "";

    public void SetConnectionInfo(ConnectionInfo info)
    {
        connectionInfo = info;
        Debug.Log(connectionInfo.GetHTTPCompatibleURL(true));
    }

    public bool Connect()
    {
        xayaService = new XAYAService(connectionInfo.GetHTTPCompatibleURL(true), connectionInfo.username, connectionInfo.userpassword, "");

        if (xayaService.GetBlockCount() > 0)
        {
            connected = true;
            gsp.SubscribeForBlockUpdates();
            Debug.Log("Xaya Daemon Connection Established");
        }
        else
        {
            connected = false;
            Debug.Log("Failed Properly Establish Xaya Daemon Connection");
            Debug.Log(string.Format("Failed with {0} as connection point and {1} and {2} as credentials.",
                      connectionInfo.GetHTTPCompatibleURL(true), connectionInfo.username, connectionInfo.userpassword));
        }

        return connected;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetPlayerName(int nameIndex)
    {
        playerName = names[nameIndex];
    }

    public string ExecuteMove(string message)
    {
        if (xayaService != null)
        {
            return xayaService.NameUpdate(playerName, "{\"g\":{\"vuteka\":\"" + message +"\"}}", new object());
        }
        else
        {
            return "Xaya Service is not Initialized.";
        }
    }

    public int networkBlockCount
    {
        get
        {
            if (xayaService == null)
                return 0;
            else
                return (int)xayaService.GetBlockCount();
        }
    }

    public string[] names
    {
        get
        {
            List<string> nameList = new List<string>();
            List<GetNameListResponse> responses = xayaService.GetNameList();

            if (responses == null || xayaService == null)
                return new string[0];

            foreach (GetNameListResponse response in responses)
            {
                if (response.ismine)
                    nameList.Add(response.name);
            }

            return nameList.ToArray();
        }
    }
}
