using UnityEngine;

public class EcosystemTree : MonoBehaviour
{
    [Header("Tree Stats")]
    public int timesEaten = 0;
    public int maxEats = 5;
    public float age = 0f;
    public float lifespan = 220f;
    
    [Header("Growth Logic")]
    public bool isFullyGrown = true;
    private float growTimer = 0f;

    void Start()
    {
        // If spawned small (1/5th size), it must grow before being eaten
        if (transform.localScale.y < 1f)
        {
            isFullyGrown = false;
        }
    }

    void Update()
    {
        // 1. Aging and Death
        age += Time.deltaTime;
        if (age >= lifespan)
        {
            Destroy(gameObject);
        }

        // 2. Growth
        if (!isFullyGrown)
        {
            growTimer += Time.deltaTime;
            // Grows every 25 seconds
            if (growTimer >= 25f) 
            {
                growTimer = 0f;
                transform.localScale += Vector3.one * 0.2f; // Grow by 1/5th
                
                if (transform.localScale.y >= 1f)
                {
                    transform.localScale = Vector3.one;
                    isFullyGrown = true;
                }
            }
        }
    }

    // Called by the Giraffe when it takes a bite
    public bool Consume()
    {
        if (!isFullyGrown) return false; // Can't eat baby trees!

        timesEaten++;
        if (timesEaten >= maxEats)
        {
            Destroy(gameObject);
        }
        return true; 
    }
}