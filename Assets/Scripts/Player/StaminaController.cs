using System;
using Unity.Netcode;
using UnityEngine;

public class StaminaController : NetworkBehaviour
{
    #region Events & Event Args

    public event EventHandler<OnStaminaChangeEventArgs> OnStaminaChange;

    public class OnStaminaChangeEventArgs : EventArgs
    {
        public int maxStamina;
        public int currentStamina;
    }

    public event EventHandler OnAllStaminaSpend;

    #endregion

    #region Variables & References

    [SerializeField] private int maxStamina = 100;
    private float staminaSpendMultiplayer = 1f;
    [SerializeField] private float staminaRecoveryPerSecond = 25f;

    private float currentStamina;
    private bool isRegeneratingStamina;

    private bool isFirstUpdate = true;

    #endregion

    #region Initialization

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    #endregion

    #region Update

    private void Update()
    {
        if (!IsOwner) return;
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

    #endregion

    #region Effects

    public void ChangeStaminaSpendMultiplayer(float deltaValue)
    {
        staminaSpendMultiplayer += deltaValue;
    }

    #endregion

    #region Stamina Spending & Regenerating

    public void SpendStamina(float toSpend)
    {
        currentStamina = Math.Clamp(currentStamina - toSpend * staminaSpendMultiplayer, 0, maxStamina);

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

    #endregion

    #region Check Data

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

    #endregion
}
