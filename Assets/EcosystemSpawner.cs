using UnityEngine;

public class EcosystemSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject treePrefab; // Drag your Oak_Tree here!
    public int numberOfTrees = 10;
    
    [Header("Map Boundaries")]
    [Tooltip("How far away from the edges of the terrain should trees spawn?")]
    public float edgeBuffer = 10f;

    [Header("Avoidance Tags")]
    [Tooltip("The script will not spawn trees on objects with these tags.")]
    public string waterTag = "Water";
    public string wallTag = "Wall"; // NEW: Variable to identify your Map Boundaries

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        if (Terrain.activeTerrain == null)
        {
            Debug.LogError("No Terrain found! Can't spawn trees.");
            return;
        }

        TerrainData tData = Terrain.activeTerrain.terrainData;
        Vector3 tPos = Terrain.activeTerrain.transform.position;

        int treesSpawned = 0;
        int maxAttempts = 100; // Prevents an infinite loop if it can't find a spot

        for (int i = 0; i < numberOfTrees; i++)
        {
            bool spawned = false;
            int attempts = 0;

            // Keep trying to find a spot until we find a safe one (or run out of attempts)
            while (!spawned && attempts < maxAttempts)
            {
                attempts++;

                // 1. Pick a random X and Z mathematically inside the terrain
                float randomX = Random.Range(tPos.x + edgeBuffer, tPos.x + tData.size.x - edgeBuffer);
                float randomZ = Random.Range(tPos.z + edgeBuffer, tPos.z + tData.size.z - edgeBuffer);

                // 2. Find the exact height of the dirt at those coordinates
                float heightY = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + tPos.y;
                Vector3 spawnPosition = new Vector3(randomX, heightY, randomZ);

                // 3. Check if this spot is safe (not inside the lake AND not inside the walls)
                if (IsPositionSafe(spawnPosition))
                {
                    // 4. Safe! Spawn the tree
                    GameObject newTree = Instantiate(treePrefab, spawnPosition, Quaternion.identity);
                    
                    // Give it a random rotation so the forest looks natural
                    newTree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    
                    newTree.transform.SetParent(this.transform);

                    treesSpawned++;
                    spawned = true;
                }
            }
        }

        Debug.Log("Successfully spawned " + treesSpawned + " trees!");
    }

    bool IsPositionSafe(Vector3 pos)
    {
        // Draw an invisible 3-meter sphere to check for overlaps
        Collider[] colliders = Physics.OverlapSphere(pos, 3f); 
        
        foreach (Collider col in colliders)
        {
            // NEW: Now it checks for BOTH the Water tag and the Wall tag!
            if (col.CompareTag(waterTag) || col.CompareTag(wallTag))
            {
                // We hit a restricted area! This spot is NOT safe.
                return false; 
            }
        }
        
        // Spot is safe!
        return true;
    }
}