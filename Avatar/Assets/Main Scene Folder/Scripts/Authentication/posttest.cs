using System;
using System.Text;
using System.Threading;
using System.IO; // for Path
using System.Reflection; // for Assembly
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net;
using Debug = UnityEngine.Debug;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;



public class posttest : NetworkBehaviour
{
    [SerializeField] private TMP_InputField F1;
    [SerializeField] private TMP_InputField F2;
    [SerializeField] private TMP_InputField F3;
    
    
   
    public static posttest instance;
    

    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";

    private static readonly HttpClient client = new HttpClient();
    
    

    /// <summary>
    /// Starts registration workflow
    /// </summary>
    /// <returns></returns>
    public async void post(){    
        //Get name and password from input fields
        string f1 = F1.text;
        string f2 = F2.text;
        string f3 = F3.text;
        

        //Check that name is not already on the blockchain
        HandleQueryResult(f1, f2, f3, ledgerUrl);
    }

    /// <summary>
    /// Coroutine to check if player is registered on the blockchain
    /// </summary>
    /// <param name="username"></param>
    /// <param name="ledgerUrl"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator CheckIfDuplicateUserExists(string username, string ledgerUrl, Action<bool> callback){
        try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=1"; // Specify the transaction type as "1" for NYM transactions
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;
            
            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];
                
                foreach (var transaction in transactions)
                {
                    var responseData = transaction["txn"]["data"];
                    var alias = responseData["alias"];
                    if(alias !=  null){
                        if (string.Compare(alias.ToString(), username) == 0)
                        {
                            callback(true);
                            yield break;
                        }
                    }
                    else {
                        UnityEngine.Debug.Log("Alias is null");
                        
                    }
                }
            }
            else
            {
                Debug.Log("Error retrieving transactions!");               
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        callback(false);
    }

    /// <summary>
    /// Handler to check if user exists on distributed ledger
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <param name="ledgerUrl"></param>
    /// <returns></returns>
    private void HandleQueryResult(string f1, string f2, string f3, string ledgerUrl)
    {
        
       
              
         

                //register the DID based on the seed value using the von-network webserver
                Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("field 1", f1);
            postData.Add("field 2", f2);
            postData.Add("field 3", f3);

                string jsonData = JsonConvert.SerializeObject(postData);

                // Construct the URL for the registration endpoint
                string url = ledgerUrl + registrationEndpoint;

                // Debug.Log(url);
                // Send the registration data to ACA-Py agent via HTTP request
                StartCoroutine(SendPostRequest(url, jsonData));
            
       
    }

    /// <summary>
    /// Starts coroutine to send registration request to blockchain and start the docker-compose containers if players are registered
    /// </summary>
    /// <param name="url"></param>
    /// <param name="jsonData"></param>
    /// <param name="password"></param>
    /// <param name="name"></param>
    /// <returns>Redirects players to main game page</returns>
    private IEnumerator SendPostRequest(string url, string jsonData)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        

        // yield return request.SendWebRequest();
        UnityWebRequestAsyncOperation httpRequest = request.SendWebRequest();
        while(!httpRequest.isDone){
            // Load Scene for choosing host/client
            yield return null;
        }
        
        if (httpRequest.webRequest.result == UnityWebRequest.Result.Success)
        {
            
            // Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<JsonData>(httpRequest.webRequest.downloadHandler.text);
            Debug.Log("Successfully posted it on BC");

            

            // Load Scene for choosing host/client

            Loader.Load(Loader.Scene.Login);
            request.Dispose();

        }
        else
        {
            Debug.Log("failed to posted it on BC :  " + request.error);
        }
    }

    /// <summary>
    /// Helper function for displaying error messages
    /// </summary>
    /// <param name="error"></param>
    
}