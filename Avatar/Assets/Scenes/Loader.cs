using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene {
        Main, 
        Name, 
        Loading
    }

    public static Action onLoaderCallback;

    // Start is called before the first frame update
    public static void Load(Scene scene)
    {
        //Set the loader callback action to laod the target scene
        onLoaderCallback = () => {
            SceneManager.LoadScene(scene.ToString());
        };

        //Load the loading screen
        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    public static void LoaderCallback(){
        // Triggered after first update which lets the scene refresh
        // Execute the loader callback action which will load the target scene
        if(onLoaderCallback != null){
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }

}
