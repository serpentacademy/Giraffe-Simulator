using UnityEngine;
using UnityEngine.InputSystem; // 1. Added the New Input System namespace

public class PandaThirdPersonController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public Animator animator;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // GRAVITY VARIABLES
    public float gravity = -9.81f;
    Vector3 velocity;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    void Update()
    {
        // 1. GRAVITY CHECK
        isGrounded = controller.isGrounded; 

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // 2. MOVEMENT INPUT (Updated for New Input System)
        float horizontal = 0f;
        float vertical = 0f;
        bool isRunning = false;

        // Ensure a keyboard is connected before polling it
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal = -1f;
            
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical = -1f;

            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 3. MOVE & ROTATE
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // 4. APPLY GRAVITY
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 5. ANIMATION
        float inputMagnitude = direction.magnitude;
        if (isRunning) { inputMagnitude *= 1f; } // Run
        else { inputMagnitude *= 0.5f; } // Walk
        
        if (direction.magnitude < 0.1f) inputMagnitude = 0f;

        animator.SetFloat("Speed", inputMagnitude, 0.1f, Time.deltaTime);
    }
}