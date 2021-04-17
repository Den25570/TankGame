using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectMenu : MonoBehaviour
{
    public UIManager UIManager;

    public GameObject MainObject;
    public InputField IpInputField;

    public void TryConnectToServer()
    {
        try
        {
            if (UIManager.NetworkManager.Client.ConnectToServer(IpInputField.text.Replace(",", "."), UIManager.NetworkManager.Client.RemoteServerPort))
            {
                UIManager.WaitingMenu.MainObject.SetActive(true);
                UIManager.ConnectMenu.MainObject.SetActive(false);
            }
        }
        catch
        {
           
        }
        
    }
}
