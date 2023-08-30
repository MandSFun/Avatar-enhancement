using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurrenderAnimation : MonoBehaviour
{
    private int animSurrender;
    public Animator anim;

    void Start()
    {animSurrender = Animator.StringToHash("Surrender");

    }

    // Update is called once per frame
    void Update()
    {if(Input.GetKeyDown(KeyCode.P)){
        anim.SetBool(animSurrender, true);
    }

    }
}
