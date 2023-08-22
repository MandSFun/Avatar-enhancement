using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class CredentialIssuer : MonoBehaviour
{

    
    [SerializeField] private TMP_InputField userIDInputField;
    [SerializeField] private TMP_InputField expiryInputField;
    [SerializeField] private TMP_Text issuerName;
    private string IPAddress;
    private string issuer;
    // Start is called before the first frame update
    void Start()
    {
        IPAddress = LoginController.Instance.IPAddress;
        issuer = LoginController.Instance.verifiedUsername;
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
        }
        catch (FormatException)
        {
            Console.WriteLine($"Unable to parse '{expiryInputField.text}'");
        }
        
        int CredentialID = (userID+issuer+expiryInputField.text).GetHashCode();
        AddCredentialToServer(issuer, CredentialID, userID, expiryDate);
        
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
                LoginController.Instance.CreateNewDB();
                LoginController.Instance.CreateTables();
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
