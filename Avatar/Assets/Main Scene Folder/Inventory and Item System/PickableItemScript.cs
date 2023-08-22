using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickableItemScript : MonoBehaviour
{
    public GameObject myHands; //reference to your hands/the position where you want your object to go
    bool canpickup; //a bool to see if you can or cant pick up the item
    GameObject objectToPickUp; // the gameobject onwhich you collided with
    GameObject collidedObject;
    public bool hasItem; // a bool to see if you have an item in your hand
    public static PickableItemScript instance;
    [SerializeField] private Item dynamiteItem;
    [SerializeField] private Item smokeGrenadeItem;
    [SerializeField] private Item grenadeItem;
    [SerializeField] private Item backpackItem;

    [SerializeField] Camera aimCam;


    List<GameObject> HiddenItems = new List<GameObject>();
    Animator animator;
    [SerializeField] public GameObject newDynamiteObj;
    [SerializeField] public GameObject newSmokeGrenadeObj;
    [SerializeField] public GameObject newAk47Obj;
    [SerializeField] public GameObject newGrenadeObj;
    public int playerID;
    public Item currItem;
    public bool isPlayingAnimation = false;

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
        try
        {
            playerID = LoginController.Instance.verifiedUsername.GetHashCode();
        }
        catch (System.Exception e)
        {
            playerID = NetworkManagerUI.instance.localPlayerID;
            Debug.Log("Unable to get playerID from SQL Server. Using default playerID from local username: " + playerID);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("Pickup") || this.animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
        {
            isPlayingAnimation = true;
            Debug.Log("playing pickup or throw");
        }
        else
        {
            isPlayingAnimation = false;
        }
        
        ActivatePlayerAction();
        UpdatePlayerHeldItem();
        if (collidedObject != null && collidedObject.tag == "MiniGameTrigger" && Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadSceneAsync("Shooting Game");
        }
        //Debug.Log(collidedObject.tag);

    }

    private void ActivatePlayerAction()
    {
        Transform currentHeldObject = FindWithTag(gameObject, "PickableObject");
        //Debug.Log("object to pick = " + objectToPickUp + " and the object held = " + currentHeldObject);

        //pick the object
        if (!isPlayingAnimation && canpickup == true 
            && Input.GetKeyDown(KeyCode.F) 
            && currentHeldObject != objectToPickUp
            && !isHoldingInteractableObject()) // if you enter thecollider of the object and press F

        {
            PlayerInteractable.Instance.SetHasWeaponFalse();
            canpickup = false;
            isPlayingAnimation = true;
            animator.SetTrigger("Pickup");
            StartCoroutine(DelayedPickUp(objectToPickUp));
            hasItem = true;

        }

        //drop the object
        else if (!isPlayingAnimation && Input.GetKeyDown(KeyCode.V) && hasItem) // DROP THE OBJECT
        {
            objectToPickUp.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again
            objectToPickUp.transform.parent = null; // make the object not be a child of the hands
            hasItem = false;
            BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
            bc[0].enabled = true;
            InventoryManager.instance.GetSelectedItem(true);

        }


        //throw the object
        else if (!isPlayingAnimation && Input.GetKeyDown(KeyCode.Mouse1) && hasItem && !PlayerInteractable.Instance.hasWeapon) // holding a pickable object
        {

            Transform objectToThrow = FindWithTag(gameObject, "PickableObject");
            Debug.Log("detected pickable object to throw" + objectToThrow);

            //if the pickableobject is a dynamite
            if ((objectToThrow.GetComponent<Grenade>() != null))
            {
                animator.SetTrigger("Throw");
                StartCoroutine(DelayedThrow(objectToThrow));
            }
            else if ((objectToThrow.GetComponent<SmokeGrenade>() != null))
            {
                animator.SetTrigger("Throw");
                StartCoroutine(DelayedThrow(objectToThrow));
            }


        }
    }

    private void UpdatePlayerHeldItem()
    {
       
        if (InventoryManager.instance.GetSelectedItem(false) == null) // character has not selected anything
        {
            RemovePlayerHeldItem();
        }
        else if (InventoryManager.instance.GetSelectedItem(false).name == "Backpack")
        {
            //trigger inventory menu
            InventoryManager.instance.backpackScreen.SetActive(true);
        }
        //character has chosen another object
        else if (hasItem && InventoryManager.instance.GetSelectedItem(false) != currItem)
        {
            RemovePlayerHeldItem();
            SpawnNewPlayerItem();
        }
        // character is not holding something but has a selected object
        else if ((!hasItem && InventoryManager.instance.GetSelectedItem(false) != null && !hasItem))
        {
            // Debug.Log("spawning new object into player hand");
            //spawn object into the players hand
            SpawnNewPlayerItem();

        }
    }

    private void RemovePlayerHeldItem()
    {
        InventoryManager.instance.backpackScreen.SetActive(false);
        hasItem = false;
        //Debug.Log("Player not holding anything");
        //delete the gameobject that is in the players hands

        if (PlayerInteractable.Instance.hasWeapon)
        {
            PlayerInteractable.Instance.SetHasWeaponFalse();
            PlayerInteractable.Instance.SetHasWeapon(false);
        }
        else
        {
            DestroyHeldItem();
        }

        hasItem = false;
        currItem = null;
        
    }

    private void DestroyHeldItem()
    {
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
    }

    private void SpawnNewPlayerItem()
    {
        InventoryManager.instance.backpackScreen.SetActive(false);
        Item item = InventoryManager.instance.GetSelectedItem(false); 
        try
        {
            
            if (item != null && item.name == "Dynamite")
            {
                //GameObject newObject = newDynamiteObj;
                //newObject = Instantiate(newDynamiteObj);
                GameObject newObject = Instantiate(newDynamiteObj) as GameObject;
                SpawnAndHold(newObject, GetTransform());

            }
            else if (item != null && item.name == "Grenade")
            {
                //GameObject newObject = newGrenadeObj;
                //newObject = Instantiate(newGrenadeObj);
                GameObject newObject = Instantiate(newGrenadeObj) as GameObject;
                SpawnAndHold(newObject, GetTransform());

            }

            else if (item != null && item.name == "SmokeGrenade")
            {
               // GameObject newObject = newSmokeGrenadeObj;
                //newObject = Instantiate(newSmokeGrenadeObj);
                GameObject newObject = Instantiate(newSmokeGrenadeObj) as GameObject;
                SpawnAndHold(newObject, GetTransform());

            }
            else if (item != null && item.name == "AK47")
            {
                
                //GameObject newObject = Instantiate(newAk47Obj) as GameObject;
                PlayerInteractable.Instance.TriggerPickupAnimation(transform.position, "AK47", false);
                PlayerInteractable.Instance.hasWeapon = true;
            }
            //makes the object become a child of the parent so that it moves with the hands
            hasItem = true;
            currItem = item;
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        

        
    }

    private Transform GetTransform()
    {
        return transform;
    }

    private void SpawnAndHold(GameObject newObject, Transform transform)
    {
        newObject.GetComponent<Rigidbody>().isKinematic = true;   //makes the rigidbody not be acted upon by forces
        BoxCollider[] bc = newObject.GetComponents<BoxCollider>();
        bc[0].enabled = false;
        newObject.transform.position = myHands.transform.position; // sets the position of the object to your hand position
        newObject.transform.parent = myHands.transform;
        newObject.transform.rotation = Quaternion.Euler(myHands.transform.eulerAngles.x, myHands.transform.eulerAngles.y, myHands.transform.eulerAngles.z - 90);
    }


    private void OnTriggerEnter(Collider other) // to see when the player enters the collider
    {
         Debug.Log("Collided with " + other.gameObject);
        if (other.gameObject.tag == "PickableObject") //on the object you want to pick up set the tag to be anything, in this case "object"
        {
            Debug.Log("Picakable object found" + other);
            canpickup = true;  //set the pick up bool to true
            objectToPickUp = other.gameObject; //set the gameobject you collided with to one you can reference
                                               //  Debug.Log("Picakable object found");
        }
        collidedObject = other.gameObject;

        //open mini game scene
        
    }
    private void OnTriggerExit(Collider other)
    {
        canpickup = false; //when you leave the collider set the canpickup bool to false
        collidedObject = null;

    }



    private IEnumerator DelayedThrow(Transform objectToThrow)
    {
        // Wait for x seconds
        yield return new WaitForSeconds(0.713f);
        objectToThrow.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again
        objectToThrow.transform.parent = null; // make the object not be a child of the hands 
        hasItem = false;
        BoxCollider[] bc = objectToThrow.GetComponents<BoxCollider>();
        bc[0].enabled = true;
        //throw dynamite in direction of camera
        //Vector3 aimDir = Camera.main.transform.forward;
        Vector3 aimDir = transform.forward;
        Rigidbody rb = objectToThrow.GetComponent<Rigidbody>();
        //trigger the dynamite to explode
        if (objectToThrow.GetComponent<Grenade>() != null)
        {
            objectToThrow.GetComponent<Grenade>().isTriggered = true;
        }
        else if (objectToThrow.GetComponent<SmokeGrenade>() != null)
        {
            objectToThrow.GetComponent<SmokeGrenade>().isTriggered = true;
        }

        InventoryManager.instance.GetSelectedItem(true);
        rb.AddForce(aimDir.normalized * 20f + Vector3.up * 5f, ForceMode.Impulse);

    }
    private IEnumerator DelayedPickUp(GameObject objectToPickUp)
    {
        // Wait for x seconds
        yield return new WaitForSeconds(0.713f);
        objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;   //makes the rigidbody not be acted upon by forces
        BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
        bc[0].enabled = false;  //disable the collider, KEEP TRIGGER ACTIVE
                                //add item to inventory slot
        if (objectToPickUp.GetComponent<Grenade>())
        {
            Debug.Log("Adding item to player inventory with ID = " + playerID);
            if (objectToPickUp.GetComponent<Grenade>().type == 0)
            {
                InventoryManager.instance.AddItem(dynamiteItem, true, playerID);
                currItem = dynamiteItem;
            }else if (objectToPickUp.GetComponent<Grenade>().type == 1)
            {
                InventoryManager.instance.AddItem(grenadeItem, true, playerID);
                currItem = grenadeItem;
            }
            
        }
        else if (objectToPickUp.GetComponent<SmokeGrenade>())
        {
            Debug.Log("Adding item to player inventory with ID = " + playerID);
            InventoryManager.instance.AddItem(smokeGrenadeItem, true, playerID);
            currItem = smokeGrenadeItem;
        }

        if (hasItem)
        {
            // destroy the old gameobject and attatch the new one
            //GameObject currentHeldObject = GameObject.FindGameObjectWithTag("PickableObject");
            // Destroy(currentHeldObject);es
            DestroyHeldItem();
        }

        objectToPickUp.transform.position = myHands.transform.position; // sets the position of the object to your hand position
        objectToPickUp.transform.parent = myHands.transform; //makes the object become a child of the parent so that it moves with the hands
        hasItem = true;
        Quaternion myRotation = Quaternion.identity;
        myRotation.eulerAngles = new Vector3(-7.5f, 172, -260);
        objectToPickUp.transform.rotation = myRotation;
        

    }

    Transform FindWithTag(GameObject root, string tag)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag(tag)) return t;
        }
        return null;
    }

    public bool isHoldingInteractableObject()
    {
        if (InventoryManager.instance.GetSelectedItem(false) == null)
        {
            return false;
        }
        else if (InventoryManager.instance.GetSelectedItem(false).name == "AK47")
        {
            return true;
        }
        return false;
    }


}
