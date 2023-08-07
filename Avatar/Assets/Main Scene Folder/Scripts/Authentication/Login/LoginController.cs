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

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField IPAddressInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;

    public GameObject parentPopupWindow;
    private GameObject errorWindow;
    private GameObject successfulLoginWindow;
    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";
    private static readonly HttpClient client = new HttpClient();
    public static LoginController Instance;
    public string verifiedUsername;
    public string verifiedPassword;
    public string IPAddress;
  



    public void Awake()
    {
        Instance = this;

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
     
        //Get name and password from input fields
        string name = nameInputField.text;
        string password = passwordInputField.text;
        IPAddress = IPAddressInputField.text;

        //Add check that name is not already on the blockchain
        StartCoroutine(HandleLoginQueryResult(name, password, ledgerUrl)); 
    }

    /// <summary>
    /// Handler to check if user exists on distributed ledger
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="ledgerUrl"></param>
    /// <returns>Redirects user to main game page</returns>
    private IEnumerator HandleLoginQueryResult(string username, string password, string ledgerUrl)
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
                StartCoroutine(SendLoginRequest(url, jsonData, password, username));
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
    IEnumerator SendLoginRequest(string url, string jsonData, string password, string name)
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

            //configure the SQL server account as the user
            verifiedUsername = name;
            verifiedPassword = password;

            // Load Scene for choosing host/client
            Loader.Load(Loader.Scene.Main);
            StartAcaPyInstanceAsync(arguments);
            request.Dispose();

        }
        else
        {
            UnityEngine.Debug.LogError("Login failed: " + request.error);
        }
    }

    /// <summary>
    /// Start Docker-Compose file and create a seperate terminal to log output from the containers
    /// </summary>
    /// <param name="composeFilePath"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
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
            process.Dispose();
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
    private async void StartAcaPyInstanceAsync(Dictionary<string, string> arguments)
    {
        string composeFilePath = "../../Assets/Main Scene Folder/Scripts/Wallet/";
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
}
