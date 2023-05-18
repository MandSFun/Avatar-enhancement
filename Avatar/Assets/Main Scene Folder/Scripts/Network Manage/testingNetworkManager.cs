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
    // private void Start()
    // {
    // NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    //  NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;

    


    // }
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

    public void Register(){

        //TODO: Add check that name is not already on the blockchain        

        //Format Name into wallet seed which is 32 characters
        int numZero = 32 - nameInputField.text.Length - 1;
        string seed = nameInputField.text;
        for (int i = 0; i < numZero; i++) 
        {
            seed = seed + "0";
        }
        seed = seed + "1";

        //register the DID based on the seed value using the von-network webserver
        Dictionary<string, string> registrationData = new Dictionary<string, string>();
        registrationData.Add("seed", seed);
        registrationData.Add("role", "TRUST_ANCHOR");
        registrationData.Add("alias", nameInputField.text);
        // {
        //     { "seed", seed },
        //     { "role", "TRUST_ANCHOR" },
        //     { "alias", nameInputField.text }
        // };

        string jsonData = JsonConvert.SerializeObject(registrationData);

        // Construct the URL for the registration endpoint
        string url = agentUrl + registrationEndpoint;

        // Send the registration data to ACA-Py agent via HTTP request
        StartCoroutine(SendRegistrationRequest(url, jsonData));
        Loader.Load(Loader.Scene.Main);
    }

    IEnumerator SendRegistrationRequest(string url, string jsonData)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // UnityWebRequest request = UnityWebRequest.Post(url, jsonData);
        // request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        Debug.Log(request.result);
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Registration successful!");
            // Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);
            
            // string wallet_seed = request.downloadHandler.text["seed"];
            // string verkey = request.downloadHandler.text["verkey"];
            Debug.Log("DID: " + response.did);
            
            // Where to send messages that arrive destined for a given verkey 
            Debug.Log("Verkey: " + response.verkey);
            
            Debug.Log("Seed: " + response.seed);
        }
        else
        {
            Debug.LogError("Registration failed: " + request.error);
        }
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
