using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using I18N.Common;
using System.Globalization;

public class CredentialIssuer : MonoBehaviour
{

    
    [SerializeField] private TMP_InputField userIDInputField;
    [SerializeField] private TMP_InputField expiryInputField;
    [SerializeField] private TMP_Text issuerName;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    private string IPAddress;
    private string issuer;
    bool validInput = false;
    // Start is called before the first frame update
    void Start()
    {
        IPAddress = LoginController.instance.IPAddress;
        issuer = LoginController.instance.verifiedUsername;
        issuerName.text = issuer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void GenerateCredential()
    {
       
        //Get ID and expiry from input fields
        string userID = userIDInputField.text;
        int expiryDate = -1;
        
        //check if date in the valid format
        try
        {
            expiryDate = Int32.Parse(expiryInputField.text);
            DateTime date = DateTime.ParseExact(expiryInputField.text, "ddmmyyyy", CultureInfo.InvariantCulture);
            Debug.Log("Input date is " + date);
            validInput = true;
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to parse '{expiryInputField.text}'");
            popupWindow.SetActive(true);
            windowMessage.text = "Expiry Date is in the wrong format!";
        }

        if (validInput)
        {
            int CredentialID = (userID + issuer + expiryInputField.text).GetHashCode();
            sendReq(userID, CredentialID, userID, expiryDate);
        }
        
        
    }


    public async void sendReq(string issuer, int credentialID, string userID, int expiry)
    {
       
            string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";

            using (HttpClient httpClient = new HttpClient())
            {
            

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""{expiry.ToString()}"",
                    ""{userID.GetHashCode()}""                
                ],
                ""schema_name"": ""{credentialID.ToString()}"",
                ""schema_version"": ""1.0""
            }}";

            // Set headers
            httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create the request content
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    popupWindow.SetActive(true);
                    windowMessage.text = "Credential Generation Success! \n Credential ID = " + credentialID; 
                    
                    AddCredentialToServer(issuer, credentialID, userID, expiry);
            }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        
    }

    public bool AddCredentialToServer(string issuer, int credentialID,string userID, int expiry)
    {
        string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=AvatarProject;User ID=sa;Password=D5taCard;";
        SqlConnection con = new SqlConnection(adminConString);
        

        try
        {
            using (SqlConnection connection = new SqlConnection(adminConString))
            {

                connection.Open();
                LoginController.instance.CreateNewDB();
                LoginController.instance.CreateTables();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO IssuedCredentials (CredentialID,Issuer,UserID,Expiry) VALUES (" + credentialID + ",'" + issuer + "','" + userID + "'," + expiry + ");";
                    command.ExecuteNonQuery();
                    Debug.Log("(SQL server) credential added with id: " + credentialID + " by user " + issuer);
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("(SQL Server) Error inserting credential!");

        }
        return false;
    }


   
}
