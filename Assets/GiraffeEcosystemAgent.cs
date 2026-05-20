using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GiraffeEcosystemAgent : Agent
{
    [Header("Prefabs")]
    public GameObject giraffePrefab;
    public GameObject treePrefab;
    
    [Header("Position Correction")]
    public float heightOffset = 2.5f; 

    [Header("Survival Stats")]
    public float thirstTimer = 400f;
    public float hungerTimer = 500f;
    
    // Changing this to PUBLIC so you can watch it count up in the Inspector!
    public int eatCount = 0; 
    
    [Header("Reproduction")]
    public float pregnancyLength = 20f;
    private int babiesPending = 0;
    private float pregnancyTimer = 0f;

    [Header("Aging")]
    public float ageTimer; 

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    
    private Animator animator;

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        ageTimer = Random.Range(1000f, 2000f);
    }

    void Update()
    {
        // Thirst, Hunger, and Age drain every second
        thirstTimer -= Time.deltaTime;
        hungerTimer -= Time.deltaTime;
        ageTimer -= Time.deltaTime;

        // --- THE BULLETPROOF PREGNANCY TIMER ---
        if (babiesPending > 0)
        {
            pregnancyTimer -= Time.deltaTime;
            if (pregnancyTimer <= 0f)
            {
                // 1. Give birth!
                Instantiate(giraffePrefab, transform.position + transform.right * 2f, Quaternion.identity);
                AddReward(2f); 
                
                // 2. Remove one baby from the queue
                babiesPending--;
                
                // 3. If there are MORE babies queued up, reset the timer!
                if (babiesPending > 0) pregnancyTimer = pregnancyLength;
            }
        }

        // 1. Did they successfully survive a full life?
        if (ageTimer <= 0f)
        {
            AddReward(5f); 
            Destroy(gameObject);
        }
        // 2. Did they die of starvation or dehydration?
        else if (thirstTimer <= 0f || hungerTimer <= 0f)
        {
            AddReward(-2f); 
            Destroy(gameObject); 
        }
    }

    // ---------------- AI MOVEMENT ---------------- //
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(thirstTimer / 400f); 
        sensor.AddObservation(hungerTimer / 500f); 
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveAmount = actions.ContinuousActions[0];
        float turnAmount = actions.ContinuousActions[1];

        transform.Translate(Vector3.forward * moveAmount * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnAmount * turnSpeed * Time.deltaTime);

        if (animator != null) {
            animator.SetFloat("Speed", Mathf.Abs(moveAmount) > 0.1f ? 1f : 0f);
        }

        if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
            Vector3 fixedPos = transform.position;
            fixedPos.y = terrainHeight + Terrain.activeTerrain.transform.position.y + heightOffset;
            transform.position = fixedPos;
        }

        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
        else
        {
            AddReward(-0.0005f); 
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 0f; 
        continuousActionsOut[1] = 0f; 
    }

    // ---------------- ECOSYSTEM INTERACTIONS ---------------- //
    
    void OnTriggerStay(Collider other)
    {
        // --- NEW: THE DROWNING ZONE ---
        if (other.CompareTag("DeepWater"))
        {
            AddReward(-2f); // Massive penalty for walking into the deep end!
            Destroy(gameObject); // The giraffe drowns and is removed from the simulation
            return; // Stop running the rest of the code
        }
        if (other.CompareTag("Water"))
        {
            if (thirstTimer <= (400f - 50f)) 
            {
                thirstTimer = 400f;
                AddReward(1f); 
                if (EcosystemCounter.Instance != null) EcosystemCounter.Instance.RegisterDrink();
            }
        }
        else if (other.CompareTag("Tree"))
        {
            if (hungerTimer <= (500f - 50f))
            {
                EcosystemTree tree = other.GetComponent<EcosystemTree>();
                
                if (tree != null && tree.Consume())
                {
                    hungerTimer = 500f;
                    eatCount++;
                    AddReward(1f); 

                    if (EcosystemCounter.Instance != null) EcosystemCounter.Instance.RegisterEat();

                    // --- THE REPRODUCTION QUEUE ---
                    if (eatCount >= 5)
                    {
                        eatCount = 0; // Reset counter for the next baby
                        babiesPending++; // Add a baby to the queue!
                        
                        // If this is the first baby in the queue, start the 20s timer
                        if (babiesPending == 1) pregnancyTimer = pregnancyLength; 
                    }

                    // --- NEW: SAFE SEED DROPPING ---
                    // 1/3 Chance to try and plant a new tree
                    if (Random.Range(0, 3) == 0)
                    {
                        // Calculate a spot slightly behind the giraffe
                        Vector3 seedDropPos = transform.position - (transform.forward * 2f);
                        
                        // Snap the seed to the actual terrain height so it doesn't float
                        if (Terrain.activeTerrain != null)
                        {
                            float heightY = Terrain.activeTerrain.SampleHeight(seedDropPos) + Terrain.activeTerrain.transform.position.y;
                            seedDropPos.y = heightY;
                        }

                        // Check the area using an invisible 2-meter sphere
                        bool isSpotSafe = true;
                        Collider[] colliders = Physics.OverlapSphere(seedDropPos, 2f);
                        
                        foreach (Collider col in colliders)
                        {
                            // If we detect another Tree, Water, or a Wall, abort the spawn!
                            if (col.CompareTag("Tree") || col.CompareTag("Water") || col.CompareTag("Wall"))
                            {
                                isSpotSafe = false;
                                break; 
                            }
                        }

                        // If the coast is clear, plant the seed!
                        if (isSpotSafe)
                        {
                            GameObject newTree = Instantiate(treePrefab, seedDropPos, Quaternion.identity);
                            EcosystemTree newTreeScript = newTree.GetComponent<EcosystemTree>();
                            if (newTreeScript != null)
                            {
                                newTreeScript.PlantAsSeed(Random.Range(70f, 130f));
                            }
                        }
                    }
                }
            }
        }
    }
}