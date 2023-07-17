using System;
using System.Text;
using System.Threading;
using System.IO; // for Path
using System.Reflection; // for Assembly
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;


public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;

    private string ledgerUrl = "http://localhost:9000";
    private string registrationEndpoint = "/register";


    public void Login(){

        //TODO: Add check that name is not already on the blockchain        

        //Get name and password from input fields
        string name = nameInputField.text;
        string password = passwordInputField.text;

        //format string to have no whitespace and to be all lowercase
        string seed = name;
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
        StartCoroutine(SendLoginRequest(url, jsonData, password, name));
        
    }

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
            // Load Scene for choosing host/client
            // SceneManager.LoadScene(Loader.Scene.Loading.ToString());
            UnityEngine.Debug.Log("Progress: " + httpRequest.progress);
            yield return null;
        }
        
        // yield return request.SendWebRequest();
        UnityEngine.Debug.Log(httpRequest.webRequest.result);
        if (httpRequest.webRequest.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("Login successful!");
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
            arguments.Add("WALLET_NAME", name);
            arguments.Add("LABEL", name);
            arguments.Add("VERKEY", response.verkey);
            arguments.Add("AGENT_WALLET_SEED", response.seed);
            arguments.Add("WALLET_KEY", password);
            UnityEngine.Debug.Log("DID: " + arguments["DID"]);
            UnityEngine.Debug.Log("Verkey: " + arguments["VERKEY"]);
            
            // Load Scene for choosing host/client
            Loader.Load(Loader.Scene.Main);
            StartAcaPyInstanceAsync(arguments);
            request.Dispose();

        }
        else
        {
            UnityEngine.Debug.LogError("Registration failed: " + request.error);
        }
    }


    public async Task RunDockerComposeAsync(string composeFilePath, Dictionary<string, string> arguments)
    {
        Process process = new Process();

        try
        {
            string composeFile = Path.GetDirectoryName(composeFilePath);
            string currentScriptPath = Assembly.GetExecutingAssembly().Location; // Get the current script file path
            string currentScriptDirectory = Path.GetDirectoryName(currentScriptPath); // Get the directory path of the current script
            string composeFileFullPath = Path.Combine(currentScriptDirectory, composeFile); // Combine the current script directory with the relative compose file path
            

            UnityEngine.Debug.Log("Directory of composeFileFullPath: " + composeFileFullPath);
            
            UnityEngine.Debug.Log("Overriding env file");
            //.env file path
            string envFullPath = Path.Combine(composeFileFullPath, ".env");
            UnityEngine.Debug.Log("Env path: " + envFullPath);

            SaveEnvFile(envFullPath, arguments);
            UnityEngine.Debug.Log("Override complete");

            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = composeFileFullPath; // Set the working directory to the current script directory
            process.StartInfo.Arguments = $"/k docker-compose -f docker-compose.login.yaml up"; // Specify the compose file and command
    
            UnityEngine.Debug.Log("Directory of process.StartInfo.Arguments: " + process.StartInfo.Arguments);


            process.StartInfo.UseShellExecute = true;

            process.EnableRaisingEvents = true;
            process.Start();

            bool processStarted = await Task.Run(() => process.WaitForExit(Timeout.Infinite));
            UnityEngine.Debug.Log("Process started?: " + processStarted);

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

    // Function to save the dictionary as a .env file
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

    
    // Function to load the contents of the .env file into a dictionary
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

    private async void StartAcaPyInstanceAsync(Dictionary<string, string> arguments)
    {
        // string directoryPath = "/home/aortz99/ACA-PY/aries-cloudagent-python/scripts";
        // string scriptCommand = "./run_docker start";
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

    public void RedirectToRegistration(){
        Loader.Load(Loader.Scene.Registration);
    }
}
