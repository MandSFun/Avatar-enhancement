using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class Loader
{
    private class LoadingMonoBehaviour: MonoBehaviour { }
    public enum Scene {
        Main, 
        Name, 
        Loading,
        SplashScreen
    }

    public static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;

    // Start is called before the first frame update
    public static void Load(Scene scene)
    {
        //Set the loader callback action to laod the target scene
        onLoaderCallback = () => {
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
        };

        //Load the loading screen
        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    public static IEnumerator LoadSceneAsync(Scene scene)
    {
        // yield return null;
        //Set the loader callback action to load the target scene
        loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());

        //Don't let the Scene activate until you allow it to
        loadingAsyncOperation.allowSceneActivation = false;

        while(!loadingAsyncOperation.isDone){

            if (loadingAsyncOperation.progress >= 0.9f)
            {
                //Wait to you press the space key to activate the Scene
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    //Activate the Scene
                    loadingAsyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public static float GetLoadingProgress() {
        if (loadingAsyncOperation != null){
            return loadingAsyncOperation.progress/0.9f;
            // return null;
            // return progress;
        } else {
            return 0;
        }
    }

    public static string UpdateLoadingText() {
        
        if (loadingAsyncOperation != null){
            // Check if the load has finished
            if (loadingAsyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                return "Press the spacebar/Enter to continue";
                //Wait to you press the space key to activate the Scene
            } else {
                return " ";
            }
        } else {
            return " ";
        }
    }

    public static string LoadingProgressText() {
        
        if (loadingAsyncOperation != null){
            // Check if the load has finished
            return "LOADING PROGRESS: " + (loadingAsyncOperation.progress/0.9f * 100) + "/100%";
        } else {
            return "LOADING PROGRESS: 0/100%";
        }
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
