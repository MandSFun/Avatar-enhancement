using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChildPopupController : MonoBehaviour
{
    public GameObject PopupWindow;
    
    public void ShowPopup()
    {
        PopupWindow.SetActive(true); // Show the pop-up window
    }

    public void HidePopup()
    {
        PopupWindow.SetActive(false); // Show the pop-up window
    }

}
