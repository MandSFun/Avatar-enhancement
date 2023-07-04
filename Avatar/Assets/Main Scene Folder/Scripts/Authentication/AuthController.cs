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


    // private void RunScriptInDirectory(string directoryPath, string scriptCommand, Dictionary<string, string> arguments)
    // {
    //     ProcessStartInfo startInfo = new ProcessStartInfo
    //     {
    //         WorkingDirectory = directoryPath,
    //         FileName = "bash",
    //         Arguments = $"-c \"{scriptCommand}\" --label {arguments["Name"]} -it http 0.0.0.0 8001 -ot http --admin 0.0.0.0 11001 --admin-insecure-mode --genesis-url http://host.docker.internal:9000/genesis --endpoint http://localhost:8001/ --seed {arguments["Seed"]} --debug-connections --auto-provision --wallet-type indy --wallet-name {arguments["Name"]} --wallet-key {arguments["WalletSecret"]}",
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         WindowStyle =  ProcessWindowStyle.Minimized,
    //         // CreateNoWindow = true
    //     };

    //     Process process = new Process();
    //     process.StartInfo = startInfo;

    //     process.OutputDataReceived += (sender, e) =>
    //     {
    //         if (!string.IsNullOrEmpty(e.Data))
    //         {
    //             Console.WriteLine(e.Data);
    //         }
    //     };

    //     process.ErrorDataReceived += (sender, e) =>
    //     {
    //         if (!string.IsNullOrEmpty(e.Data))
    //         {
    //             Console.WriteLine(e.Data);
    //         }
    //     };

    //     // UnityEngine.Debug.Log("Running script now");

    //     process.Start();
    //     UnityEngine.Debug.Log("Running script now");
    //     process.BeginOutputReadLine();
    //     process.BeginErrorReadLine();
    //     UnityEngine.Debug.Log(process.StartInfo);
    //     UnityEngine.Debug.Log("Process start time:" + process.StartTime);
    //     // process.WaitForExit();
    // }

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
            process.StartInfo.Arguments = $"/k docker-compose up"; // Specify the compose file and command
    
            // if (additionalArgs != null && additionalArgs.Length > 0)
            // {
            //     string argsString = string.Join(" --env ", additionalArgs);
            //     process.StartInfo.Arguments += $" {argsString}";
            // }
            UnityEngine.Debug.Log("Directory of process.StartInfo.Arguments: " + process.StartInfo.Arguments);


            process.StartInfo.UseShellExecute = true;

            // TaskCompletionSource<object> processExitCompletionSource = new TaskCompletionSource<object>();

            // process.Exited += (sender, e) =>
            // {
            //     processExitCompletionSource.TrySetResult(null);
            // };

            process.EnableRaisingEvents = true;
            process.Start();

            bool processStarted = await Task.Run(() => process.WaitForExit(Timeout.Infinite));
            UnityEngine.Debug.Log("Process started?: " + processStarted);

            // await processExitCompletionSource.Task;
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


}
