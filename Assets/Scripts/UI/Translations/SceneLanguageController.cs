using System;
using UnityEngine;

public class SceneLanguageController : MonoBehaviour
{
    private bool isFirstUpdate = true;

    private void Start()
    {
        //Loader.OnSceneChange += Loader_OnSceneChange;
    }

    private void Loader_OnSceneChange(object sender, EventArgs e)
    {
        TextTranslationController.ResetStaticData();
        TextTranslationController.ChangeLanguage(TextTranslationController.GetCurrentLanguage());
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            TextTranslationController.ChangeLanguage(TextTranslationController.GetCurrentLanguage());
        }
    }
}
