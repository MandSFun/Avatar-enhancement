using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using StarterAssets;
using TMPro;
using UnityEngine.Animations.Rigging;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 

    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Player")]
        [SerializeField] private float moveSpeed = 2.0f;

        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float RotationSpeed = 0.5f;

        [Range(0.0f, 0.3f)]
        [SerializeField] private float RotationSmoothTime = 0.1f;
        [SerializeField] private float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]

        public float JumpHeight = 1.2f;


        public float Gravity = -15.0f;

        [Space(10)]

        public float JumpTimeout = 0.50f;


        public float FallTimeout = 0.15f;
        private string playerName;

        [Header("Player Grounded")]

        public bool grounded = true;

        public float GroundedOffset = -0.14f;


        public float GroundedRadius = 0.28f;

        public LayerMask GroundLayers;


        [Header("Player Die")]
        private int animDying;
        private bool isDead = false;

        [SerializeField] private GameObject bloodGush;

        [SerializeField] private Transform bloodGushOrigin;

        [Header("Cinemachine")]

        CinemachineComponentBase componentBase;
        float camDist;
        float sensitivity = 8.0f;
        [SerializeField] float maxCameraDistance;
        Input scroll;
        public GameObject CinemachineCameraTarget;

        public float TopClamp = 70.0f;

        public float BottomClamp = -30.0f;

        public float CameraAngleOverride = 0.0f;


        public bool LockCameraPosition = false;

        // cinemachine
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;

        // Player
        private float speed;
        private float animationBlend;
        private float targetRotation = 0.0f;
        private float rotationVelocity;
        private float verticalVelocity;
        private float terminalVelocity = 53.0f;

        // Timeout deltatime
        private float jumpWait;
        private float fallTimeoutDelta;

        // animation IDs
        private int animSpeed;
        private int animGrounded;
        private int animJump;
        private int animFreefall;
        private int animMotionSPD;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput playerInput;
