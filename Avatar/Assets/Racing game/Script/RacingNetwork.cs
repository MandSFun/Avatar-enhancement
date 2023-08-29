using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
public class RacingNetwork : NetworkBehaviour
{
    // Start is called before the first frame update
    public GameObject holder;
    [SerializeField] TextMeshProUGUI ipAddressText;
    [SerializeField] TMP_InputField ip;
    [SerializeField] string ipAddress;
    [SerializeField] UnityTransport transport;
    string hostIP;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        GetLocalIPAddress();
        // Debug.Log("Host start");
        holder.SetActive(false);
    }//

    // To Join a game
    public void StartClient()
    {
        ipAddress = ip.text.Trim();
        Debug.Log(ipAddress);
        SetIpAddress(ipAddress);
        NetworkManager.Singleton.StartClient();
        // Debug.Log("Client start");
        holder.SetActive(false);
    }
    /* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
    // ONLY FOR HOST SIDE 
    public string GetLocalIPAddress()
    {
        string hostName = Dns.GetHostName();
        hostIP = Dns.GetHostEntry(hostName).AddressList[1].ToString();
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
   hostIP,  // The IP address is a string
   (ushort)12345, // The port number is an unsigned short
   "0.0.0.0" // The server listen address is a string.
); Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ToString());
        ipAddressText.text = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ToString();
        Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ToString());
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
    public void checkclick()
    {
        Debug.Log("Clicked");
        SceneManager.LoadScene("Shooting Game");
    }
}
