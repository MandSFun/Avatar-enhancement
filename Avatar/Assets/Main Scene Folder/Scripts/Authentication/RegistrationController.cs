using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RegistrationController : MonoBehaviour
{
    public void RedirectToLogin(){
        Loader.Load(Loader.Scene.Login);
    }
}
