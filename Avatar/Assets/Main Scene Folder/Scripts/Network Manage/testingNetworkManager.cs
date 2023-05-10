using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;
using Newtonsoft.Json;

[System.Serializable]
public class JsonData
{
    public string did;
    public string seed;
    public string verkey;
}

public class testingNetworkManager : NetworkBehaviour
{ 
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;


    [SerializeField] private Button LeaveButton;

    [SerializeField] private GameObject Holder;

    private static Dictionary<ulong, PlayerData> clientData;//Dictionary to store 
    private bool isServerStarted = false;

    //aries agent url
    public string agentUrl = "localhost:8000";
    public string registrationEndpoint = "/register";


   // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Starting...");
        Debug.Log("Name:" + nameInputField.text);
        // nameInputField.onValueChanged.AddListener(HandleInputValueChanged);
        // passwordInputField.onValueChanged.AddListener(HandleInputValueChanged);
        // NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        // NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
    }

    private void HandleInputValueChanged(string value)
    {
        // Handle the input field value change here
        Debug.Log("Input Field Value Changed: " + value);
    }

    private void Destroy()
    {
        if (NetworkManager.Singleton == null) { return; }
       // NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
       // NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    public void Leave()
    {   NetworkManager.Singleton.Shutdown();
        if (IsClient) NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        } 
        
        Holder.SetActive(true);

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
        NetworkManager.Singleton.StartClient();

    }
    public void Host()
    {   //Instantiate dictornary 'clientData' for Id->PlayerData;
        
        // clientData = new Dictionary<ulong, PlayerData>();
    //     clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);
      
    //    //NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        
        //create dictionary for player data
        Debug.Log("Pressing Button");
        string name = nameInputField.text;
        string password = passwordInputField.text;
        // if( nameInputField.text != null){
        //     Debug.Log("The name is" + name);
        // }
        // else{
        //     Debug.Log("The name is null");
        // }
        // if( nameInputField.text != null){
        //     Debug.Log("The pw is" + password);
        // }
        // else{
        //     Debug.Log("The pw is null");
        // }
        Debug.Log("The name is "+ name);
        Debug.Log("The pw is "+ password);
        Dictionary<string, string> registrationData = new Dictionary<string, string>()
        {
            { "name", nameInputField.text },
            // { "email", emailInput.text },
            { "password", passwordInputField.text }
        };
        
        string jsonData = JsonUtility.ToJson(registrationData);
        // Construct the URL for the registration endpoint
        string url = agentUrl + registrationEndpoint;

        // Send the registration data to ACA-Py agent via HTTP request
        StartCoroutine(SendRegistrationRequest(url, jsonData));

        // NetworkManager.Singleton.StartHost();
        //setPassword(passwordInputField.text);
         
    }

    // '?' allows null return for un-nullable;
    // public static PlayerData? GetPlayerData(ulong clientId)
    // {   ////For name display;
    //     ////Get the client data for the specific id;
    //     if (clientData.TryGetValue(clientId, out PlayerData playerData))
    //     {
    //         return playerData;
    //     }

    //     return null;
    // }

    

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
    {Debug.Log($"ClientId {clientId} connected, LocalClientId is {NetworkManager.Singleton.LocalClientId}");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Holder.SetActive(false);
            LeaveButton.gameObject.SetActive(true);
            

        }
    }
    public void setPassword(string password){
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
