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
    [SerializeField] public bool isAnimationPlaying = false;
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
    public bool hasWeapon;

    [SerializeField] private TwoBoneIKConstraint lefthandIK;
    [SerializeField] private TwoBoneIKConstraint righthandIK;
    [SerializeField] private MultiParentConstraint weaponPose;
    [SerializeField] private MultiParentConstraint TargetAiming;
    [SerializeField] private MultiParentConstraint rightclickAiming;

    private StarterAssets.ThirdPersonController playerController;
    public delegate void HasWeaponChanged(bool value);
    public static event HasWeaponChanged OnHasWeaponChanged;

    [SerializeField] private Item itemAK47;


    private void Start()
    {
        weaponPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs["AK47"] = AK47prefab;
        anim = GetComponent<Animator>();
        AssignAnimationIDs();
        rb = GetComponent<RigBuilder>();
        playerController = GetComponent<StarterAssets.ThirdPersonController>();



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
                    if (objectInteraction != null && InventoryManager.instance.FindItemInSlot(objectInteraction.GetWeaponIdentifier()) == false) //add condition to check if weapon does not already exist in inventory
                    {
                        // Face the object
                        Vector3 lookDirection = collider.transform.position - transform.position;
                        lookDirection.y = 0f;
                        transform.rotation = Quaternion.LookRotation(lookDirection);

                        // Trigger the object's interaction

                        playerController.enabled = false;
                        // Get the identifier of the weapon
                        string weaponIdentifier = objectInteraction.GetWeaponIdentifier();

                        // Trigger the pickup animation with the weapon identifier
                        TriggerPickupAnimation(collider.transform.position, weaponIdentifier, true);
                        PickableItemScript.instance.hasItem = true;
                        //AddItemToInventory(weaponIdentifier);
                        Debug.Log("Playing");
                        break;
                    }
                }
            }
        }


        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0f &&
            (anim.GetCurrentAnimatorStateInfo(0).IsName("Throw") || anim.GetCurrentAnimatorStateInfo(0).IsName("Pickup")))
        {
            isAnimationPlaying = true;
        }
        else
        {
            isAnimationPlaying = false;
        }

    }

    private void AddItemToInventory(string weaponIdentifier)
    {
        int playerID;
        try
        {
            playerID = LoginController.instance.verifiedUsername.GetHashCode();
        }
        catch (System.Exception e)
        {
            playerID = NetworkManagerUI.instance.localPlayerID;
            Debug.Log("Unable to get playerID from SQL Server. Using default playerID from local username: " + playerID);
        }
        if (weaponIdentifier == "AK47")
        {
            InventoryManager.instance.AddItem(itemAK47, true, playerID);

        }

    }

    public void SetCurrentInteractable(Transform interactable)
    {
        currentInteractable = interactable;
    }

    public void TriggerPickupAnimation(Vector3 objectPosition, string weaponIdentifier, bool playAnimation)
    {

        // Trigger the pickup animation
        if (playAnimation)
        {
            // Face the object
            Vector3 lookDirection = objectPosition - transform.position;
            lookDirection.y = 0f;
            transform.rotation = Quaternion.LookRotation(lookDirection);
            anim.SetTrigger(animPickup);
        }



        // Check if the weapon identifier exists in the dictionary
        if (weaponPrefabs.ContainsKey(weaponIdentifier))
        {
            // Get the corresponding weapon prefab
            GameObject weaponPrefab = weaponPrefabs[weaponIdentifier];
            rb.enabled = true;
            // Call the SpawnWeapon method on the next frame with the correct prefab
            StartCoroutine(DelayedSpawnWeapon(weaponPrefab, playAnimation, weaponIdentifier));
        }
        else
        {
            Debug.LogWarning("No prefab found for weapon identifier: " + weaponIdentifier);
        }
    }

    private IEnumerator DelayedSpawnWeapon(GameObject weaponPrefab, bool playAnimation, string weaponIdentifier)
    {
        //Delay before instantiating the weapon into the hand;
        if (playAnimation)
        {
            yield return new WaitForSeconds(delayBeforeSpawn);
            PickableItemScript.instance.hasItem = true;
            AddItemToInventory(weaponIdentifier);
        }


        // Spawn the weapon
        if (weaponPrefab != null)
        {

            SetHasWeaponTrue(weaponPrefab);
            SetHasWeapon(true);




        }
        playerController.enabled = true;
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

    public void SetHasWeaponFalse()
    {
        anim.SetBool("HasWeapon", false);

        // Remove the parent-child relationship between the character and the weapon
        if (weapon != null)
        {
            // weapon.transform.SetParent(null);
            weapon.transform.SetParent(null);

            // Reset the TwoBoneIK targets
            lefthandIK.data.target = null;
            righthandIK.data.target = null;
            weaponPose.data.constrainedObject = null;
            rightclickAiming.data.constrainedObject = null;
            TargetAiming.data.constrainedObject = null;

            // Destroy the weapon object
            Destroy(weapon);

            // Rebuild the Rigidbody
            try
            {
                rb.Build();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }



        }

    }
    public void SetHasWeapon(bool value)
    {
        hasWeapon = value;
        OnHasWeaponChanged?.Invoke(hasWeapon);
    }


}