using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkStatusIconScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
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
