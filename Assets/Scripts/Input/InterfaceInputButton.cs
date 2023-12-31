using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceInputButton : NetworkBehaviour
{
    [SerializeField] private GameInput.Binding inputBinding = GameInput.Binding.Attack;

    [SerializeField] private Transform pressedInputBindingButton;

    [SerializeField] private BindingButtonTextTracker bindingButtonTextTracker;

    private Button interfaceBindingButton;
    private float hideAfterTime;

    private bool isFirstUpdate;

    private void Awake()
    {
        pressedInputBindingButton.gameObject.SetActive(false);

        interfaceBindingButton = GetComponent<Button>();
    }

    public override void OnNetworkSpawn()
    {
        isFirstUpdate = true;
    }

    private void Start()
    {
        bindingButtonTextTracker.ChangeTrackingBinding(inputBinding);

        interfaceBindingButton.onClick.AddListener(() => { GameInput.Instance.TriggerBindingButton(inputBinding); });
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            switch (inputBinding)
            {
                case GameInput.Binding.Attack:
                    GameInput.Instance.OnAttackAction += OnFollowingActionTriggered;
                    PlayerController.Instance.GetPlayerAttackController().OnChargeAttackStopCharging +=
                        OnFollowingActionStopped;
                    break;
                case GameInput.Binding.Sprint:
                    GameInput.Instance.OnSprintAction += OnFollowingActionTriggered;
                    PlayerController.Instance.OnStopSprinting += OnFollowingActionStopped;
                    break;
            }
        }

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
