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


[System.Serializable]
public class JsonData
{
    public string did;
    public string seed;
    public string verkey;
}

public class AuthController : NetworkBehaviour
{ 
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Dropdown dropDown;
    public GameObject parentPopupWindow;
    private GameObject errorWindow;
    private GameObject successfulRegistrationWindow;
    public static AuthController instance;
    public string registeredUsername;
    public string registeredPassword;


    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";

    private static readonly HttpClient client = new HttpClient();


    /// <summary>
    /// Starts registration workflow
    /// </summary>
    /// <returns></returns>
    public async void Register(){    
        //Get name and password from input fields
        string name = nameInputField.text;
        string password = passwordInputField.text;
        string role = dropDown.captionText.text;

        //Check that name is not already on the blockchain
        StartCoroutine(HandleQueryResult(name, password, role, ledgerUrl));
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
                displayErrorText("Error retrieving transactions!");               
            }
        }
        catch (Exception ex)
        {
            displayErrorText(ex.Message);
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
    private IEnumerator HandleQueryResult(string username, string password, string role, string ledgerUrl)
    {
        yield return StartCoroutine(CheckIfDuplicateUserExists(username, ledgerUrl, (aliasExists) =>
        {
            if (aliasExists)
            {
                displayErrorText("Username already exists!! Please try a different username");
            }
            else
            {  
                //format string to have no whitespace and to be all lowercase
                string seed = username;
                string seedFormatted = seed.Replace(" ", "");
                seed = seedFormatted.ToLower();

                //Format name into wallet seed which is 32 characters
                int numZero = 32 - seed.Length - 1;
                
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

                string jsonData = JsonConvert.SerializeObject(registrationData);

                // Construct the URL for the registration endpoint
                string url = ledgerUrl + registrationEndpoint;

                // Debug.Log(url);
                // Send the registration data to ACA-Py agent via HTTP request
                StartCoroutine(SendRegistrationRequest(url, jsonData, password, username));
            }
        }));
    }

    /// <summary>
    /// Starts coroutine to send registration request to blockchain and start the docker-compose containers if players are registered
    /// </summary>
    /// <param name="url"></param>
    /// <param name="jsonData"></param>
    /// <param name="password"></param>
    /// <param name="name"></param>
    /// <returns>Redirects players to main game page</returns>
    private IEnumerator SendRegistrationRequest(string url, string jsonData, string password, string name)
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
            successfulRegistrationWindow = parentPopupWindow.transform.GetChild(7).gameObject;                
            TMP_Text successText = successfulRegistrationWindow.transform.GetChild(1).GetComponent<TMP_Text>();
            successText.text = "Registration successful!";
            
            successfulRegistrationWindow.SetActive(true);
            // Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<JsonData>(httpRequest.webRequest.downloadHandler.text);


            //create new SQL server login for the new user
            registeredUsername = name;
            registeredPassword = password;
            CreateNewUserAccount(registeredUsername, registeredUsername);

            // Load Scene for choosing host/client

            Loader.Load(Loader.Scene.Login);
            request.Dispose();

        }
        else
        {
            displayErrorText(request.error);
        }
    }

    /// <summary>
    /// Helper function for displaying error messages
    /// </summary>
    /// <param name="error"></param>
    private void displayErrorText(string error){
        errorWindow = parentPopupWindow.transform.GetChild(6).gameObject;                
        TMP_Text errorText = errorWindow.transform.GetChild(1).GetComponent<TMP_Text>();
        errorText.text = error;
        errorWindow.SetActive(true);
    }

    public void CreateNewUserAccount(string username, string password)
    {
        string DBname = "AvatarProject";
        string connstring = "Data Source=10.255.253.29;Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connstring))
            {

                connection.Open();
                

                using (var command = connection.CreateCommand())
                {
                    /*
                     * CREATE LOGIN user1234 WITH PASSWORD = 'password';
                        USE AvatarProject; CREATE USER user1234 FOR LOGIN user1234;
                        USE AvatarProject; GRANT SELECT, INSERT, UPDATE, DELETE TO user1234;

                     * 
                     */

                    command.CommandText = "CREATE LOGIN " + username + " WITH PASSWORD = '" + password + "', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF; " +
                        "USE " + DBname + "; CREATE USER " + username + " FOR LOGIN " + username + "; " +
                        "USE " + DBname + "; GRANT SELECT, INSERT, UPDATE, DELETE TO " + username + "; ";

                    command.ExecuteNonQuery();
                }

                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL server) Error creating new account");
            
        }

    }
}