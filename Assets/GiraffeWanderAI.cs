using UnityEngine;

public class GiraffeWanderAI : MonoBehaviour
{
    private Animator animator;

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float turnSpeed = 2f;

    [Header("Position Correction")]
    public float heightOffset = 2.0f;

    [Header("Interactions")]
    public GameObject waterEmojiCanvas; // Slot for your floating Canvas
    private bool isDrinking = false;    // Stops the AI from wandering while drinking

    private int currentState = 0;
    private float actionTimer = 0f;
    private Vector3 targetDirection;

    void Start()
    {
        animator = GetComponent<Animator>();
        ChooseNextAction();
    }

    void Update()
    {
        // If the giraffe is currently drinking, ignore the wander timers
        if (isDrinking) return;

        actionTimer -= Time.deltaTime;

        if (actionTimer <= 0)
        {
            ChooseNextAction();
        }

        if (currentState == 1) // Walk
        {
            MoveGiraffe(walkSpeed);
            animator.SetFloat("Speed", 1f);
        }
        else if (currentState == 2) // Run
        {
            MoveGiraffe(runSpeed);
            animator.SetFloat("Speed", 2f);
        }
        else // Idle
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void MoveGiraffe(float speed)
    {
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        Vector3 newPosition = transform.position + (transform.forward * speed * Time.deltaTime);

        if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(newPosition);
            newPosition.y = terrainHeight + Terrain.activeTerrain.transform.position.y + heightOffset; 
        }

        transform.position = newPosition;
    }

    void ChooseNextAction()
    {
        float randomAction = Random.Range(0f, 100f);

        if (randomAction < 40f) 
        {
            currentState = 0; // Idle
            actionTimer = Random.Range(3f, 8f);
        }
        else if (randomAction < 85f) 
        {
            currentState = 1; // Walk
            actionTimer = Random.Range(5f, 15f);
        }
        else 
        {
            currentState = 2; // Run
            actionTimer = Random.Range(2f, 4f);
        }

        float randomAngle = Random.Range(0f, 360f);
        targetDirection = new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle));
    }

    // --- NEW DRINKING LOGIC --- //

void OnTriggerEnter(Collider other)
    {
        // If we bump into the lake trigger and aren't already drinking
        if (other.CompareTag("Water") && !isDrinking)
        {
            // NEW: Print a message to the Unity Console!
            Debug.Log("The Giraffe has entered the water sphere and is starting to drink!");

            StartDrinking();
        }
    }

    void StartDrinking()
    {
        isDrinking = true;
        currentState = 0; // Force Idle state
        animator.SetFloat("Speed", 0f); // Stop animation movement

        // Show the 💧 emoji
        if (waterEmojiCanvas != null)
        {
            waterEmojiCanvas.SetActive(true);
        }

        // Rest/Drink for 6 seconds, then call StopDrinking()
        Invoke("StopDrinking", 6f);
    }

    void StopDrinking()
    {
        isDrinking = false;

        // Hide the 💧 emoji
        if (waterEmojiCanvas != null)
        {
            waterEmojiCanvas.SetActive(false);
        }

        // Turn around and walk away from the water!
        targetDirection = -transform.forward; 
        currentState = 1; 
        actionTimer = 5f; 
    }
}