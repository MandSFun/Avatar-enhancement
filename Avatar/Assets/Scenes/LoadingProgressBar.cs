using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingProgressBar : MonoBehaviour
{
    public Slider slider;

    // Start is called before the first frame update
    // void Start()
    // {
    //     slider.value = 0;
    // }
    private void Awake()
    {
        slider = transform.GetComponent<Slider>();
    }

    private void Update(){
        slider.value = Loader.GetLoadingProgress();
    }

}
