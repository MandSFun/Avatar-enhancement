using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBackButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
   public void BackToMainScene()
    {
        SceneManager.LoadScene("Main", LoadSceneMode.Additive);
    }


    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
