using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkStatusIconScript : MonoBehaviour
{

    [SerializeField] TMP_Text IPTextField;
    string connectedIPAddress;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        connectedIPAddress = SQLConnection.instance.IPAddress;
        IPTextField.text = "Server IP: " + connectedIPAddress;
        RawImage image = this.GetComponent<RawImage>();
        if (SQLConnection.instance.SQLServerConnected)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.red;
        }
    }
}