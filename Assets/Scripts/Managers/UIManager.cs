using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public GameObject MessageCanvas;
    public GameObject MenuCanvas;

    public MainMenu MainMenu;
    public WaitingMenu WaitingMenu;
    public ConnectMenu ConnectMenu;

    public GameObject Level;

    private void Awake()
    {
        MessageCanvas.SetActive(false);
        MenuCanvas.SetActive(true);

        MainMenu.MainObject.SetActive(true);
        WaitingMenu.MainObject.SetActive(false);
        ConnectMenu.MainObject.SetActive(false);

        WaitingMenu.GameStatusText.text = "";
        WaitingMenu.PlayerStatusText.text = "";

        //Level.SetActive(false);
    }
}
