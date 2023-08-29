    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class ToggleLoopButton : MonoBehaviour
    {
        public FollowWaypoint followWaypoint;
        public Button toggleButton;
        public TMP_Text toggleButtonText;
        private bool isLooping = false;
        private bool carSpawned = false;

        private void Start()
        {
            // Disable the toggle button initially
            toggleButton.interactable = false;
            toggleButtonText.text = "Loop: Off";

            // Attach the button click event listener
            toggleButton.onClick.AddListener(ToggleLoop);
        }

        private void OnEnable()
        {
            ModelSpawner.OnVehicleSpawned += HandleCarSpawned;
        }

        private void OnDisable()
        {
            ModelSpawner.OnVehicleSpawned -= HandleCarSpawned;
        }

        private void HandleCarSpawned(bool value)
        {
            carSpawned = value;
            Debug.Log("Car has spawned: " + carSpawned);

            if (carSpawned)
            {
                GameObject car = GameObject.FindGameObjectWithTag("Car");
                if (car != null)
                {
                    followWaypoint = car.GetComponent<FollowWaypoint>();
                    if (followWaypoint == null)
                    {
                        Debug.LogWarning("FollowWaypoint component not found on the car!");
                    }
                }
                else
                {
                    Debug.LogWarning("Car object not found in the scene with the 'Car' tag!");
                }

                // Enable the toggle button once the car is spawned
                toggleButton.interactable = true;
            }
        }

        private void Update()
        {
            // Update the button text based on the looping state
            toggleButtonText.text = isLooping ? "Loop: On" : "Loop: Off";
        }

        public void ToggleLoop()
        {
            // Invert the looping state
            isLooping = !isLooping;

            // Pass the looping state to the FollowWaypoint script
            if (followWaypoint != null)
            {
                followWaypoint.ToggleLoop();
            }
            else
            {
                Debug.LogWarning("FollowWaypoint component not found!");
            }
        }
    }
