using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PickableItemScript : MonoBehaviour
{
    public GameObject myHands; //reference to your hands/the position where you want your object to go
    bool canpickup; //a bool to see if you can or cant pick up the item
    GameObject objectToPickUp; // the gameobject onwhich you collided with
    public bool hasItem; // a bool to see if you have an item in your hand
    public static PickableItemScript instance;
    [SerializeField] private Item dynamiteItem;

    [SerializeField]Camera aimCam;
    

    List<GameObject> HiddenItems = new List<GameObject>();
    Animator animator;
    [SerializeField] public GameObject newDynamiteItem;
    [SerializeField] public GameObject newAk47Item;
    public int playerID;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        canpickup = false;    //setting both to false
        hasItem = false;
        animator = GetComponent<Animator>();
        playerID = NetworkManagerUI.instance.playerID;

    }

    // Update is called once per frame
    void Update()
    {
       

        // character is not holding something but has a selected object
        if (!hasItem && InventoryManager.instance.GetSelectedItem(false) != null && !hasItem) 
        {
           // Debug.Log("spawning new object into player hand");
            //spawn object into the players hand
            Item item = InventoryManager.instance.GetSelectedItem(false);
            
            if (item.name == "Dynamite")
            {
                GameObject newObject = new GameObject();
                newObject = Instantiate(newDynamiteItem);
                newObject.GetComponent<Rigidbody>().isKinematic = true;   //makes the rigidbody not be acted upon by forces
                BoxCollider[] bc = newObject.GetComponents<BoxCollider>();
                bc[0].enabled = false;
                newObject.transform.position = myHands.transform.position; // sets the position of the object to your hand position
                newObject.transform.parent = myHands.transform;
            }
            else if (item.name == "Ak47")
            {
                //newObject = Instantiate(newAk47Item);
                PlayerInteractable.Instance.TriggerPickupAnimation(this.transform.position,"AK47");
            }
            //makes the object become a child of the parent so that it moves with the hands
            hasItem = true;


        }else if (InventoryManager.instance.GetSelectedItem(false) == null  ) // character has not selected anything
        {
            //Debug.Log("Player not holding anything");
            //delete the gameobject that is in the players hands
            foreach (Transform transform in myHands.transform)
            {
                if (transform.CompareTag("PickableObject"))
                {
                    Destroy(transform.gameObject);
                    break;
                }

                //INSERT CODE HERE TO KEEP AK47
                if (transform.CompareTag("Weapon"))
                {
                    Destroy(transform.gameObject);
                    break;
                }
            }
            
            
            
            hasItem = false;
        }




        if (canpickup == true) // if you enter thecollider of the objecct
        {
            if (Input.GetKeyDown(KeyCode.F) && !hasItem) //picks up object and add to inventory manager
            {
                animator.SetTrigger("Pickup");
                StartCoroutine(DelayedPickUp(objectToPickUp)); 

            }   


        }

        if (Input.GetKeyDown(KeyCode.F) && hasItem) // if you have an item and get the key to remove the object, again can be any key
        {
            objectToPickUp.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again

            objectToPickUp.transform.parent = null; // make the object not be a child of the hands
            hasItem = false;

            BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
            bc[0].enabled = true;


        }


        try
        {
           
            if (Input.GetKeyDown(KeyCode.Mouse1) && hasItem == true && !PlayerInteractable.Instance.hasWeapon)  // holding a pickable object
            {

                //if the pickableobject is a dynamite
                if (objectToPickUp.GetComponent<Grenade>() != null)
                {
                    animator.SetTrigger("Throw");
                    StartCoroutine(DelayedThrow(objectToPickUp));
                }
               

            }
        }
        catch (NullReferenceException e)
        {
            //  Block of code to handle errors
            Console.WriteLine(e.Message);
            Debug.Log("Player not holding throwable item");

        }
        
    }

   
    private void OnTriggerEnter(Collider other) // to see when the player enters the collider
    {
       // Debug.Log("Picakable object found");
        if (other.gameObject.tag == "PickableObject") //on the object you want to pick up set the tag to be anything, in this case "object"
        {
            canpickup = true;  //set the pick up bool to true
            objectToPickUp = other.gameObject; //set the gameobject you collided with to one you can reference
          //  Debug.Log("Picakable object found");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        canpickup = false; //when you leave the collider set the canpickup bool to false

    }



    private IEnumerator DelayedThrow(GameObject objectToThrow)
    {
        // Wait for x seconds
        yield return new WaitForSeconds(0.713f);
        objectToPickUp.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again
        objectToPickUp.transform.parent = null; // make the object not be a child of the hands 
        hasItem = false;
        BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
        bc[0].enabled = true;
        //throw dynamite in direction of camera
        Vector3 aimDir = Camera.main.transform.forward;
        Rigidbody rb = objectToPickUp.GetComponent<Rigidbody>();
        //trigger the dynamite to explode 
        objectToPickUp.GetComponent<Grenade>().isTriggered = true;
        //remove the dynamite from inventory
        InventoryManager.instance.GetSelectedItem(true);
        rb.AddForce(aimDir.normalized * 20f + Vector3.up * 5f, ForceMode.Impulse);
        
    }
    private IEnumerator DelayedPickUp(GameObject objectToPickUp) {
        // Wait for x seconds
        yield return new WaitForSeconds(0.713f);
        objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;   //makes the rigidbody not be acted upon by forces
        BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
        bc[0].enabled = false;  //disable the collider, KEEP TRIGGER ACTIVE
                                //add item to inventory slot
        if (objectToPickUp.GetComponent<Grenade>())
        {
           // Debug.Log("Adding item to player inventory with ID = " + playerID);
            InventoryManager.instance.AddItem(dynamiteItem, true, playerID);
        }

        if (hasItem == true)
        {
            // destroy the old gameobject and attatch the new one
            GameObject currentHeldObject = GameObject.FindGameObjectWithTag("PickableObject");
            Destroy(currentHeldObject);
        }

        objectToPickUp.transform.position = myHands.transform.position; // sets the position of the object to your hand position
        objectToPickUp.transform.parent = myHands.transform; //makes the object become a child of the parent so that it moves with the hands
        hasItem = true;
        Quaternion myRotation = Quaternion.identity;
        myRotation.eulerAngles = new Vector3(-7.5f, 172, -260);
        objectToPickUp.transform.rotation = myRotation;

    }




}
