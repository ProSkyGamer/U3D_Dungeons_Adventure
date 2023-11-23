using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraModes
    {
        ThirdPerson,
        Weapon
    }

    [Header("Main Camera Settings")]
    [SerializeField] private Transform playerVisualTransform;

    [SerializeField] private CameraModes currentCameraMode = CameraModes.ThirdPerson;
    [SerializeField] private bool isCameraMovementLocked;

    [SerializeField] private float minCameraDistance = 4f;
    [SerializeField] private float maxCameraDistance = 6f;
    [SerializeField] private float playerHideDistance = 3f;
    [SerializeField] private Vector3 additionalCameraOffset;
    private float currentCameraDistance;
    private float previousCameraDistance;
    private readonly float smoothAutoCameraDistanceSpeed = 0.2f;

    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float smoothing = 2f;

    [SerializeField] private float maxYAngle = 75f;
    [SerializeField] private float minYAngle = 345f;

    [SerializeField] private LayerMask cameraCollidableTerrain;

    private Vector2 velocity;
    private Vector2 frameVelocity;

    private bool isAnyInterfaceOpened;
    private bool isShowingCursor;

    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = transform;

        currentCameraDistance = (maxCameraDistance + minCameraDistance) / 2;

        ChangeCameraMode(currentCameraMode);

        Cursor.visible = false;
    }

    private void Start()
    {
        PlayerAttackController.OnGunChargedAttackTriggered += PlayerAttackController_OnGunChargedAttackTriggered;
        GameInput.Instance.OnCursorShowAction += GameInput_OnCursorShowAction;

        ShopUI.Instance.OnShopOpen += OnAnyTabOpen;
        ShopUI.Instance.OnShopClose += OnAnyTabClose;

        CharacterUI.OnCharacterUIOpen += OnAnyTabOpen;
        CharacterUI.OnCharacterUIClose += OnAnyTabClose;
    }

    private void GameInput_OnCursorShowAction(object sender, EventArgs e)
    {
        isShowingCursor = true;
        Cursor.visible = true;
    }

    private void OnAnyTabClose(object sender, EventArgs e)
    {
        isAnyInterfaceOpened = false;
    }

    private void OnAnyTabOpen(object sender, EventArgs e)
    {
        isAnyInterfaceOpened = true;
        isShowingCursor = true;
        Cursor.visible = true;
    }

    private void PlayerAttackController_OnGunChargedAttackTriggered(object sender, EventArgs e)
    {
        switch (currentCameraMode)
        {
            case CameraModes.ThirdPerson:
                ChangeCameraMode(CameraModes.Weapon);
                break;
            case CameraModes.Weapon:
                ChangeCameraMode(CameraModes.ThirdPerson);
                break;
        }
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;
        if (isAnyInterfaceOpened) return;
        if (isCameraMovementLocked) return;

        CameraMovement();
        CameraScroll();
        CameraAutoDistance();
        TryHideClosestObjects();

        if (isShowingCursor)
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.ShowCursor) != 1f)
                Cursor.visible = false;
    }

    private void CameraMovement()
    {
        var smoothedMousePosition = Vector2.Lerp(Vector2.zero,
            GameInput.Instance.GetMousePosition() * (sensitivity * Time.deltaTime), smoothing);

        switch (currentCameraMode)
        {
            case CameraModes.ThirdPerson:
                Camera_ThirdPersonModeMovement(smoothedMousePosition);
                break;
            case CameraModes.Weapon:
                Camera_WeaponModeMovement(smoothedMousePosition);
                break;
        }
    }

    private void Camera_ThirdPersonModeMovement(Vector2 mouseDelta)
    {
        RotateCamera(mouseDelta);
    }

    private void Camera_WeaponModeMovement(Vector2 mouseDelta)
    {
        RotateCamera(mouseDelta);
    }

    private void RotateCamera(Vector2 mouseRotation)
    {
        cameraTransform.Rotate(-mouseRotation.y, mouseRotation.x, 0f);

        var transformLocalEulerAngles = cameraTransform.localEulerAngles;
        transformLocalEulerAngles.z = 0;
        if (transformLocalEulerAngles.x > maxYAngle && transformLocalEulerAngles.x < minYAngle)
            transformLocalEulerAngles.x = transformLocalEulerAngles.x - 100 > maxYAngle
                ? Mathf.Clamp(transformLocalEulerAngles.x, minYAngle, maxYAngle)
                : maxYAngle;

        cameraTransform.localEulerAngles = transformLocalEulerAngles;

        var newCameraPosition = GetAvailableCameraPosition(
            currentCameraDistance, out var newCameraDistance);

        if (newCameraDistance < currentCameraDistance)
        {
            if (previousCameraDistance == 0)
                previousCameraDistance = currentCameraDistance;

            currentCameraDistance = newCameraDistance;
        }

        cameraTransform.position = newCameraPosition;
    }

    private Vector3 GetDesiredCameraPositionByDistance(float cameraDistance)
    {
        var currentPlayerPosition = playerVisualTransform.position;
        var transformLocalEulerAngles = cameraTransform.localEulerAngles;

        return new Vector3(
            currentPlayerPosition.x -
            cameraDistance * Mathf.Sin(transformLocalEulerAngles.y * Mathf.PI / 180) *
            Mathf.Cos(transformLocalEulerAngles.x * Mathf.PI / 180),
            currentPlayerPosition.y +
            cameraDistance * Mathf.Sin(transformLocalEulerAngles.x * Mathf.PI / 180),
            currentPlayerPosition.z -
            cameraDistance * Mathf.Cos(transformLocalEulerAngles.y * Mathf.PI / 180) *
            Mathf.Cos(transformLocalEulerAngles.x * Mathf.PI / 180)) + additionalCameraOffset;
    }

    private Vector3 GetAvailableCameraPosition(float cameraDistance, out float newCameraDistance)
    {
        var desiredCameraPosition = GetDesiredCameraPositionByDistance(cameraDistance);
        newCameraDistance = cameraDistance;

        var raycastHits = Physics.RaycastAll(desiredCameraPosition, cameraTransform.forward,
            currentCameraDistance, cameraCollidableTerrain);

        var closestDistance = maxCameraDistance;
        foreach (var raycastHit in raycastHits)
        {
            if (raycastHit.distance >= closestDistance || raycastHit.distance == 0) continue;

            closestDistance = raycastHit.distance;
        }

        if (cameraDistance - closestDistance < cameraDistance && closestDistance != maxCameraDistance)
        {
            desiredCameraPosition = GetDesiredCameraPositionByDistance(cameraDistance - closestDistance - 0.01f);

            newCameraDistance = cameraDistance - closestDistance;
        }

        return desiredCameraPosition;
    }

    private void CameraScroll()
    {
        if (currentCameraMode == CameraModes.ThirdPerson)
        {
            var mouseScroll = GameInput.Instance.GetMouseScroll();

            if (mouseScroll != 0)
            {
                mouseScroll = mouseScroll > 0 ? 1 : -1;

                if (previousCameraDistance != 0 && mouseScroll < 0)
                    previousCameraDistance = 0;

                var scrollValue =
                    Mathf.Clamp(
                        Mathf.Lerp(currentCameraDistance, currentCameraDistance + mouseScroll * Time.deltaTime,
                            smoothing), minCameraDistance, maxCameraDistance);

                currentCameraDistance = scrollValue;

                RotateCamera(Vector2.zero);
            }
        }
    }

    private void CameraAutoDistance()
    {
        if (previousCameraDistance == 0) return;
        if (currentCameraMode != CameraModes.ThirdPerson) return;

        var newCameraPosition = GetAvailableCameraPosition(
            currentCameraDistance + smoothAutoCameraDistanceSpeed, out var newCameraDistance);

        if (newCameraDistance > currentCameraDistance)
        {
            if (newCameraDistance >= previousCameraDistance)
                previousCameraDistance = 0;

            cameraTransform.position = newCameraPosition;
            currentCameraDistance = newCameraDistance;
        }
    }

    private void TryHideClosestObjects()
    {
        playerVisualTransform.gameObject.SetActive(currentCameraDistance > playerHideDistance);
    }

    public void ChangeCameraMode(CameraModes newCameraMode)
    {
        currentCameraMode = newCameraMode;

        switch (currentCameraMode)
        {
            case CameraModes.ThirdPerson:
                currentCameraDistance = previousCameraDistance == 0
                    ? (maxCameraDistance + minCameraDistance) / 2
                    : previousCameraDistance;
                break;
            case CameraModes.Weapon:
                previousCameraDistance = currentCameraDistance;
                currentCameraDistance = 0;
                break;
        }
    }

    public CameraModes GetCurrentCameraMode()
    {
        return currentCameraMode;
    }
}
