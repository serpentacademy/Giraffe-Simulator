using UnityEngine;
using System.Collections;

public class EcosystemTree : MonoBehaviour
{
    private bool isEdible = true;
    private Collider treeCollider;

    [Header("Lifecycle")]
    private float lifeTimer;
    
    [Header("Health")]
    private int maxBites = 5;
    private int currentBites = 0;

    void Awake()
    {
        // Grab the physical collider on the tree so we can turn it on and off
        treeCollider = GetComponent<Collider>();
        
        // Assign a random lifespan between 3,000 and 7,000 seconds the moment it exists
        lifeTimer = Random.Range(3000f, 7000f);
    }

    void Update()
    {
        // Only age the tree if it is a fully grown, interactive plant
        if (isEdible)
        {
            lifeTimer -= Time.deltaTime;
            
            // If the tree reaches the end of its life, it naturally dies
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    // The Giraffe calls this function when it tries to eat
    public bool Consume()
    {
        // If it's just a seed waiting to grow, the giraffe can't eat it!
        if (!isEdible) return false;
        
        // Register the bite
        currentBites++;

        // Visual Feedback: Shrink the tree a tiny bit so you know it's being eaten!
        transform.localScale *= 0.9f; 

        // If it has been eaten 5 times, it is destroyed
        if (currentBites >= maxBites)
        {
            Destroy(gameObject);
        }

        return true;
    }

    // The Giraffe calls this function right when the tree is spawned
    public void PlantAsSeed(float delayTime)
    {
        StartCoroutine(GrowthRoutine(delayTime));
    }

    IEnumerator GrowthRoutine(float delayTime)
    {
        // 1. Instantly become a tiny, inedible seed
        isEdible = false;
        if (treeCollider != null) treeCollider.enabled = false;
        transform.localScale = Vector3.one * 0.2f; // 1/5th size

        // 2. Wait the requested 70-130 seconds
        yield return new WaitForSeconds(delayTime);

        // 3. Turn the physics back on so giraffes can see it again
        if (treeCollider != null) treeCollider.enabled = true;
        isEdible = true;

        // 4. Smoothly animate the tree growing to full size over 300 seconds!
        float growTime = 300f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.one;

        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / growTime);
            yield return null;
        }

        // Ensure it ends up perfectly at normal size
        transform.localScale = targetScale; 
    }
}