#endif
        private Animator anim;
        private CharacterController controller;
        private StarterAssetsInputs input;
        private Transform mainCamera;
        public CinemachineVirtualCamera ThirdPersonCam;
        public CinemachineVirtualCamera FirstPersonCam;
        public GameObject carCamera;

        private const float thresehold = 0.01f;
        private const float speedOffset = 0.1f;
        private bool hasAnim;
        private bool firstpersonstatus = false;
        private RigBuilder rb;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            //Reference main cam;
            mainCamera = Camera.main.transform;
        }

        private void Start()
        {  // Debug.Log(NetworkManager.Singleton.LocalClientId);

            //cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            if(!IsOwner)return;
            hasAnim = TryGetComponent(out anim);
            controller = GetComponent<CharacterController>();
            input = GetComponent<StarterAssetsInputs>();
            //playerNameText = GameObject.FindGameObjectWithTag("nop").GetComponentInChildren<TMP_Text>();
           

            AssignAnimationIDs();

            // reset our timeouts on start
            jumpWait = JumpTimeout;
            fallTimeoutDelta = FallTimeout;
            transform.position = new Vector3(0, 0, 0);
            if (IsOwner && IsClient)
            {
                //bloodGush.gameObject.SetActive(false);

                if (ThirdPersonCam == null && FirstPersonCam == null)
                {
                    ThirdPersonCam = GameObject.FindGameObjectWithTag("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
                     ThirdPersonCam.Follow = transform.GetChild(0).transform;
                
                    componentBase = ThirdPersonCam.GetCinemachineComponent(CinemachineCore.Stage.Body);
                    FirstPersonCam = GameObject.FindGameObjectWithTag("FirstPersonCamera").GetComponent<CinemachineVirtualCamera>();
                          FirstPersonCam.Follow = transform.GetChild(0).transform;
                          FirstPersonCam.gameObject.SetActive(false);
                }


                if (FirstPersonCam == null)
                {
                    Debug.Log(GameObject.FindGameObjectWithTag("FirstPersonCamera"));

                }

               
            }


        }

        private void Update()
        {
            if (IsOwner)
            {
                //hasAnim = TryGetComponent(out anim);
                if (!isDead)
                {
                    GroundedCheck();
                    if (PlayerInteractable.Instance.isAnimationPlaying  == false)
                    {
                        JumpAndGravity();
                        Move();
                    }
                    
                    if (firstpersonstatus == false)
                    {
                        Scroll();
                    }
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        if (ThirdPersonCam.gameObject.activeSelf)
                        {

                            FirstPersonCam.gameObject.SetActive(true);
                            ThirdPersonCam.gameObject.SetActive(false);
                            firstpersonstatus = true;
                        
                            Cursor.lockState = CursorLockMode.Locked;
                        }
                        else
                        {
                            FirstPersonCam.gameObject.SetActive(false);
                            ThirdPersonCam.gameObject.SetActive(true);
                            firstpersonstatus = false;
                            Cursor.lockState = CursorLockMode.None;
                        }


                    }
                }
                else
                {
                    anim.SetBool(animDying, true);
                    bloodGush.gameObject.SetActive(true);
                    bloodGush.transform.position = bloodGushOrigin.position;

                }

            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //isClient checks if current instance is client,IsOwner checks if client owns the object,
            //ensure playerinput is only enabled for client instance;



            if (IsOwner)
            {


                playerInput = GetComponent<PlayerInput>();
                playerInput.enabled = true;
                //FirstPersonCam.gameObject.SetActive(false);

            }
        }
        [ServerRpc]
        private void TestServerRpc()
        {
            Debug.Log("TEst server rpc" + OwnerClientId);
        }
        private void AssignAnimationIDs()
        {   //Take parameters in animators and sets to int in script;
            animSpeed = Animator.StringToHash("Speed");
            animGrounded = Animator.StringToHash("Grounded");
            animJump = Animator.StringToHash("Jump");
            animFreefall = Animator.StringToHash("FreeFall");
            animMotionSPD = Animator.StringToHash("MotionSpeed");
            animDying = Animator.StringToHash("Dying");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            anim.SetBool(animGrounded, grounded);

        }

        private void CameraRotation()
        {
            if (firstpersonstatus)
            {
                if (input.look.sqrMagnitude >= thresehold)
                {
                    //Don't multiply mouse input by Time.deltaTime
                    float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                    cinemachineTargetPitch += input.look.y * RotationSpeed * deltaTimeMultiplier;
                    rotationVelocity = input.look.x * RotationSpeed * deltaTimeMultiplier;

                    // clamp our pitch rotation
                    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

                    // Update Cinemachine camera target pitch
                    CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

                    // rotate the player left and right
                    transform.Rotate(Vector3.up * rotationVelocity);
                }
            }
            else
            {
                // if there is an input and camera position is not fixed
                if (input.look.sqrMagnitude >= thresehold && !LockCameraPosition)
                {
                    //Don't multiply mouse input by Time.deltaTime;
                    float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                    cinemachineTargetYaw += input.look.x * deltaTimeMultiplier;
                    cinemachineTargetPitch += input.look.y * deltaTimeMultiplier;
                }

                // clamp our rotations so our values are limited 360 degrees
                cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
                cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

                // Cinemachine will follow this target
                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride,
                    cinemachineTargetYaw, 0.0f);
            }
        }

        private void Scroll()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                camDist = scroll * sensitivity;
                if (componentBase is Cinemachine3rdPersonFollow)
                {

                    if (camDist < 0)
                    {
                        (componentBase as Cinemachine3rdPersonFollow).CameraDistance -= camDist;
                        if ((componentBase as Cinemachine3rdPersonFollow).CameraDistance > maxCameraDistance)
                        {
                            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = maxCameraDistance;
                        }
                    }
                    else
                    {
                        (componentBase as Cinemachine3rdPersonFollow).CameraDistance -= camDist;
                        if ((componentBase as Cinemachine3rdPersonFollow).CameraDistance <= 1)
                        {
                            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = 1;
                        }
                    }
                }
            }

        }
        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = input.sprint ? runSpeed : moveSpeed;
            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // If there is no input, set the target speed to 0
            if (input.move == Vector2.zero) targetSpeed = 0.0f;

            //Gets length of vector and sets as speed;
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            //Analog or digital tenerary;
            float inputMagnitude = input.analogMovement ? input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (animationBlend < 0.01f) animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving

            if (firstpersonstatus)
            {
                inputDirection = transform.right * input.move.x + transform.forward * input.move.y;
                controller.Move(inputDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            }
            else
            {
                if (input.move != Vector2.zero)
                {
                    targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                      mainCamera.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }


                Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

                // move the player
                controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                                 new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            }
            // update animator if using character
            if (hasAnim)
            {
                anim.SetFloat(animSpeed, animationBlend);
                anim.SetFloat(animMotionSPD, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (hasAnim)
                {
                    anim.SetBool(animJump, false);
                    anim.SetBool(animFreefall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                // Jump
                if (input.jump && jumpWait <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (hasAnim)
                    {
                        anim.SetBool(animJump, true);
                    }
                }

                // jump timeout
                if (jumpWait >= 0.0f)
                {
                    jumpWait -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                jumpWait = JumpTimeout;

                // fall timeout
                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (hasAnim)
                    {
                        anim.SetBool(animFreefall, true);
                    }
                }

                // if we are not grounded, do not jump
                input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity < terminalVelocity)
            {
                verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayerDie"))
            {
                isDead = true;


            }
        }
    }
}