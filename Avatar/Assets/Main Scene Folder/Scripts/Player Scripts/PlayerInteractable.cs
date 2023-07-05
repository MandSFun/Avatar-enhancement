using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;
using System;

public class PlayerInteractable : NetworkBehaviour
{
    public static PlayerInteractable Instance { get; private set; }

    [SerializeField] private float interactionRange = 2f;
    private Animator anim;
    private int animPickup;
    private Transform currentInteractable;
    private bool isPickupAnimationPlaying;
    [SerializeField] private AimTrigger aimTrigger;

    [SerializeField] private Transform pickingupPlaceholder; // Reference to the hand bone GameObject;
    [SerializeField] private Transform weaponPlaceholder; // Empty GameObject acting as a placeholder for the weapon;

    // Dictionary to map weapon identifiers to their corresponding prefabs
    [SerializeField] private Dictionary<string, GameObject> weaponPrefabs;

    [SerializeField] private GameObject AK47prefab;
    [SerializeField] private float delayBeforeSpawn;

    [SerializeField] private RigBuilder rb;
    private GameObject weapon;

    [SerializeField] Transform shoulder;
    private bool hasWeapon;

    [SerializeField] private TwoBoneIKConstraint lefthandIK;
    [SerializeField] private TwoBoneIKConstraint righthandIK;
    [SerializeField] private MultiParentConstraint weaponPose;
    [SerializeField] private MultiParentConstraint TargetAiming;
    [SerializeField] private MultiParentConstraint rightclickAiming;

    public delegate void HasWeaponChanged(bool value);
    public static event HasWeaponChanged OnHasWeaponChanged;

    public DemoScript demoScript;
    [SerializeField]  private Item itemAK47;
    

    private void Start()
    {
        weaponPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs["AK47"] = AK47prefab;
        anim = GetComponent<Animator>();
        AssignAnimationIDs();
        rb = GetComponent<RigBuilder>();
     

        Debug.Log("Hello");
      //  rb.enabled = false;

        //     rb.enabled = true;
        //      rightHandgrip = testingweapon.transform.Find("rightgrip").transform;
        //      Debug.Log(rightHandgrip);
        //    leftHandgrip = testingweapon.transform.Find("leftgrip").transform;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void AssignAnimationIDs()
    {
        animPickup = Animator.StringToHash("Pickup");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Interactable"))
                {
                    InteractableObject objectInteraction = collider.GetComponent<InteractableObject>();
                    if (objectInteraction != null)
                    {
                        // Face the object
                        Vector3 lookDirection = collider.transform.position - transform.position;
                        lookDirection.y = 0f;
                        transform.rotation = Quaternion.LookRotation(lookDirection);

                        // Trigger the object's interaction


                        // Get the identifier of the weapon
                        string weaponIdentifier = objectInteraction.GetWeaponIdentifier();

                        // Trigger the pickup animation with the weapon identifier
                        TriggerPickupAnimation(collider.transform.position, weaponIdentifier);
                        AddItemToInventory(weaponIdentifier);
                        Debug.Log("Playing");
                        break;
                    }
                }
            }
        }
        
    }

    private void AddItemToInventory(string weaponIdentifier)
    {
        if (weaponIdentifier == "AK47")
        {
            InventoryManager.instance.AddItem(itemAK47);
            
        }
        
    }

    public void SetCurrentInteractable(Transform interactable)
    {
        currentInteractable = interactable;
    }

    public void TriggerPickupAnimation(Vector3 objectPosition, string weaponIdentifier)
    {
        // Face the object
        Vector3 lookDirection = objectPosition - transform.position;
        lookDirection.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        // Trigger the pickup animation
        anim.SetTrigger(animPickup);
        isPickupAnimationPlaying = true;

        // Check if the weapon identifier exists in the dictionary
        if (weaponPrefabs.ContainsKey(weaponIdentifier))
        {
            // Get the corresponding weapon prefab
            GameObject weaponPrefab = weaponPrefabs[weaponIdentifier];
            rb.enabled = true;
            // Call the SpawnWeapon method on the next frame with the correct prefab
            StartCoroutine(DelayedSpawnWeapon(weaponPrefab));
        }
        else
        {
            Debug.LogWarning("No prefab found for weapon identifier: " + weaponIdentifier);
        }
    }

    private IEnumerator DelayedSpawnWeapon(GameObject weaponPrefab)
    {
        //Delay before instantiating the weapon into the hand;
        yield return new WaitForSeconds(delayBeforeSpawn);

        // Spawn the weapon
        if (weaponPrefab != null)
        {

            SetHasWeaponTrue(weaponPrefab);
         SetHasWeapon(true);



        }
    }

    private void OnAnimatorMove() { }//Callback function by unity to overrirde the default root motion handling this behaviour;

    public void SetHasWeaponTrue(GameObject weaponPrefab)
    {
        anim.SetBool("HasWeapon", true);
        // Instantiate the weapon prefab at the calculated spawn position
        weapon = Instantiate(weaponPrefab);
        weapon.GetComponent<NetworkObject>().Spawn();
        weapon.transform.SetParent(transform, false);


        weaponPose.data.constrainedObject = weapon.transform;
          rightclickAiming.data.constrainedObject = weapon.transform;
        TargetAiming.data.constrainedObject = weapon.transform;
      
        // Set the weapon's position and rotation

        // Update TwoBoneIK targets
        lefthandIK.data.target = weapon.transform.Find("leftgrip").transform;
        righthandIK.data.target = weapon.transform.Find("rightgrip").transform;
        //rb.enabled = true;

        // Rebuild the Rigidbody
        rb.Build();


    }
    public void SetHasWeapon(bool value)
    {
        hasWeapon = value;
        OnHasWeaponChanged?.Invoke(hasWeapon);
    }
   

 }