using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class InteractableObject : MonoBehaviour
{ 
    
    [SerializeField] private string weaponIdentifier;

    // Other methods and variables...

    public string GetWeaponIdentifier()
    {
        return weaponIdentifier;
    }

}
