using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public ServerTCP Server;
    public ClientTCP Client;
    public GameManager GameManager;
    public UIManager UIManager;

    [HideInInspector] public bool isHost;
    [HideInInspector] public bool isClient;

    private void Awake()
    {
        Application.quitting += CloseAllConnections;
    }

    void Start()
    {
        if (Server.enabled)
        {
            isHost = true;
        }

        if (Client.enabled)
        {
            isClient = true;
        }
    }

    private void FixedUpdate()
    {
        if (isHost)
        {
            if (GameManager.GameStatus == GameManager.MatchStatus.OnGoing)
            {
                if (Server.CountActiveClients() < GameManager.PlayersRequired)
                {
                    GameManager.Endmatch();
                }
                else
                {
                    var WorldSnapshot = GameManager.GenerateWorldSnapshot();
                    Server.SendWorldSnapshot(WorldSnapshot);
                }
            }
        }

        if (isClient)
        {
            if (UIManager.WaitingMenu.enabled)
            {
                if (Client.Status == ClientTCP.ClientStatus.NotConnected)
                {
                    UIManager.WaitingMenu.SetStatusText("Connecting to the server");
                    UIManager.WaitingMenu.ReadyButton.interactable = false;
                }
                else
                {
                    UIManager.WaitingMenu.SetStatusText("Waiting for the players");
                    UIManager.WaitingMenu.ReadyButton.interactable = true;

                    UIManager.WaitingMenu.SetPlayerStatusText(Client.PlayerCount, Client.Status == ClientTCP.ClientStatus.Ready);
                }
                
            }
        }
    }

    public void CloseAllConnections()
    {
        if (isHost)
        {
            Server.CloseSockets();
        }

        if (isClient)
        {
            Client.CloseClient();
        }
    }


}
