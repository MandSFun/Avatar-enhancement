using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingProgressText : MonoBehaviour
{
    public TMP_Text m_Text;

    // Start is called before the first frame update
    // void Start()
    // {
    //     slider.value = 0;
    // }
    private void Awake()
    {
        m_Text = transform.GetComponent<TMP_Text>();
    }

    private void Update(){
        //Output the current progress
        m_Text.text = Loader.LoadingProgressText();
    }
}
