using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject MainObject;
    public Text PlayerCountText;
    public Text StatusText;

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
