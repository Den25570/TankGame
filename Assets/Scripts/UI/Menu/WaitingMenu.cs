using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingMenu : MonoBehaviour
{
    public UIManager UIManager;
    public GameObject MainObject;

    public Text PlayerStatusText;
    public Text GameStatusText;
    public Button ReadyButton; 

    private string basicStatusText;
    private int textUpdates = 0;
    private int nextUpdate = 1;

    private void Start()
    {
        basicStatusText = "";
        textUpdates = 0;
    }

    private void Update()
    {
        if (Time.time >= nextUpdate)
        {
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            GameStatusText.text = basicStatusText + new string('.', textUpdates++);
            textUpdates %= 4;
        }
        
    }

    private IEnumerator WaitForSecond()
    {
        yield return new WaitForSeconds(1f);
    }

    public void SetPlayerStatusText(int playerCount, bool playerReady)
    {
        PlayerStatusText.text = "Players: " + playerCount;
        PlayerStatusText.text += "\n";
        PlayerStatusText.text += playerReady ? "You are not ready" : "You are ready";
    }

    public void SetStatusText(string text)
    {
        basicStatusText = text;
        if (GameStatusText.text == "")
            GameStatusText.text = basicStatusText;
    }

    public void CloseConnection()
    {
        UIManager.NetworkManager.Client.CloseClient();
    }
}
