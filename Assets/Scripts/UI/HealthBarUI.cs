using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Image healthBarImageValue;
    [SerializeField] private TextMeshProUGUI healthBarTextValue;

    #endregion

    #region Health Bar Methods

    public void ChangeHealthBarValue(int currentHealth, int maxHealth)
    {
        healthBarTextValue.text = $"{currentHealth} / {maxHealth}";
        var fillAmount = currentHealth / (float)maxHealth;
        healthBarImageValue.fillAmount = fillAmount;
    }

    #endregion
}
