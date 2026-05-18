using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject giraffePrefab; 
    public int initialGiraffes = 7;
    
    [Header("Map Boundaries")]
    [Tooltip("How far away from the edges/walls should they spawn?")]
    public float edgeBuffer = 10f;

    [Header("Avoidance Tags")]
    public string waterTag = "Water";
    public string wallTag = "Wall";

    void Start()
    {
        SpawnGiraffes();
    }

    void SpawnGiraffes()
    {
        if (Terrain.activeTerrain == null)
        {
            Debug.LogError("No Terrain found! Can't spawn animals.");
            return;
        }

        TerrainData tData = Terrain.activeTerrain.terrainData;
        Vector3 tPos = Terrain.activeTerrain.transform.position;

        int spawnedCount = 0;
        int maxAttempts = 100; 

        for (int i = 0; i < initialGiraffes; i++)
        {
            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxAttempts)
            {
                attempts++;

                // 1. Pick a random X and Z mathematically inside the terrain
                float randomX = Random.Range(tPos.x + edgeBuffer, tPos.x + tData.size.x - edgeBuffer);
                float randomZ = Random.Range(tPos.z + edgeBuffer, tPos.z + tData.size.z - edgeBuffer);

                // 2. Find the exact height of the dirt at those coordinates
                float heightY = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + tPos.y;
                
                // 3. We add +2 to the Y height so the giraffe drops in slightly above the ground, 
                // preventing its legs from getting stuck in the dirt on frame 1!
                Vector3 spawnPosition = new Vector3(randomX, heightY + 2f, randomZ);

                // 4. Check if this spot is safe
                if (IsPositionSafe(spawnPosition))
                {
                    GameObject newGiraffe = Instantiate(giraffePrefab, spawnPosition, Quaternion.identity);
                    
                    // Give them a random starting rotation so they don't all face the same way
                    newGiraffe.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    
                    newGiraffe.transform.SetParent(this.transform);

                    spawnedCount++;
                    spawned = true;
                }
            }
        }

        Debug.Log("Successfully spawned " + spawnedCount + " giraffes to begin training!");
    }

    bool IsPositionSafe(Vector3 pos)
    {
        // Draw an invisible 3-meter sphere to check for walls or water
        Collider[] colliders = Physics.OverlapSphere(pos, 3f); 
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(waterTag) || col.CompareTag(wallTag))
            {
                return false; 
            }
        }
        
        return true;
    }
}