using System;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceInputButton : MonoBehaviour
{
    [SerializeField] private GameInput.Binding inputBinding = GameInput.Binding.Attack;

    [SerializeField] private Transform pressedInputBindingButton;

    [SerializeField] private BindingButtonTextTracker bindingButtonTextTracker;

    private Button interfaceBindingButton;
    private float hideAfterTime;

    private void Awake()
    {
        pressedInputBindingButton.gameObject.SetActive(false);

        interfaceBindingButton = GetComponent<Button>();
    }

    private void Start()
    {
        bindingButtonTextTracker.ChangeTrackingBinding(inputBinding);

        interfaceBindingButton.onClick.AddListener(() => { GameInput.Instance.TriggerBindingButton(inputBinding); });

        switch (inputBinding)
        {
            case GameInput.Binding.Attack:
                GameInput.Instance.OnAttackAction += OnFollowingActionTriggered;
                PlayerAttackController.OnChargeAttackStopCharging += OnFollowingActionStopped;
                break;
            case GameInput.Binding.Sprint:
                GameInput.Instance.OnSprintAction += OnFollowingActionTriggered;
                PlayerController.Instance.OnStopSprinting += OnFollowingActionStopped;
                break;
        }
    }

    private void Update()
    {
        if (hideAfterTime > 0)
        {
            hideAfterTime -= Time.deltaTime;

            if (hideAfterTime <= 0)
                pressedInputBindingButton.gameObject.SetActive(false);
        }
    }

    private void OnFollowingActionStopped(object sender, EventArgs e)
    {
        WaitBeforeHidingPressedButton();
    }

    private void WaitBeforeHidingPressedButton()
    {
        var waitFor = 0.25f;

        hideAfterTime = waitFor;
    }

    private void OnFollowingActionTriggered(object sender, EventArgs e)
    {
        pressedInputBindingButton.gameObject.SetActive(true);
    }
}
