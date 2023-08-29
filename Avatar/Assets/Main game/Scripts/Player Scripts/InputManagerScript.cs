using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class InputManagerScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private TMP_Text testName;
    private string playerName;
    private string nextSceneName;
    private void Start()
    {   
        enterButton.onClick.AddListener(SaveName);
        clearButton.onClick.AddListener(ClearName);
        //LoadName();
    }

    private void SaveName()
    {
        playerName = nameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
 
        //ClearName();
        if (playerName == "")//Check if the inputField is empty;
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
        else
            LoadNextScene();
    }

    private void ClearName()
    {
        nameInputField.text = "";
        PlayerPrefs.DeleteKey("PlayerName");
    }

    private void LoadName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
            nameInputField.text = playerName;
        }
    }

   private void LoadNextScene()
{
    nextSceneName = "Main";
    SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    DestroyPreviousSceneObjects();
}

private void DestroyPreviousSceneObjects()
{
    Scene previousScene = SceneManager.GetActiveScene();
    for (int i = 0; i < previousScene.rootCount; i++)
    {
        GameObject go = previousScene.GetRootGameObjects()[i];
        Destroy(go);
    }
}

}
