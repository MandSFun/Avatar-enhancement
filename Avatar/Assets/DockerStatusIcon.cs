using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


public class DockerStatusIcon : MonoBehaviour
{
    public bool SQLServerConnection = false;
    public static DockerStatusIcon instance; 


    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        Image image = this.GetComponent<Image>();
        if (SQLServerConnection)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.red;


            string hostName = Dns.GetHostName();
            string IPAddress = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            string adminConString = "Data Source=" + IPAddress + ";Initial Catalog=master;User ID=sa;Password=D5taCard;";
                SqlConnection con = new SqlConnection(adminConString);
                try
                { 
                    con.Open();
                    Debug.Log("SQL server connection successful!");
                    con.Close();
                    SQLServerConnection = true;
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR SQL server connection unsuccessful!");
                SQLServerConnection = false;
                }

                

        }
    }
}
 