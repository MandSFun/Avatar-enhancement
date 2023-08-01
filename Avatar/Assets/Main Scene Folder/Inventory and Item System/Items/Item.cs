using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
   // public string id;
    public string name;
    public Sprite image;
    public bool stackable;
    public GameObject prefab;
    
}
