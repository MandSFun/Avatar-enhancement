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
        //Set the loader callback action to laod the target scene
        loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());
        while(!loadingAsyncOperation.isDone){
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

    public static void LoaderCallback(){
        // Triggered after first update which lets the scene refresh
        // Execute the loader callback action which will load the target scene
        if(onLoaderCallback != null){
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }

}
