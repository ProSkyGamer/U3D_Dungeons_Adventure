using System;
using UnityEngine;

public static class TextTranslationController
{
    public const string PLAYER_PREFS_CURRENT_LANGUAGE = "CurrentLanguage";

    public static event EventHandler<OnLanguageChangeEventArgs> OnLanguageChange;

    public class OnLanguageChangeEventArgs : EventArgs
    {
        public Languages language;
    }

    public enum Languages
    {
        English,
        Russian
    }

    private static Languages currentLanguage;

    public static void ChangeLanguage(Languages changeLanguageTo)
    {
        currentLanguage = changeLanguageTo;
        PlayerPrefs.SetInt(PLAYER_PREFS_CURRENT_LANGUAGE, (int)currentLanguage);
        OnLanguageChange?.Invoke(null, new OnLanguageChangeEventArgs
            { language = currentLanguage });
    }

    public static Languages GetCurrentLanguage()
    {
        return currentLanguage = (Languages)PlayerPrefs.GetInt(PLAYER_PREFS_CURRENT_LANGUAGE, 0);
    }

    public static void ResetStaticData()
    {
        OnLanguageChange = null;
    }

    public static string GetTextFromTextTranslationSOByLanguage(Languages language,
        TextTranslationsSO textTranslationsSO)
    {
        return language == Languages.English ? textTranslationsSO.englishTextTranslation :
            language == Languages.Russian ? textTranslationsSO.russianTextTranslation : "Undefined Language";
    }
}
