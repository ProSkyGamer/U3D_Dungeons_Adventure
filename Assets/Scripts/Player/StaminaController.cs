using System;
using UnityEngine;

public class StaminaController : MonoBehaviour
{
    public static event EventHandler<OnStaminaChangeEventArgs> OnStaminaChange;

    public class OnStaminaChangeEventArgs : EventArgs
    {
        public int maxStamina;
        public int currentStamina;
    }

    public event EventHandler OnAllStaminaSpend;

    [SerializeField] private int maxStamina = 100;

    [SerializeField] private float staminaRecoveryPerSecond = 25f;

    private float currentStamina;

    private bool isRegeneratingStamina;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            OnStaminaChange?.Invoke(this, new OnStaminaChangeEventArgs
            {
                currentStamina = (int)currentStamina, maxStamina = maxStamina
            });
        }

        if (isRegeneratingStamina)
        {
            currentStamina = Math.Clamp(currentStamina + staminaRecoveryPerSecond * Time.deltaTime, 0, maxStamina);

            OnStaminaChange?.Invoke(this, new OnStaminaChangeEventArgs
            {
                currentStamina = (int)currentStamina, maxStamina = maxStamina
            });

            if (currentStamina >= maxStamina)
                StopStaminaRegeneration();
        }
    }

    public void SpendStamina(float toSpend)
    {
        currentStamina = Math.Clamp(currentStamina - toSpend, 0, maxStamina);

        OnStaminaChange?.Invoke(this, new OnStaminaChangeEventArgs
        {
            currentStamina = (int)currentStamina, maxStamina = maxStamina
        });

        if (isRegeneratingStamina)
            StopStaminaRegeneration();

        if (currentStamina <= 0)
            OnAllStaminaSpend?.Invoke(this, EventArgs.Empty);
    }

    public void StartStaminaRegeneration()
    {
        isRegeneratingStamina = true;
    }

    private void StopStaminaRegeneration()
    {
        isRegeneratingStamina = false;
    }

    public bool IsHaveAvailableStamina()
    {
        return currentStamina > 0;
    }

    public bool IsHaveNeededStamina(float staminaValueToCheck)
    {
        return currentStamina >= staminaValueToCheck;
    }

    public bool IsRegeneratingStamina()
    {
        return isRegeneratingStamina;
    }

    public bool IsStaminaFull()
    {
        return currentStamina >= maxStamina;
    }
}
