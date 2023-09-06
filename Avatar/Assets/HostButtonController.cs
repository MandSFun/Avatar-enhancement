using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class HostButtonController : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        Image image = this.GetComponent<Image>();
        if (userdatapersist.Instance.isHost)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.white;
        }
    }

    public void Toggle()
    {
        userdatapersist.Instance.isHost = !userdatapersist.Instance.isHost; 
    }
}
