using UnityEngine;
using UnityEngine.UI;

public class GiraffeSurvivalStats : MonoBehaviour
{
    [Header("UI References")]
    public Image hungerBarFill;
    public Image waterBarFill;

    [Header("Stats Settings")]
    public float maxHunger = 100f;
    public float maxWater = 100f;

    private float currentHunger;
    private float currentWater;

    private float hungerDrainRate;
    private float waterDrainRate;

    void Start()
    {
        currentHunger = maxHunger;
        currentWater = maxWater;

        // NEW TIMERS: Drains 100 over 60 seconds and 50 seconds respectively
        hungerDrainRate = maxHunger / 60f; 
        waterDrainRate = maxWater / 50f; 
    }

    void Update()
    {
        // Drain stats over time
        if (currentHunger > 0)
        {
            currentHunger -= hungerDrainRate * Time.deltaTime;
        }

        if (currentWater > 0)
        {
            currentWater -= waterDrainRate * Time.deltaTime;
        }

        // Keep values between 0 and their maximum
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentWater = Mathf.Clamp(currentWater, 0, maxWater);

        // Update the visual Fill Amount on the UI Images
        if (hungerBarFill != null)
        {
            hungerBarFill.fillAmount = currentHunger / maxHunger;
        }

        if (waterBarFill != null)
        {
            waterBarFill.fillAmount = currentWater / maxWater;
        }
    }
}