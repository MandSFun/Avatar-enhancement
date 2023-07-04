using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class AimRightClick : MonoBehaviour
{
    private bool hasWeaponSpawned = false;
    private Shooting Gun;
    [SerializeField]private Rig aimingRig;
    PrometeoCarController prom;
    private float raisingtime = 0.18f;
    bool isAiming = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(1) &&Gun !=null ){
            aimingRig.weight +=Time.deltaTime / raisingtime;
            isAiming = true;
        }else{
            aimingRig.weight -= Time.deltaTime/raisingtime;
            isAiming = false;
        }//In this case,Right click aim can be added by creating a seperate anim rigging.
        //Duplicate of Riglayer_aiming and its child weaponAim.
        //Remove Multi-Aim Constraint.
        //Set reference to new Riglayer_aiming and replace "rig " accordingly.
         if(isAiming== true )
        {

            if(Input.GetKeyDown(KeyCode.G) && Gun!=null){
                Gun.isFiring();   
            }
        }
    }
     private void OnEnable()
    {
        PlayerInteractable.OnHasWeaponChanged += HandleHasWeaponChanged;
    }

    private void OnDisable()
    {
        PlayerInteractable.OnHasWeaponChanged -= HandleHasWeaponChanged;
    }

    private void HandleHasWeaponChanged(bool value)
    {
        hasWeaponSpawned = value;
       

        GameObject weaponinPlayer = GameObject.FindGameObjectWithTag("Weapon");
      Gun =weaponinPlayer.GetComponent<Shooting>();

    }
}
