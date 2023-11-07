using TMPro;
using UnityEngine;

public class ChangeLanguageDropdownUI : MonoBehaviour
{
    private TMP_Dropdown languagesDropdown;

    private void Awake()
    {
        languagesDropdown = GetComponent<TMP_Dropdown>();

        languagesDropdown.options.Clear();
        languagesDropdown.options.Add(
            new TMP_Dropdown.OptionData(TextTranslationController.Languages.English.ToString()));
        languagesDropdown.options.Add(new TMP_Dropdown.OptionData("Русский")); //Чтобы текст в Dropdown был на русском

        languagesDropdown.value = (int)TextTranslationController.GetCurrentLanguage();
    }

    private void Start()
    {
        languagesDropdown.onValueChanged.AddListener(ChangeLanguage);
    }

    private void ChangeLanguage(int newValue)
    {
        TextTranslationController.ChangeLanguage((TextTranslationController.Languages)newValue);
    }
}
