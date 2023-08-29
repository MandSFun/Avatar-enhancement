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
using System.Net;
using System.Data.SqlClient;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPAddressInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField; 
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;

    public GameObject parentPopupWindow;
    private GameObject errorWindow;
    private GameObject successfulLoginWindow;
    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";
    private static readonly HttpClient client = new HttpClient();
    public static LoginController instance;
    public string verifiedUsername;
    public string verifiedPassword;
    public string IPAddress;
    
  



    public void Awake()
    {


        instance = this;

        try
        {
            IPAddress = AuthController.instance.IPAddress;
            IPAddressInputField.text = AuthController.instance.IPAddress;
            nameInputField.text = AuthController.instance.registeredUsername;  
        }
        catch (Exception ex)
        {
            string hostName = Dns.GetHostName();
            IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            IPAddressInputField.text = IPAddress;
        }

        DontDestroyOnLoad(gameObject);


    }

    /// <summary>
    /// Starts coroutine to check if player is registered on the blockchain
    /// </summary>
    /// <returns></returns>
    public async void Login(){

        if (DockerStatusIcon.instance.SQLServerConnection == false)
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Not connected to SQL Server!";
        }
        else
        {
            //Get name and password from input fields
            string nameInput = nameInputField.text;
            string passwordInput = passwordInputField.text;
            IPAddress = IPAddressInputField.text;


            //Add check that name is not already on the blockchain
            StartCoroutine(HandleLoginQueryResult(nameInput, passwordInput, ledgerUrl, true));
        }
        
    }

    public void OnDestroy()
    {
        UnityEngine.Debug.Log( "(ATTENTION) logincontroller destroyed!");
    }

    public async void AccessAdminPanel()
    {

        //Get name and password from input fields
        string nameInput = nameInputField.text;
        string passwordInput = passwordInputField.text;
        IPAddress = IPAddressInputField.text;

        //Add check that name is not already on the blockchain
        StartCoroutine(HandleLoginQueryResult(nameInput, passwordInput, ledgerUrl, false));
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

    public void CreateTables()
    {
        string connstring = "Data Source=" + IPAddress + ";Initial Catalog= AvatarProject;User ID=sa;Password=D5taCard;";
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
                    command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IssuedCredentials')BEGIN  CREATE TABLE IssuedCredentials ( CredentialID INT, Issuer varchar(20), UserID varchar(20), Expiry INT) END;";
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error creating new database. Get admin to create database!");
        }

    }

    /// <summary>
    /// Handler to check if user exists on distributed ledger
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="ledgerUrl"></param>
    /// <returns>Redirects user to main game page</returns>
    private IEnumerator HandleLoginQueryResult(string username, string password, string ledgerUrl, bool loadMainScene)
    {
        yield return StartCoroutine(CheckIfUserExists(username, ledgerUrl, (userExists) =>
        {
            if (userExists)
            {
                
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

                UnityEngine.Debug.Log("Seed: " + seed);

                //register the DID based on the seed value using the von-network webserver
                Dictionary<string, string> registrationData = new Dictionary<string, string>();
                registrationData.Add("seed", seed);
                registrationData.Add("alias", username);

                string jsonData = JsonConvert.SerializeObject(registrationData);

                // Construct the URL for the registration endpoint
                string url = ledgerUrl + registrationEndpoint;

                // Debug.Log(url);
                // Send the registration data to ACA-Py agent via HTTP request
                StartCoroutine(SendLoginRequest(url, jsonData, password, username, loadMainScene));
            }
            else
            {  
                displayErrorText("Please register an account first!");
            }
        }));
    }

    /// <summary>
    /// Check if player exists on the blockchain/distributed ledger
    /// </summary>
    /// <param name="username"></param>
    /// <param name="ledgerUrl"></param>
    /// <param name="callback"></param>
    /// <returns>True if player is registered on distributed ledger</returns>
    public IEnumerator CheckIfUserExists(string username, string ledgerUrl, Action<bool> callback){
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
                displayErrorText("Error retrieving transactions.");
            }
        }
        catch (Exception ex)
        {
            displayErrorText($"Error: {ex.Message}");
        }
        callback(false);
    }

    /// <summary>
    /// Starts coroutine to send Login request to blockchain and start the docker-compose containers if players are registered
    /// </summary>
    /// <param name="url"></param>
    /// <param name="jsonData"></param>
    /// <param name="password"></param>
    /// <param name="name"></param>
    /// <returns>Redirects players to main game page</returns>
    IEnumerator SendLoginRequest(string url, string jsonData, string password, string name, bool loadMainScene)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        

        // yield return request.SendWebRequest();
        UnityWebRequestAsyncOperation httpRequest = request.SendWebRequest();
        while(!httpRequest.isDone){
            yield return null;
        }
        
        // yield return request.SendWebRequest();
        if (httpRequest.webRequest.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("Registration successful!");
            // Debug.Log(request.downloadHandler.text);
            var response = JsonUtility.FromJson<JsonData>(httpRequest.webRequest.downloadHandler.text);
            
            //add arguments
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("DID", response.did);
            arguments.Add("WALLET_NAME", name);
            arguments.Add("LABEL", name);
            arguments.Add("VERKEY", response.verkey);
            arguments.Add("AGENT_WALLET_SEED", response.seed);
            arguments.Add("WALLET_KEY", password);
            UnityEngine.Debug.Log("DID: " + arguments["DID"]);
            UnityEngine.Debug.Log("Verkey: " + arguments["VERKEY"]);

            

            bool isAuthenticated = AuthenticateWithSQLServer(name,password);

            // Load Scene for choosing host/client
            if (isAuthenticated)
            {

                //configure the SQL server account as the user
                verifiedUsername = name;
                verifiedPassword = password;
                userdatapersist.Instance.verifiedPassword = verifiedPassword;
                userdatapersist.Instance.verifiedUser = verifiedUsername;
                userdatapersist.Instance.IPAdd = IPAddress;



                if (loadMainScene)
                {
                    //configure the SQL server account as the user
                    
                    Loader.Load(Loader.Scene.Main);
                }
                else
                {
                    SceneManager.LoadSceneAsync("Admin Panel");
                }
                
                StartAcaPyInstanceAsync(arguments);
                request.Dispose();
            }
            else
            {
                popupWindow.SetActive(true);
                windowMessage.text = "Error logging in!";
            }
            

        }
        else
        {
            UnityEngine.Debug.LogError("Login failed: " + request.error);
        }
    }




    public bool AuthenticateWithSQLServer( string username, string password)
    {
        string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        SqlConnection con = new SqlConnection(adminConString);
        bool dataFound = false;

        try
        {
             using (SqlConnection connection = new SqlConnection(adminConString))
            {

                connection.Open();
             
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM userdata WHERE username_hash = " + username.GetHashCode() + ";";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["username_hash"] == null)
                            {
                                UnityEngine.Debug.Log("(SQL server) no such user exists, no data found");
                                dataFound = false;
                                UnityEngine.Debug.Log("User does not exist!");
                            }
                            else if (reader["password_hash"].ToString() == password.GetHashCode().ToString())
                            {
                               return true;
                            }
                            else
                            {
                                dataFound = false;
                                popupWindow.SetActive(true);
                                windowMessage.text = "Wrong Password!";
                            }
                        }
                        reader.Close();
                    }

                    
                    
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error validating password!");
            popupWindow.SetActive(true);
            windowMessage.text = "Error Validating password!";

        }
        return false;
    }

    /// <summary>
    /// Start Docker-Compose file and create a seperate terminal to log output from the containers
    /// </summary>
    /// <param name="composeFilePath"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    /// 


    public async Task RunDockerComposeAsync(string composeFilePath, Dictionary<string, string> arguments)
    {
        Process process = new Process();

        try
        {
            string composeFile = Path.GetDirectoryName(composeFilePath);
            string currentScriptPath = Assembly.GetExecutingAssembly().Location; // Get the current script file path
            string currentScriptDirectory = Path.GetDirectoryName(currentScriptPath); // Get the directory path of the current script
            string composeFileFullPath = Path.Combine(currentScriptDirectory, composeFile); // Combine the current script directory with the relative compose file path
            
            UnityEngine.Debug.Log("Overriding env file");
            //.env file path
            string envFullPath = Path.Combine(composeFileFullPath, ".env");
            UnityEngine.Debug.Log("Env path: " + envFullPath);

            SaveEnvFile(envFullPath, arguments);
            UnityEngine.Debug.Log("Override complete");

            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = composeFileFullPath; // Set the working directory to the current script directory
            process.StartInfo.Arguments = $"/k docker-compose up"; // Specify the compose file and command
            UnityEngine.Debug.Log("Directory of process.StartInfo.Arguments: " + process.StartInfo.Arguments);


            process.StartInfo.UseShellExecute = true;

            process.EnableRaisingEvents = true;
            process.Start();

            bool processStarted = await Task.Run(() => process.WaitForExit(Timeout.Infinite));
            UnityEngine.Debug.Log("Process started: " + processStarted);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running docker-compose: {ex.Message}");
        }
        finally
        {
            process.Close();
           // process.Dispose();
        }
    }

    /// <summary>
    /// Function to override existing .env file with parameters to start docker-compose containers
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="envVariables"></param>
    void SaveEnvFile(string filePath, Dictionary<string, string> envVariables)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (KeyValuePair<string, string> kvp in envVariables)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }
    }

    
    /// <summary>
    /// Function to load the contents of the .env file into a dictionary
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Content of the .env file in the directory of the docker-compose.yaml file</returns>
    Dictionary<string, string> LoadEnvFile(string filePath)
    {
        Dictionary<string, string> envVariables = new Dictionary<string, string>();

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                {
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex);
                        string value = trimmedLine.Substring(equalsIndex + 1);
                        envVariables[key] = value;
                    }
                }
            }
        }

        return envVariables;
    }

    /// <summary>
    /// Format other arguments needed in the .env file and start the docker-compose instance
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public async void StartAcaPyInstanceAsync(Dictionary<string, string> arguments)
    {
        string composeFilePath = "../../Assets/Main Scene Folder/Scripts/Wallet/";
        //string composeFilePath = "../../Avatar/Assets/Main Scene Folder/Scripts/Wallet/";
        arguments.Add("ACAPY_ENDPOINT_PORT", "8001");
        arguments.Add("ACAPY_ADMIN_PORT", "11001");
        arguments.Add("CONTROLLER_PORT", "3001");
        arguments.Add("ACAPY_ENDPOINT_URL", "http://localhost:8002/");
        arguments.Add("LEDGER_URL", "http://host.docker.internal:9000");
        arguments.Add("TAILS_SERVER_URL", "http://tails-server:6543");
        // string[] additionalArgs = { $"--WALLET_KEY={arguments["WALLET_KEY"]}", $"--LABEL={arguments["WALLET_NAME"]}", $"--WALLET_NAME={arguments["WALLET_NAME"]}", $"--AGENT_WALLET_SEED={arguments["SEED"]}", $"--ACAPY_ENDPOINT_PORT={arguments["ACAPY_ENDPOINT_PORT"]}", $"--ACAPY_ADMIN_PORT={arguments["ACAPY_ADMIN_PORT"]}", $"--CONTROLLER_PORT={arguments["CONTROLLER_PORT"]}" };

        UnityEngine.Debug.Log("Starting ACA-PY instance now");
        await RunDockerComposeAsync(composeFilePath, arguments);
        UnityEngine.Debug.Log("Docker Compose completed.");
        // RunScriptInDirectory(directoryPath, scriptCommand, arguments);
    }

    /// <summary>
    /// Helper function for displaying error messages
    /// </summary>
    /// <param name="error"></param>
    private void displayErrorText(string error){
        errorWindow = parentPopupWindow.transform.GetChild(0).gameObject;                
        TMP_Text errorText = errorWindow.transform.GetChild(1).GetComponent<TMP_Text>();
        errorText.text = error;
        errorWindow.SetActive(true);
    }

    /// <summary>
    /// Function that redirects users to registration pages
    /// </summary>
    public void RedirectToRegistration(){
        Loader.Load(Loader.Scene.Registration);
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
