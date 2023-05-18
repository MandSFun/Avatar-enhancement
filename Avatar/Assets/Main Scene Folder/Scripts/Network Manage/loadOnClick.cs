using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;
using Newtonsoft.Json;


public class loadOnClick : MonoBehaviour
{ 
    public GameObject blackOutSquare;
    public void Update(){
        if(Input.GetKeyDown(KeyCode.Return)){
            StartCoroutine(FadeBlackOutSquare());
        }
    }

    public IEnumerator FadeBlackOutSquare(bool fadeToBlack = true, int fadeSpeed = 1){
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        float fadeAmount;
        if (fadeToBlack){
            while (blackOutSquare.GetComponent<Image>().color.a < 1){
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);
                objectColor = new Color(objectColor.r, objectColor. g,objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
            Loader.Load(Loader.Scene.Name);
        }

    }
    

}
