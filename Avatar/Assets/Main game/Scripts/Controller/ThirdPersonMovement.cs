using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ThirdPersonMovement : NetworkBehaviour
{
    public CharacterController controller;
    public float movementSpeed = 0.5f;
    Vector3 sphere;
    float grounddist;
    private const float _threshold = 0.01f;
    public bool LockCameraPosition = false;
    [SerializeField] LayerMask groundMask;
    [SerializeField] private float cameraAngleOffset = 0.0f;
    Vector3 velocity;
    float grav = -1f;
    private Vector2 movement;
    public Vector3 direction;
    MovementBase currentState;
    public Idle basestate = new Idle();
    public Walking walkstate = new Walking();
    public Animator anim;
    float horizontalinput, verticalinput;
    private Transform mainCam;
    public GameObject cinemachineCamera;
    public float TopClamp = 70.0f;


    public float BottomClamp = -30.0f;
    private float cinemachineTargetYaw;//Rotation on Y axis;
    private float cinemachineTargetPitch;//Rotation on X axis;
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
                return  true;
#else
            return false;
#endif
        }
    }

    // Start is called before the first frame update
    private void Awake()
    {
        mainCam = Camera.main.transform;
    }
    void Start()
    {
        cinemachineTargetYaw = cinemachineCamera.transform.rotation.eulerAngles.y;
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        SwitchState(basestate);

    }

    // Update is called once per frame

    void FixedUpdate()
    {
        Gravity();
        directionFunction();
        anim.SetFloat("hzinput", horizontalinput);
        anim.SetFloat("vinput", verticalinput);
        currentState.UpdateState(this);
    }
    void LateUpdate()
    {

    }
    public void directionFunction()
    {
        horizontalinput = Input.GetAxisRaw("Horizontal");
        verticalinput = Input.GetAxisRaw("Vertical");

        //Times rotation of quaternioneular by tarrget rotation * Vector3.foward
        //target rotation use Atan for angle of rotation  on the x and z axis
        //target rotation is direction rotatiuon of camera position

        //  direction = new Vector3(movement.x, 0, movement.y).normalized;

        // Vector3 direction = new Vector3(movement.x, 0, movement.y);
        // movement = new Vector2(horizontalinput,verticalinput);
        direction = transform.forward * verticalinput + transform.right * horizontalinput;
        controller.Move(direction.normalized * movementSpeed * Time.deltaTime);
    }
    private void camRotation()
    {
        movement = new Vector2(horizontalinput, verticalinput);
        if (movement.sqrMagnitude >= _threshold && !LockCameraPosition)
        {

            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += movement.x * deltaTimeMultiplier;
            cinemachineTargetPitch += movement.y * deltaTimeMultiplier;
        }
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        cinemachineCamera.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOffset,
            cinemachineTargetYaw, 0.0f);
    }
    public void SwitchState(MovementBase state)
    {
        currentState = state;
        currentState.EnterState(this);

    }
    bool IsGrounded()
    {
        sphere = new Vector3(transform.position.x, transform.position.y - grounddist, transform.position.z);
        if (Physics.CheckSphere(sphere, controller.radius - 0.05f, groundMask)) { return true; }
        return false;
    }
    void Gravity()
    {
        if (!IsGrounded())
        {
            velocity.y += grav * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2;
        }

        controller.Move(velocity * Time.deltaTime);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
