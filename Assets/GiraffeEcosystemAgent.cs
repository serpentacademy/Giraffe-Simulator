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
// ---------------- AI MOVEMENT ---------------- //
    
    // NEW: We give the AI awareness of its own internal body timers!
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

    // Allows you to test manually with WASD before training the AI
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    // ---------------- ECOSYSTEM INTERACTIONS ---------------- //
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            thirstTimer = 120f;
            AddReward(1f); // Good job, AI!
        }
        else if (other.CompareTag("Tree"))
        {
            EcosystemTree tree = other.GetComponent<EcosystemTree>();
            if (tree != null && tree.Consume())
            {
                hungerTimer = 100f;
                eatCount++;
                AddReward(1f); // Good job, AI!

                // Reproduction check
                if (eatCount >= 5)
                {
                    eatCount = 0;
                    // Spawn new giraffe 2 meters to the right
                    Instantiate(giraffePrefab, transform.position + transform.right * 2f, Quaternion.identity);
                    AddReward(2f); // Massive reward for reproducing!
                }

                // Seed dropping check (33% chance)
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