using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RebindKeymapSingleUI : MonoBehaviour
{
    public static event EventHandler<OnBindingButtonRebindEventArgs> OnBindingButtonRebind;

    public class OnBindingButtonRebindEventArgs : EventArgs
    {
        public bool isShowing;
    }

    [SerializeField] private GameInput.Binding bindingType = GameInput.Binding.MoveUp;
    [SerializeField] private TextMeshProUGUI bindButtonText;

    private Button bindingButton;

    private void Awake()
    {
        bindingButton = GetComponent<Button>();
    }

    private void Start()
    {
        UpdateBindingText();

        bindingButton.onClick.AddListener(() => { RebindBinding(bindingType); });

        SettingsUI.OnKeymapsButtonClick += SettingsUI_OnKeymapsButtonClick;
    }

    private void SettingsUI_OnKeymapsButtonClick(object sender, EventArgs e)
    {
        UpdateBindingText();
    }

    private void UpdateBindingText()
    {
        bindButtonText.text = GameInput.Instance.GetBindingText(bindingType);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        OnBindingButtonRebind?.Invoke(this, new OnBindingButtonRebindEventArgs
        {
            isShowing = true
        });

        GameInput.Instance.RebindBinding(binding, () =>
        {
            OnBindingButtonRebind?.Invoke(this, new OnBindingButtonRebindEventArgs
            {
                isShowing = false
            });
            UpdateBindingText();
        });
    }
}
