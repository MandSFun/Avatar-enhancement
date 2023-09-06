using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userdatapersist : MonoBehaviour
{
    // Make global
    public static userdatapersist Instance
    {
        get;
        set;
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        Instance = this;
    }


    // Data persisted between scenes
    public string verifiedUser = "";
    public string verifiedPassword = "";
    public string IPAdd = "";
    public bool isHost = true;
}
