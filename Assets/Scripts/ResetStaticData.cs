public static class ResetStaticData
{
    public static void ResetData()
    {
        TextTranslationController.ResetStaticData();
        CharacterUI.ResetStaticData();
        CharacterInventoryUI.ResetStaticData();
        InventorySlotSingleUI.ResetStaticData();
        PauseUI.ResetStaticData();
        RebindKeymapSingleUI.ResetStaticData();
        SettingsUI.ResetStaticData();
    }
}