using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupController : MonoBehaviour
{
    public GameObject parentPopupWindow; // Reference to the pop-up window GameObject
    private GameObject childPopupWindow;
    public TMP_Dropdown dropDown;

    private void GetChildPopup() {
        switch(dropDown.captionText.text){
            case "STEWARD":
                childPopupWindow = parentPopupWindow.transform.GetChild(0).gameObject;
                break;
            case "TRUST_ANCHOR":
                childPopupWindow = parentPopupWindow.transform.GetChild(1).gameObject;
                break;
            case "TRUSTEE":
                childPopupWindow = parentPopupWindow.transform.GetChild(2).gameObject;
                break;
            case "USER":
                childPopupWindow = parentPopupWindow.transform.GetChild(3).gameObject;
                break;
            case "ISSUER":
                childPopupWindow = parentPopupWindow.transform.GetChild(4).gameObject;
                break;
            case "VERIFIER":
                childPopupWindow = parentPopupWindow.transform.GetChild(5).gameObject;
                break;
        }
    }
    
    public void ShowPopup()
    {
        childPopupWindow.SetActive(true); // Show the pop-up window
    }

    public void HidePopup()
    {
        childPopupWindow.SetActive(false); // Show the pop-up window
    }

    public void TogglePopup() 
    {
        GetChildPopup();
        if(childPopupWindow.activeSelf){
            HidePopup();
        }
        else{
            ShowPopup();
        }
    }
}
