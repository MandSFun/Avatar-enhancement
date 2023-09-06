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
using System.Globalization;

[System.Serializable]
public class JsonData
{
    public string did;
    public string seed;
    public string verkey;
}

public class AuthController : NetworkBehaviour
{
    [SerializeField] private TMP_InputField IPAddressInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField credentialInputField;
    [SerializeField] private TMP_InputField IDInputField;
    [SerializeField] private TMP_Dropdown dropDown;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    public GameObject parentPopupWindow;
    private GameObject errorWindow;
    private GameObject successfulRegistrationWindow;
    public static AuthController instance;
    public string registeredUsername;
    public string registeredPassword;
    public string IPAddress;
    public bool isNewUser = false;

    private string ledgerUrl ;
    private string registrationEndpoint = "/register";

    private static readonly HttpClient client = new HttpClient();
    
    private void Awake()
    {
        instance = this;
        IPAddress = LoginController.instance.IPAddress;
        IPAddressInputField.text = IPAddress;
        DontDestroyOnLoad(gameObject);
        ledgerUrl = "http://" + IPAddress + ":9000";
    }

    /// <summary>
    /// Starts registration workflow
    /// </summary>
    /// <returns></returns>
    public async void Register(){    
        //Get name and password from input fields
        string name = nameInputField.text;
        string password = passwordInputField.text;
        string role = dropDown.captionText.text;
        string ID = IDInputField.text;
        string credential = credentialInputField.text;
        IPAddress = IPAddressInputField.text;
        if (credentialInputField.text == "D5taCard" || VerifyCredentialwithID(credential, ID, name,ledgerUrl))
        {
            Debug.Log("credential is valid!");
            StartCoroutine(HandleQueryResult(name, password, role, ledgerUrl));
        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Failed to Verify Credential!";
        }
        
        
    }

    public bool VerifyCredentialwithID(string credential, string userID, string username, string ledgerUrl)
    {
        try
        {
            string transactionsUrl = $"{ledgerUrl}/ledger/domain?query=&type=101"; // Specify the transaction type as "101" for schemas
            HttpResponseMessage response = client.GetAsync(transactionsUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var transactions = JToken.Parse(responseBody)["results"];

                foreach (var transaction in transactions)
                {
                    
                    var responseData = transaction["txn"]["data"]["data"];
                    var credName = responseData["name"];
                    JArray items = (JArray)responseData["attr_names"];
                    int length = items.Count;
                    string expirydate ;
                    string hashID;
                    
                    string hashedInputID = userID.GetHashCode().ToString();
                    if (length == 2)
                    {
                         expirydate = Convert.ToString(responseData["attr_names"][0]);
                         hashID = Convert.ToString(responseData["attr_names"][1]);
                        
                        Debug.Log("credential name is " + credName + " expiry date of " + expirydate + " with hashed userid of " + hashID);

                        // check if credential ID is valid and user ID matches
                            if (credName != null && string.Compare(credName.ToString(), credential) == 0 )
                            {
                            Debug.Log("credential names match");
                                if (string.Compare(hashID, hashedInputID) == 0)
                                {
                                Debug.Log("userID hashes match");
                                    DateTime expiryDate = DateTime.ParseExact(expirydate, "ddmmyyyy", CultureInfo.InvariantCulture);
                                    DateTime dateNow = DateTime.Now;

                                    if (expiryDate >= dateNow) //has not expired
                                    {
                                    Debug.Log("Credential has not expired");
                                    return true;
                                    }
                                }
                                
                            
                            }

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

        popupWindow.SetActive(true);
        windowMessage.text = "Credential is not valid!";
        return false;
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
            SQLAddNewUserDetail(name, password);

            isNewUser = true;

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

    
    // add the new users details into the userdetails db in master using SQL SA account
    private void SQLAddNewUserDetail(string username, string password)
    {
        string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        SqlConnection con = new SqlConnection(adminConString);
        try
        {
            LoginController.instance.CreateNewDB();
            con.Open();
            Debug.Log("SQL server connection successful!");

            LoginController.instance.CreateTables();

           
            LoginController.instance.CreateNewUserAccount(AuthController.instance.registeredUsername, AuthController.instance.registeredPassword);
            UpdateUserInfoTable(username, password) ;
            con.Close();

        }catch (Exception ex)
        {
            Debug.Log("Error adding new user info to SQL server");
        }
    }

    public void UpdateUserInfoTable(string username, string password)
    {
        int usernameHash = username.GetHashCode();
        int passwordHash = password.GetHashCode();
        string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        try
        {
            using (SqlConnection connection = new SqlConnection(adminConString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {

                    
                    command.CommandText = "INSERT INTO userdata (username_hash,password_hash) VALUES (" + usernameHash + "," + passwordHash + ");";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) user added with id: " + usernameHash + " with password = " + passwordHash);
                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error adding userdata into DB");
        }   

    }

    


    
}