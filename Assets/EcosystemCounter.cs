using UnityEngine;
using TMPro; 
using System.IO; 
using System;    
using UnityEngine.SceneManagement; // <-- ADD THIS LINE

public class EcosystemCounter : MonoBehaviour
{
    public static EcosystemCounter Instance;

    // --- NEW: THE SPEED SLIDER ---
    [Header("Simulation Speed")]
    [Range(1f, 20f)] // Drag this in the Inspector to speed up the game!
    public float simulationSpeed = 20f; 

    [Header("UI Settings - Populations")]
    public TextMeshProUGUI treeText;    
    public TextMeshProUGUI giraffeText; 

    [Header("UI Settings - Lifetime Events")]
    public TextMeshProUGUI drinkText;   
    public TextMeshProUGUI eatText;     

    private int totalDrinks = 0;
    private int totalEats = 0;
    
    // --- PEAK POPULATION TRACKERS ---
    private int maxTrees = 0;
    private int maxGiraffes = 0;
    
    private float uiUpdateTimer = 0f;
    
    [Header("Simulation Tracking")]
    private float simulationTimer = 0f; 
    private bool simulationEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        Time.timeScale = simulationSpeed; 
    }

    void Update()
    {
        if (simulationEnded) return;

        // Constantly apply your chosen speed!
        Time.timeScale = simulationSpeed;

        // Use unscaledDeltaTime so the JSON timers don't break when you speed up the game!
        simulationTimer += Time.unscaledDeltaTime;
        uiUpdateTimer += Time.unscaledDeltaTime;
        
        if (uiUpdateTimer >= 0.5f)
        {
            uiUpdateTimer = 0f;
            UpdateCounters();
        }
    }

    public void RegisterDrink() { totalDrinks++; }
    public void RegisterEat() { totalEats++; }

    void UpdateCounters()
    {
        int treeCount = GameObject.FindGameObjectsWithTag("Tree").Length;
        int giraffeCount = GameObject.FindGameObjectsWithTag("Giraffe").Length;

        // --- UPDATE THE HIGH SCORES ---
        if (treeCount > maxTrees) maxTrees = treeCount;
        if (giraffeCount > maxGiraffes) maxGiraffes = giraffeCount;

        if (treeText != null) treeText.text = "Trees: " + treeCount.ToString();
        if (giraffeText != null) giraffeText.text = "Giraffes: " + giraffeCount.ToString();
        
        if (drinkText != null) drinkText.text = "Drinks: " + totalDrinks.ToString();
        if (eatText != null) eatText.text = "Eats: " + totalEats.ToString();

        // Extinction Check
        if (simulationTimer > 2f && giraffeCount == 0 && !simulationEnded)
        {
            simulationEnded = true;
            SaveSimulationData(treeCount);
            Time.timeScale = 0f; 
        }
    }

    void SaveSimulationData(int finalTreeCount)
    {
        // 1. Pack our data into the blueprint 
        SimulationResults results = new SimulationResults();
        results.totalTimeSeconds = simulationTimer;
        results.totalEats = totalEats;
        results.totalDrinks = totalDrinks;
        results.remainingTrees = finalTreeCount;
        results.peakTrees = maxTrees;          
        results.peakGiraffes = maxGiraffes;    

        // 2. Convert it to formatted JSON text
        string jsonOutput = JsonUtility.ToJson(results, true);

        // 3. Create the /results/ folder
        string folderPath = Application.dataPath + "/results/";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 4. Create the filename
        string fileName = DateTime.Now.ToString("HH-mm-ss-dd") + ".json";
        string fullPath = folderPath + fileName;

        // 5. Write the file
        File.WriteAllText(fullPath, jsonOutput);

        Debug.Log("<color=#00FF00>EXTINCTION REACHED! Data successfully saved to: </color>" + fullPath);
    }
}

// --- THE JSON BLUEPRINT ---
[System.Serializable]
public class SimulationResults
{
    public float totalTimeSeconds;
    public int totalEats;
    public int totalDrinks;
    public int remainingTrees;
    public int peakTrees;
    public int peakGiraffes;
}