using UnityEngine;
using TMPro; 

public class EcosystemCounter : MonoBehaviour
{
    // The "Singleton" - This allows any giraffe to easily find this exact script!
    public static EcosystemCounter Instance;

    [Header("UI Settings - Populations")]
    public TextMeshProUGUI treeText;    
    public TextMeshProUGUI giraffeText; 

    [Header("UI Settings - Lifetime Events")]
    public TextMeshProUGUI drinkText;   // NEW: Slot for drinks
    public TextMeshProUGUI eatText;     // NEW: Slot for eats

    // The hidden memory banks
    private int totalDrinks = 0;
    private int totalEats = 0;
    private float timer = 0f;

    void Awake()
    {
        // When the game starts, this script announces itself as the global manager
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= 0.5f)
        {
            timer = 0f;
            UpdateCounters();
        }
    }

    // A giraffe will call this exact function when it drinks!
    public void RegisterDrink()
    {
        totalDrinks++;
    }

    // A giraffe will call this exact function when it eats!
    public void RegisterEat()
    {
        totalEats++;
    }

    void UpdateCounters()
    {
        int treeCount = GameObject.FindGameObjectsWithTag("Tree").Length;
        int giraffeCount = GameObject.FindGameObjectsWithTag("Giraffe").Length;

        if (treeText != null) treeText.text = "Trees: "+treeCount.ToString();
        if (giraffeText != null) giraffeText.text = "Giraffes: "+giraffeCount.ToString();
        
        // Update our new screens!
        if (drinkText != null) drinkText.text = "Drinks: "+totalDrinks.ToString();
        if (eatText != null) eatText.text = "Eats: "+totalEats.ToString();
    }
}