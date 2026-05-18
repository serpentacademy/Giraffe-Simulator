using UnityEngine;
using System.Collections;

public class EcosystemTree : MonoBehaviour
{
    private bool isEdible = true;
    private Collider treeCollider;

    void Awake()
    {
        // Grab the physical collider on the tree so we can turn it on and off
        treeCollider = GetComponent<Collider>();
    }

    // The Giraffe calls this function when it tries to eat
    public bool Consume()
    {
        // If it's just a seed waiting to grow, the giraffe can't eat it!
        if (!isEdible) return false;
        
        Destroy(gameObject);
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

        // 4. Smoothly animate the tree growing to full size over 5 seconds!
        float growTime = 5f;
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