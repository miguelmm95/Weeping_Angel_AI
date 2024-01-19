using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool CanMove { get; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && controller.isGrounded;
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && controller.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canUseHeadbob = true;

    [Header("Controls (tmp)")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Movement Param.")]                                                 // Basic parameters for the movement
    [SerializeField] private float walklSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;

    [Header("Crouch Param.")]
    [SerializeField] private float crouchHeight = 0.5f;                             // Crough height
    [SerializeField] private float standHeight = 2.0f;                              // Stand height
    [SerializeField] private float timeToCrouch = 0.25f;                            // Time to crouch
    [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);           // Standing center point
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);     // Crouching center point

    private bool isCrouching;               // Is crouching
    private bool duringCrouchAnimation;     // Is in the middle of the crouch animation

    [Header("Jumping Param.")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 13.0f;


    [Header("cameraLook Param.")]                                               // Paramemeters for the camera
    [SerializeField] private float mouseSensitivity = 3.5f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 90.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 90.0f;

    [Header("Smooth Param.")]                                                   // Parameters to smooth the player and camera movement. Works like the Lerp function
    [SerializeField] [Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField] [Range(0.0f, 0.5f)] float cameraSmoothTime = 0.03f;

    [Header("Headbob Param.")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    //
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    //
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;

    private float defaultYPos = 0;
    private float timer;

    private Camera playerCamera;
    private CharacterController controller;

    private float cameraPith = 0.0f;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;

    private Vector2 currentCameraDelta = Vector2.zero;
    private Vector2 currentCameraDeltaVelocity = Vector2.zero;

    void Awake()
    {
        // Get the camera and the controller
        playerCamera = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();

        // Lock the cursor in the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Save the start position of the camera to reset correctly when the headbob stops
        defaultYPos = playerCamera.transform.localPosition.y;
    }


    void Update()
    {

        if (CanMove)
        {
            UpdateMovement();
            UdpateCameraLook();

            if (canJump)
            {
                Jump();
            }

            if (canCrouch)
            {
                Crouch();
            }

            if (canUseHeadbob)
            {
                Headbob();
            }
        }
    }

    void UdpateCameraLook()
    {
        Vector2 targetCameraDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentCameraDelta = Vector2.SmoothDamp(currentCameraDelta, targetCameraDelta, ref currentCameraDeltaVelocity, cameraSmoothTime);

        cameraPith -= currentCameraDelta.y * mouseSensitivity;
        cameraPith = Mathf.Clamp(cameraPith, -upperLookLimit, lowerLookLimit);

        playerCamera.transform.localEulerAngles = Vector3.right * cameraPith;

        transform.Rotate(Vector3.up * currentCameraDelta.x * mouseSensitivity);
    }

    void UpdateMovement()
    {
        currentInput = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
        currentInput.Normalize();      // Fuck this 1.4f in particular

        currentDir = Vector2.SmoothDamp(currentDir, currentInput, ref currentDirVelocity, moveSmoothTime);

        float moveDirectioY = moveDirection.y;
        moveDirection = ((transform.TransformDirection(Vector3.forward) * currentDir.x) + (transform.TransformDirection(Vector3.right) * currentDir.y)) * (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walklSpeed) + Vector3.up * currentDir.y;
        moveDirection.y = moveDirectioY;

        if (!controller.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }

    void Jump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    void Headbob()
    {
        if (!controller.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    void Crouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standHeight : crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        while(timeElapsed < timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        isCrouching = !isCrouching;         // Toggl the isCrouching variable

        duringCrouchAnimation = false;
    }
}