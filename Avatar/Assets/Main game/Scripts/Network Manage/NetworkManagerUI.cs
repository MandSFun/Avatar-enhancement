using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;
using System.Net;
using Unity.Netcode.Transports.UTP;
using System.Net.Sockets;
using System;

public class NetworkManagerUI : NetworkBehaviour
{


    public static NetworkManagerUI instance;

    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;


    [SerializeField] private Button LeaveButton;

    [SerializeField] private GameObject Holder;
    public static NetworkManagerUI Singleton;
    private static Dictionary<ulong, PlayerData> clientData;//Dictionary to store 
    private bool isServerStarted = false;

    public PlayerNameOverhead GetPlayerObj(ulong id) => NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerNameOverhead>();
    public NetworkManager network;
    public int localPlayerID;
    public ulong LocalId => network.LocalClient.ClientId;
    string IPAddress;
    [SerializeField] UnityTransport transport;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
        try
        {
            // string hostName = Dns.GetHostName();
            // IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            // IPAddressInputField.text = AuthController.instance.IPAddress;
            nameInputField.text =userdatapersist.Instance.verifiedUser;
            Debug.Log(userdatapersist.Instance.verifiedUser + "Name");
        }
        catch (Exception ex)
        {
            string hostName = Dns.GetHostName();
            IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            //  IPAddressInputField.text = IPAddress;
        }
    }

    private void Start()
    { //network = NetworkManager.Singleton;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
    }
    private void Destroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        // NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
        if (!IsServer) NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);

        SceneManager.LoadScene("Main");

    }
    public void Client()

    {   //Convert to byte array;
        // var payload = JsonUtility.ToJson(new ConnectionPayload()
        // {

        //     NetworkPlayerName = nameInputField.text,
        //     password = passwordInputField.text
        // }); 

        // byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
        // NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        IPAddress = passwordInputField.text;
        SetIpAddress(IPAddress);

        NetworkManager.Singleton.StartClient();



    }
    public void Host()
    {   //Instantiate dictornary 'clientData' for Id->PlayerData;

        //     clientData = new Dictionary<ulong, PlayerData>();
        //     clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);

        //    //NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        localPlayerID = nameInputField.text.GetHashCode();
        NetworkManager.Singleton.StartHost();
        GetlocalIP();

        //setPassword(passwordInputField.text);

    }
    private void GetlocalIP()
    {
        string hostName = Dns.GetHostName();
        IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
        Debug.Log(IPAddress + " " + hostName);

    }
    public void SetIpAddress(string ipad)
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.ConnectionData.Address = ipad;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // if (NetworkManager.Singleton.IsServer)
        // {
        //     clientData.Remove(clientId);
        // }

        // Are we the client that is disconnecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Holder.SetActive(true);
            LeaveButton.gameObject.SetActive(false);


        }
    }
    private void HandleClientConnect(ulong clientId)
    {//Debug.Log($"ClientId {clientId} connected, LocalClientId is {NetworkManager.Singleton.LocalClientId}");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Holder.SetActive(false);
            LeaveButton.gameObject.SetActive(true);
            //UpdatePlayernameServerRPC(nameInputField.text);


        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void MyGlobalServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            // Do things for this client
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayernameServerRPC(string clientName, ServerRpcParams serverRpcParams)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            PlayerNameOverhead playerObj = NetworkManagerUI.Singleton.GetPlayerObj(clientId);
            // playerObj.UpdateplayernameServerRPC(clientName);
        }
    }

    public void setPassword(string password)
    {
        byte[] hashed = Encoding.ASCII.GetBytes(password);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = hashed;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Get the password sent by the client
        byte[] clienthash = request.Payload;
        string payload = Encoding.ASCII.GetString(request.Payload);

        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        byte[] hostHash = NetworkManager.Singleton.NetworkConfig.ConnectionData;

        // Check if the hashes match

        bool approved = clienthash.Equals(hostHash);


        if (approved)
        {
            // Store their player data in the clientData dictionary
            ulong clientId = request.ClientNetworkId;

            clientData[clientId] = new PlayerData(connectionPayload.NetworkPlayerName);
            response.Approved = true;
            response.CreatePlayerObject = true;
            // Position to spawn the player object   (if null it uses default of Vector3.zero)

        }
        else
        {
            // If the client is not approved, reject the connection and provide a reason
            response.Approved = false;
            response.Reason = "Incorrect password.";
        }
        response.PlayerPrefabHash = null;
    }


}
