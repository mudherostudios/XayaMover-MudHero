using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XAYAMoverGame;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
    public GameState gameState;
    public Text blockProgression;
    public Text playerCount;
    public GameObject moverPrefab;
    public XAYAClient client;
    public XAYAConnector connector;
    private bool needsUpdate = false;

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

        if (Input.GetKeyDown(KeyCode.I))
            connector.LaunchMoverStateProcessor();
        else if (Input.GetKeyDown(KeyCode.C))
            client.Connect();
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
