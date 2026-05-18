using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class GiraffeEcosystemAgent : Agent
{
    [Header("Prefabs")]
    public GameObject giraffePrefab;
    public GameObject treePrefab;
    
    [Header("Position Correction")]
    public float heightOffset = 2.5f; // Adjust this in the Inspector!

    [Header("Survival Stats")]
    public float thirstTimer = 120f;
    public float hungerTimer = 100f;
    private int eatCount = 0;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    
    private Animator animator;

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Thirst and Hunger drain every second
        thirstTimer -= Time.deltaTime;
        hungerTimer -= Time.deltaTime;

        if (thirstTimer <= 0f || hungerTimer <= 0f)
        {
            // Neural Network Punishment for dying
            AddReward(-2f); 
            
            // The Giraffe dies and is removed from the ecosystem
            Destroy(gameObject); 
        }
    }

    // ---------------- AI MOVEMENT ---------------- //
    
    // We give the AI awareness of its own internal body timers!
    public override void CollectObservations(VectorSensor sensor)
    {
        // We divide by the max timers to keep the numbers between 0.0 and 1.0 (Neural Networks love this)
        sensor.AddObservation(thirstTimer / 120f); 
        sensor.AddObservation(hungerTimer / 100f); 
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveAmount = actions.ContinuousActions[0];
        float turnAmount = actions.ContinuousActions[1];

        transform.Translate(Vector3.forward * moveAmount * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnAmount * turnSpeed * Time.deltaTime);

        // Update Animation
        if (animator != null) {
            animator.SetFloat("Speed", Mathf.Abs(moveAmount) > 0.1f ? 1f : 0f);
        }

        // MAGIC TRICK: Keep the AI glued to the floor with our custom offset!
        if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
            Vector3 fixedPos = transform.position;
            
            // Set the Y position to the terrain height PLUS our custom lift offset
            fixedPos.y = terrainHeight + Terrain.activeTerrain.transform.position.y + heightOffset;
            
            transform.position = fixedPos;
        }

        // FIXED: We check to make sure MaxStep isn't 0 before doing the math!
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
        else
        {
            // If MaxStep is 0, just apply a tiny generic penalty
            AddReward(-0.0005f); 
        }
    }

    // Allows you to test safely without crashing the new Unity Input System
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // By setting these to 0, we prevent Unity from looking for a keyboard layout it doesn't understand
        continuousActionsOut[0] = 0f; 
        continuousActionsOut[1] = 0f; 
    }

  // ---------------- ECOSYSTEM INTERACTIONS ---------------- //
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            thirstTimer = 120f;
            AddReward(1f); 
            
            // NEW: Tell the Global Manager we drank!
            if (EcosystemCounter.Instance != null) EcosystemCounter.Instance.RegisterDrink();
        }
        else if (other.CompareTag("Tree"))
        {
            EcosystemTree tree = other.GetComponent<EcosystemTree>();
            if (tree != null && tree.Consume())
            {
                hungerTimer = 100f;
                eatCount++;
                AddReward(1f); 

                // NEW: Tell the Global Manager we ate!
                if (EcosystemCounter.Instance != null) EcosystemCounter.Instance.RegisterEat();

                if (eatCount >= 5)
                {
                    eatCount = 0;
                    Instantiate(giraffePrefab, transform.position + transform.right * 2f, Quaternion.identity);
                    AddReward(2f); 
                }

                if (Random.Range(0, 3) == 0)
                {
                    StartCoroutine(PoopSeedRoutine());
                }
            }
        }
    }

    IEnumerator PoopSeedRoutine()
    {
        // Wait 40-60 seconds to poop
        float delay = Random.Range(40f, 60f);
        yield return new WaitForSeconds(delay);

        // Save the location where the poop dropped
        Vector3 poopPos = transform.position; 
        
        // Wait 20 seconds for the seed to take root
        yield return new WaitForSeconds(20f);

        // A new tree is born!
        GameObject newTree = Instantiate(treePrefab, poopPos, Quaternion.identity);
        newTree.transform.localScale = Vector3.one * 0.2f; // 1/5th size
    }
}