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

    [SerializeField] private float minCameraDistance = 1.5f;
    [SerializeField] private float maxCameraDistance = 3.5f;
    [SerializeField] private Vector3 additionalCameraOffset;
    private float currentCameraDistance;

    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float smoothing = 2f;

    [SerializeField] private float maxYAngle = 75f;
    [SerializeField] private float minYAngle = 345f;

    [SerializeField] private LayerMask cameraCollidableTerrain;

    private Vector2 velocity;
    private Vector2 frameVelocity;

    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = transform;

        currentCameraDistance = (maxCameraDistance + minCameraDistance) / 2;

        ChangeCameraMode(currentCameraMode);
    }

    private void Update()
    {
        if (isCameraMovementLocked)
            return;

        CameraMovement();
        CameraScroll();
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

        var cameraPosition = GetDesiredCameraPosition();

        var raycastHits = Physics.RaycastAll(cameraPosition, cameraTransform.forward,
            currentCameraDistance, cameraCollidableTerrain);

        while (raycastHits.Length > 0)
        {
            currentCameraDistance -= 0.1f;

            cameraPosition = GetDesiredCameraPosition();

            raycastHits = Physics.RaycastAll(cameraPosition, cameraTransform.forward,
                currentCameraDistance, cameraCollidableTerrain);
        }


        cameraTransform.position = cameraPosition;
    }

    private Vector3 GetDesiredCameraPosition()
    {
        var currentPlayerPosition = playerVisualTransform.position;
        var transformLocalEulerAngles = cameraTransform.localEulerAngles;

        return new Vector3(
            currentPlayerPosition.x -
            currentCameraDistance * Mathf.Sin(transformLocalEulerAngles.y * Mathf.PI / 180) *
            Mathf.Cos(transformLocalEulerAngles.x * Mathf.PI / 180),
            currentPlayerPosition.y +
            currentCameraDistance * Mathf.Sin(transformLocalEulerAngles.x * Mathf.PI / 180),
            currentPlayerPosition.z -
            currentCameraDistance * Mathf.Cos(transformLocalEulerAngles.y * Mathf.PI / 180) *
            Mathf.Cos(transformLocalEulerAngles.x * Mathf.PI / 180)) + additionalCameraOffset;
    }

    private void CameraScroll()
    {
        if (currentCameraMode == CameraModes.ThirdPerson)
        {
            var mouseScroll = GameInput.Instance.GetMouseScroll();

            if (mouseScroll != 0)
            {
                mouseScroll = mouseScroll > 0 ? 1 : -1;

                var scrollValue =
                    Mathf.Clamp(
                        Mathf.Lerp(currentCameraDistance, currentCameraDistance + mouseScroll * Time.deltaTime,
                            smoothing), minCameraDistance, maxCameraDistance);

                currentCameraDistance = scrollValue;

                RotateCamera(Vector2.zero);
            }
        }
    }

    public void ChangeCameraMode(CameraModes newCameraMode)
    {
        currentCameraMode = newCameraMode;

        switch (currentCameraMode)
        {
            case CameraModes.ThirdPerson:
                currentCameraDistance = (minCameraDistance + maxCameraDistance) / 2;
                break;
            case CameraModes.Weapon:
                currentCameraDistance = 0;
                break;
        }
    }

    public CameraModes GetCurrentCameraMode()
    {
        return currentCameraMode;
    }
}
