using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] Button hostBtn, joinBtn;


    public void Client()

    {

        NetworkManager.Singleton.StartClient();



    }
    public void Host()
    {

        NetworkManager.Singleton.StartHost();

        //setPassword(passwordInputField.text);

    }
}
