using UnityEngine;
using UnityEngine.UI; 

public class HealthBar : MonoBehaviour
{
    public Slider slider; 

    // Å‘åHP‚ğİ’è
    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    // Œ»İ‚ÌHP‚ğXV
    public void SetHealth(float health)
    {
        slider.value = health;
    }
}