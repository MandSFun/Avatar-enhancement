using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;



public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] Button hostBtn, joinBtn;

    [SerializeField] TextMeshProUGUI ipAddressText;
    [SerializeField] TMP_InputField ip;
    [SerializeField] string ipAddress;
    [SerializeField] UnityTransport transport;
    string hostIP;
    void Start()
    {

    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        GetLocalIPAddress();
        Debug.Log("Host start");

    }

    // To Join a game
    public void StartClient()
    {
        ipAddress = ip.text.Trim();
        Debug.Log(ipAddress);
        SetIpAddress(ipAddress);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client start");
    }
    /* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
    // ONLY FOR HOST SIDE 
    public string GetLocalIPAddress()
    {
        string hostName = Dns.GetHostName();
        hostIP = Dns.GetHostEntry(hostName).AddressList[1].ToString();

        ipAddressText.text = hostIP;
        //ipAddress = hostIP;
        return ipAddress;

    }

    /* Sets the Ip Address of the Connection Data in Unity Transport
	to the Ip Address which was input in the Input Field */
    // ONLY FOR CLIENT SIDE
    public void SetIpAddress(string ipad)
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipad;
    }
}
