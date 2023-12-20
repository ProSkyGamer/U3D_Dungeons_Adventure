using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarImageValue;
    [SerializeField] private TextMeshProUGUI healthBarTextValue;

    public void ChangeHealthBarValue(int currentHealth, int maxHealth)
    {
        healthBarTextValue.text = $"{currentHealth} / {maxHealth}";
        var fillAmount = currentHealth / (float)maxHealth;
        healthBarImageValue.fillAmount = fillAmount;
    }
}
