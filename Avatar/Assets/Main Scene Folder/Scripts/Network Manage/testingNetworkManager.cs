using System.Text;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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
    [SerializeField] private TMP_Dropdown dropDown;




    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";

   // Start is called before the first frame update
    // private void Start()
    // {
    // NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    //  NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;

    


    // }

    // public void handleDropDownValueChange(int index) {
    //     switch(index){
    //         case(0):{
    //             role = "TRUST_ANCHOR";
    //             break;
    //         }
    //         case(1):{
    //             role = "Endorser";
    //             break;
    //         }
    //         case(2):{
    //             role = "Issuer";
    //             break;
    //         }
    //         case(3):{
    //             role = "Holder";
    //             break;
    //         }
    //         case(4):{
    //             role = "Mediator";
    //             break;
    //         }
    //     }
    // }

    public void Register(){

        //TODO: Add check that name is not already on the blockchain        

        //Get name and password from input fields
        string name = nameInputField.text;
        string password = passwordInputField.text;
        string role = dropDown.captionText.text;

        UnityEngine.Debug.Log("Role: " + role);

        //Format name into wallet seed which is 32 characters
        int numZero = 32 - nameInputField.text.Length - 1;
        string seed = name;
        for (int i = 0; i < numZero; i++) 
        {
            seed = seed + "0";
        }
        seed = seed + "1";

        //register the DID based on the seed value using the von-network webserver
        Dictionary<string, string> registrationData = new Dictionary<string, string>();
        registrationData.Add("seed", seed);
        registrationData.Add("role", role);
        registrationData.Add("alias", nameInputField.text);
        // {
        //     { "seed", seed },
        //     { "role", "TRUST_ANCHOR" },
        //     { "alias", nameInputField.text }
        // };

        string jsonData = JsonConvert.SerializeObject(registrationData);

        // Construct the URL for the registration endpoint
        string url = ledgerUrl + registrationEndpoint;

        // Debug.Log(url);
        // Send the registration data to ACA-Py agent via HTTP request
        StartCoroutine(SendRegistrationRequest(url, jsonData, password, name));
        
    }

    IEnumerator SendRegistrationRequest(string url, string jsonData, string password, string name)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // UnityWebRequest request = UnityWebRequest.Post(url, jsonData);
        // request.SetRequestHeader("Content-Type", "application/json");

        // yield return request.SendWebRequest();
        UnityWebRequestAsyncOperation httpRequest = request.SendWebRequest();
        while(!httpRequest.isDone){
            // Load Scene for choosing host/client
            // SceneManager.LoadScene(Loader.Scene.Loading.ToString());
            UnityEngine.Debug.Log("Progress: " + httpRequest.progress);
            yield return null;
        }
        
        // yield return request.SendWebRequest();
        UnityEngine.Debug.Log(httpRequest.webRequest.result);
        if (httpRequest.webRequest.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("Registration successful!");
            // Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<JsonData>(httpRequest.webRequest.downloadHandler.text);
            
            // string wallet_seed = request.downloadHandler.text["seed"];
            // string verkey = request.downloadHandler.text["verkey"];
            // UnityEngine.Debug.Log("DID: " + response.did);

            // Where to send messages that arrive destined for a given verkey 
            // UnityEngine.Debug.Log("Verkey: " + response.verkey);
            // UnityEngine.Debug.Log("Seed: " + response.seed);
            
            //add arguments
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("DID", response.did);
            arguments.Add("Name", name);
            arguments.Add("Verkey", response.verkey);
            arguments.Add("Seed", response.seed);
            arguments.Add("WalletSecret", password);
            // UnityEngine.Debug.Log("Arguments: " + arguments);
            
            // Load Scene for choosing host/client
            Loader.Load(Loader.Scene.Main);
            StartAcaPyInstance(arguments);

        }
        else
        {
            UnityEngine.Debug.LogError("Registration failed: " + request.error);
        }
    }


    private void RunScriptInDirectory(string directoryPath, string scriptCommand, Dictionary<string, string> arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WorkingDirectory = directoryPath,
            FileName = "bash",
            Arguments = $"-c \"{scriptCommand}\" --label {arguments["Name"]} -it http 0.0.0.0 8001 -ot http --admin 0.0.0.0 11001 --admin-insecure-mode --genesis-url http://host.docker.internal:9000/genesis --endpoint http://localhost:8001/ --seed {arguments["Seed"]} --debug-connections --auto-provision --wallet-type indy --wallet-name {arguments["Name"]} --wallet-key {arguments["WalletSecret"]}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WindowStyle =  ProcessWindowStyle.Minimized,
            // CreateNoWindow = true
        };

        Process process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        // UnityEngine.Debug.Log("Running script now");

        process.Start();
        UnityEngine.Debug.Log("Running script now");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        UnityEngine.Debug.Log(process.StartInfo);
        UnityEngine.Debug.Log("Process start time:" + process.StartTime);
        // process.WaitForExit();
    }

    private void StartAcaPyInstance(Dictionary<string, string> arguments)
    {
        string directoryPath = "/home/aortz99/ACA-PY/aries-cloudagent-python/scripts";
        string scriptCommand = "./run_docker start";
        UnityEngine.Debug.Log("Starting ACA-PY instance now");
        RunScriptInDirectory(directoryPath, scriptCommand, arguments);
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


}
