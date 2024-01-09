using System;
using TMPro;
using UnityEngine;

public class BindingButtonTextTracker : MonoBehaviour
{
    #region Vatiables & References

    [SerializeField] private GameInput.Binding followingBinding = GameInput.Binding.Attack;

    private TextMeshProUGUI bindingText;

    #endregion

    #region Initialization & Subscribed events

    private void Start()
    {
        UpdateBindingText();

        GameInput.Instance.OnAnyBindingRebind += GameInput_OnAnyBindingRebind;
    }

    private void GameInput_OnAnyBindingRebind(object sender, EventArgs e)
    {
        UpdateBindingText();
    }

    #endregion

    #region Binding Button Methods

    private void UpdateBindingText()
    {
        if (bindingText == null)
            bindingText = GetComponent<TextMeshProUGUI>();

        bindingText.text = GameInput.Instance.GetBindingText(followingBinding);
    }

    public void ChangeTrackingBinding(GameInput.Binding newBinding)
    {
        followingBinding = newBinding;
        UpdateBindingText();
    }

    #endregion
}
