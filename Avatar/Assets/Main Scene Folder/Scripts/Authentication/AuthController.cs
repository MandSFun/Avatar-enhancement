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
    [SerializeField] private TMP_Dropdown dropDown;
    public GameObject parentPopupWindow;
    private GameObject errorWindow;
    private GameObject successfulRegistrationWindow;
    public static AuthController instance;
    public string registeredUsername;
    public string registeredPassword;
    public string IPAddress;
    public bool isNewUser = false;

    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";

    private static readonly HttpClient client = new HttpClient();
    
    private void Awake()
    {
        instance = this;
        string hostName = Dns.GetHostName();
        IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
        IPAddressInputField.text = IPAddress;
        DontDestroyOnLoad(gameObject);
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
        IPAddress = IPAddressInputField.text;

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
            CreateNewDB();
            con.Open();
            Debug.Log("SQL server connection successful!");
            
            CreateTables(adminConString);

           
            CreateNewUserAccount(AuthController.instance.registeredUsername, AuthController.instance.registeredPassword);
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

    public void CreateNewDB()
    {
        string connstring = "Data Source=" + IPAddress + ";Initial Catalog=master;User ID=sa;Password=D5taCard;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connstring))
            {

                connection.Open();


                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'AvatarProject')     " +
                        "BEGIN  CREATE DATABASE AvatarProject  END";

                    command.ExecuteNonQuery();
                }

                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL server) Error creating AvatarProject DB");

        }
    }

    public void CreateTables(string connstring)
    {
        try
        {
            //create the db connection
            using (SqlConnection connection = new SqlConnection(connstring))
            {

                connection.Open();
                //set up objeect called command to allow db control
                using (var command = connection.CreateCommand())
                {

                    //sql statements to execute
                    command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'userdata')BEGIN  CREATE TABLE userdata ( username_hash INT, password_hash INT )END;";
                    command.ExecuteNonQuery();
                    command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'weapons')BEGIN  CREATE TABLE weapons ( playerid INT, weaponid INT, quantity INT) END;";
                    command.ExecuteNonQuery();
                    command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'playerlocation')BEGIN  CREATE TABLE playerlocation ( playerid INT, x INT, y INT, z INT) END;";
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error creating new database. Get admin to create database!");
        }

    }


    public void CreateNewUserAccount(string username, string password)
    {

        string DBname = "AvatarProject";
        string connstring = "Data Source=" + IPAddress + " ;Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        //string connstring = "Data Source=192.168.56.1;Initial Catalog=AvatarProject;User ID=user;Password=user;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connstring))
            {

                connection.Open();


                using (var command = connection.CreateCommand())
                {
                    /*
                     * IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = ' username ' AND type = 'S') BEGIN
                         CREATE LOGIN   username  WITH PASSWORD ='password' , CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF; 
                        USE  AvatarProject; CREATE USER  username  FOR LOGIN  username ; 
                        USE  AvatarProject ; GRANT SELECT, INSERT, UPDATE, DELETE TO  username  END;

                     * 
                     */

                    command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = ' " + username + " ' AND type = 'S') " +
                        "BEGIN CREATE LOGIN   " + username + " WITH PASSWORD = '" + password + "' ," +
                        " CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF   USE AvatarProject; CREATE USER " + username + " FOR LOGIN " + username + " ;" +
                        " USE AvatarProject; GRANT SELECT, INSERT, UPDATE, DELETE TO " + username + "  END; ";

                    command.ExecuteNonQuery();
                }

                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL server) Error creating new account:  " + e);
        }

    }
}