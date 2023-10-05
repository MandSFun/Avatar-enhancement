using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Agreement;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Cms;
using System.ComponentModel.Design;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using UnityEngine.Windows;
using System.Linq;
using Org.BouncyCastle.Utilities.IO.Pem;

public class DHMessager : MonoBehaviour
{

    public static DHMessager instance;
    public string username;
    public static byte[] userPublicKey;
    [SerializeField] public TMP_InputField ReceiverNameInputField;
    [SerializeField] public TMP_InputField MessageString;
    public string ReceiverName;
    public string IPAddress;
    public string ledgerUrl;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    private static readonly HttpClient client = new HttpClient();
    public static byte[] alicePublicKey;
    public string message;
    public string hashedReceiverUserName;
    private DHParameters dhParameters;
    private AsymmetricCipherKeyPair keyPair;
    public static AsymmetricCipherKeyPair A;
    public static DHParameters dHParameters;

    private void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        try
        {
            username = userdatapersist.Instance.verifiedUser;
        }
        catch (System.Exception e)
        {
            Debug.Log("Unable to get username from Login page.");
        }


    }


    public void EncryptAndSend()
    {
        message = MessageString.text;
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        string encryptedString;

    }



    public void AShareParams()
    {
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        var generator = new DHParametersGenerator();
        generator.Init(512, 10, new SecureRandom());
        DHParameters param = generator.GenerateParameters();
        PostDHParametersAsString(param, hashedReceiverUserName + "params");
        // Generate key pair for Party A
        var keyGen1 = GeneratorUtilities.GetKeyPairGenerator("DH");
        var kgp1 = new DHKeyGenerationParameters(new SecureRandom(), param);
        keyGen1.Init(kgp1);
        A = keyGen1.GenerateKeyPair();
        string stringDHStaticKeyPairPartyA = GetPublicKey(A);
        Debug.Log("person A's public key is " + stringDHStaticKeyPairPartyA);
        PostStaticPublicKey(stringDHStaticKeyPairPartyA, hashedReceiverUserName + "A");  //post public static key

    }

    public void BGetParamsAndCalcSecret()
    {
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        DHParameters foundDHParams = GetDHParams(hashedReceiverUserName + "params", ledgerUrl);
        var keyGen2 = GeneratorUtilities.GetKeyPairGenerator("DH");
        var kgp2 = new DHKeyGenerationParameters(new SecureRandom(), foundDHParams);
        keyGen2.Init(kgp2);
        AsymmetricCipherKeyPair B = keyGen2.GenerateKeyPair();

        string StaticKeyString = GetStaticKeyString(hashedReceiverUserName + "A", ledgerUrl);
        Debug.Log("Found StaticKeyString of A by B is " + StaticKeyString);

        // B calc
        var importedKey = new DHPublicKeyParameters(new BigInteger(StaticKeyString), foundDHParams);
        var internalKeyAgreeB = AgreementUtilities.GetBasicAgreement("DH");
        internalKeyAgreeB.Init(B.Private);
        string stringDHStaticKeyPairPartyB = GetPublicKey(B);
        Debug.Log("person B's public key is " + stringDHStaticKeyPairPartyB);
        PostStaticPublicKey(stringDHStaticKeyPairPartyB, hashedReceiverUserName + "B");  //post public static key
        BigInteger Bans = internalKeyAgreeB.CalculateAgreement(importedKey);
        Debug.Log("B ans is " + Bans.ToString());



    }

    public void ACalculateSecret()
    {
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        DHParameters foundDHParams = GetDHParams(hashedReceiverUserName + "params", ledgerUrl);
        string StaticKeyString = GetStaticKeyString(hashedReceiverUserName + "B", ledgerUrl);
        Debug.Log("Found StaticKeyString of B by A is " + StaticKeyString);
        var importedKeyA = new DHPublicKeyParameters(new BigInteger(StaticKeyString), foundDHParams);
        var internalKeyAgreeA = AgreementUtilities.GetBasicAgreement("DH");
        internalKeyAgreeA.Init(A.Private);
        BigInteger Aans = internalKeyAgreeA.CalculateAgreement(importedKeyA);
        Debug.Log("A ans is " + Aans.ToString());

    }

    // This returns A
    public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
    {
        var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
        if (dhPublicKeyParameters != null)
        {
            return dhPublicKeyParameters.Y.ToString();
        }
        throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
    }

    // This returns a
    public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
    {
        var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
        if (dhPrivateKeyParameters != null)
        {
            return dhPrivateKeyParameters.X.ToString();
        }
        throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
    }



    public static string PublicKeyToString(AsymmetricKeyParameter publicKey)
    {
        SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
        byte[] publicKeyBytes = publicKeyInfo.GetDerEncoded();

        // Convert the public key bytes to a Base64-encoded string
        string publicKeyString = Convert.ToBase64String(publicKeyBytes);

        return publicKeyString;
    }

    public static AsymmetricKeyParameter PublicStaticKeyFromString(string publicKeyBase64)
    {
        try
        {
            // Decode the Base64 string to get the byte array
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);

            // Parse the byte array as a SubjectPublicKeyInfo structure
            Asn1InputStream asn1Stream = new Asn1InputStream(publicKeyBytes);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfo.GetInstance(asn1Stream.ReadObject());

            // Extract the public key parameters
            AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(publicKeyInfo);

            return publicKey;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating public key from string: {ex.Message}");
            return null;
        }
    }

    

    public async void PostStaticPublicKey(string input, string schemaName)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";



        List<string> result = new List<string>();

        for (int i = 0; i < input.Length; i += 250)
        {
            int length = Math.Min(250, input.Length - i);
            result.Add(input.Substring(i, length));
        }

        if (result.Count == 1)
        {
            Debug.Log("Count is 1, " + result[0]);
            using (HttpClient httpClient = new HttpClient())
            {

                // Prepare the JSON payload
                string jsonPayload = $@"{{
                ""attributes"": [
                    ""1.{result[0]}""      
                    
                   
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""2.2""
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
                    windowMessage.text = "Posted to ledger!";


                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Key is wrong size!";
        }


    }

    public async void postToLedger(string schemaName, string attribute, int version)
    {

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";

        using (HttpClient httpClient = new HttpClient())
        {

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""{attribute}""                
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""{version}.0""
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
                windowMessage.text = "Posted to ledger!";


            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
            }
        }

    }


    public DHParameters GetDHParams(string username, string ledgerUrl)
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
                    var type = responseData["version"];
                    var credName = responseData["name"];
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "2.1" && credName.ToString() == username) // found a DH paramter that is to connect to you
                    {
                        Debug.Log("The found attributes are " + attributes.ToString());
                        DHParameters foundDHParams = ExtractDHParameters(attributes.ToString());
                        return foundDHParams;
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
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

        return null;

    }

    
    public string GetStaticKeyString(string username, string ledgerUrl)
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
                    var type = responseData["version"];
                    var credName = responseData["name"];
                    var attributes = responseData["attr_names"];

                    if (type.ToString() == "2.2" && credName.ToString() == username)
                    {
                        Debug.Log("The found static attributes 1 are " + attributes[0].ToString());
                        string[] stringsToSort = { attributes[0].ToString() };

                        // Sort the strings by their first character
                        var sortedStrings = stringsToSort.OrderBy(str => str[0]);
                        string foundKey = "";
                        foreach (var str in sortedStrings)
                        {
                            string cutString = str.Substring(2);
                            foundKey += cutString;
                        }
                        Debug.Log("Found static key = " + foundKey);

                        return foundKey;
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
            popupWindow.SetActive(true);
            windowMessage.text = "Error parsing transactions!";
        }

        return null;

    }


    public static DHParameters ExtractDHParameters(string input)
    {
        // Define regular expressions to match each parameter
        string pPattern = @"P:(\d+)";
        string gPattern = @"G:(\d+)";
        string qPattern = @"Q:(\d+)";
        string mPattern = @"M:(\d+)";
        string lPattern = @"L:(\d+)";
        string jPattern = @"J:(\d+)";

        // Match each parameter using the regular expressions
        Match pMatch = Regex.Match(input, pPattern);
        Match gMatch = Regex.Match(input, gPattern);
        Match qMatch = Regex.Match(input, qPattern);
        Match mMatch = Regex.Match(input, mPattern);
        Match lMatch = Regex.Match(input, lPattern);
        Match jMatch = Regex.Match(input, jPattern);



        BigInteger p = pMatch.Success ? new BigInteger(pMatch.Groups[1].Value) : BigInteger.Zero;
        BigInteger g = gMatch.Success ? new BigInteger(gMatch.Groups[1].Value) : BigInteger.Zero;
        BigInteger q = qMatch.Success ? new BigInteger(qMatch.Groups[1].Value) : BigInteger.Zero;
        int m = mMatch.Success ? int.Parse(mMatch.Groups[1].Value) : 0;
        int l = lMatch.Success ? int.Parse(lMatch.Groups[1].Value) : 0;
        BigInteger j = jMatch.Success ? new BigInteger(jMatch.Groups[1].Value) : BigInteger.Zero;

        Debug.Log("P: " + p.ToString() + "G: " + g.ToString() + " Q: " + q.ToString() + "J: " + j.ToString()
            + "M: " + m.ToString() + "L: " + l.ToString());

        return new DHParameters(p, g, q, m, l, j, null); // Adjust as needed

    }



    public async void PostDHParametersAsString(DHParameters dhParams, string schemaName)
    {
        if (dhParams == null)
        {
            throw new ArgumentNullException(nameof(dhParams));
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("DH Parameters:");
        sb.AppendLine($"P (Prime Modulus): {dhParams.P}");
        sb.AppendLine($"G (Generator): {dhParams.G}");
        sb.AppendLine($"Q (Factor): {dhParams.Q}");
        sb.AppendLine($"J (Subgroup Factor): {dhParams.J}");
        sb.AppendLine($"M: {dhParams.M}");
        sb.AppendLine($"L: {dhParams.L}");

        Debug.Log(sb.ToString());

        //string url = "http://localhost:11001/schemas?create_transaction_for_endorser=false";
        string url = "http://" + IPAddress + ":11001/schemas?create_transaction_for_endorser=false";

        using (HttpClient httpClient = new HttpClient())
        {

            // Prepare the JSON payload
            string jsonPayload = $@"{{
                ""attributes"": [
                    ""P:{dhParams.P}"", 
                    ""G:{dhParams.G}"",
                    ""Q:{dhParams.Q}"",
                    ""J:{dhParams.J}"",
                    ""M:{dhParams.M}"",
                    ""L:{dhParams.L}""
                ],
                ""schema_name"": ""{schemaName}"",
                ""schema_version"": ""2.1""
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
                windowMessage.text = "Posted DH parameters to ledger!";


            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
            }
        }


    }


    public AsymmetricCipherKeyPair generateDHKeyPair(DHParameters parameters)
    {
        var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
        var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
        keyGen.Init(kgp);
        return keyGen.GenerateKeyPair();
    }



}