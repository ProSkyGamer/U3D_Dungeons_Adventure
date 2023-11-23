using TMPro;
using UnityEngine;

public class TextTranslationSingleUI : MonoBehaviour
{
    [SerializeField] private TextTranslationsSO textTranslationsSO;

    private TextMeshProUGUI currentLabelText;

    private void Awake()
    {
        currentLabelText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        TextTranslationController.OnLanguageChange += TextTranslationManager_OnLanguageChange;
    }

    private void TextTranslationManager_OnLanguageChange(object sender,
        TextTranslationController.OnLanguageChangeEventArgs e)
    {
        currentLabelText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(e.language, textTranslationsSO);
    }

    public void ChangeTextTranslationSO(TextTranslationsSO textTranslationsSO)
    {
        this.textTranslationsSO = textTranslationsSO;

        if (currentLabelText == null)
            currentLabelText = GetComponent<TextMeshProUGUI>();

        currentLabelText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), textTranslationsSO);
    }
}
