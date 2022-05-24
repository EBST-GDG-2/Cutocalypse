using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public void SetMaxHealth(int heatlh)
    {
        slider.maxValue = heatlh;
        slider.value = heatlh;
        fill.color = gradient.Evaluate(1f);
    }
    public void SetHealth(int healt)
    {
        slider.value = healt;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
