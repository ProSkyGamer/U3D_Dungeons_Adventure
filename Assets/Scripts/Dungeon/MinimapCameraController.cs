using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    public static MinimapCameraController Instance;

    public enum MinimapRenderingSize
    {
        Small,
        Middle,
        Big
    }

    [SerializeField] private bool isFollowing = true;
    [SerializeField] private Transform followingObject;
    [SerializeField] private bool isMinimapFixed = true;
    [SerializeField] private MinimapRenderingSize currentRenderingSize = MinimapRenderingSize.Middle;
    [SerializeField] private float smallRenderingSizeCameraYPosition = 10;
    [SerializeField] private float middleRenderingSizeCameraYPosition = 15;
    [SerializeField] private float bigRenderingSizeCameraYPosition = 20;

    private Transform cameraTransform;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        cameraTransform = transform;
    }

    private void Update()
    {
        if (!isFollowing) return;
        if (followingObject == null) return;

        var followingObjectPosition = followingObject.position;
        var newPosition = new Vector3(followingObjectPosition.x, transform.position.y, followingObjectPosition.z);

        if (newPosition != transform.position)
            transform.position = newPosition;

        if (isMinimapFixed) return;

        var playerRotation = followingObject.localEulerAngles.y;

        var localEulerAngles = cameraTransform.localEulerAngles;
        var cameraRotation = new Vector3(localEulerAngles.x, playerRotation, localEulerAngles.z);
        cameraTransform.localEulerAngles = cameraRotation;
    }

    public void ChangeCameraFixedMode(bool isFixed)
    {
        isMinimapFixed = isFixed;

        if (!isFixed) return;

        var localEulerAngles = cameraTransform.localEulerAngles;
        var cameraRotation = new Vector3(localEulerAngles.x, 0f, localEulerAngles.z);
        cameraTransform.localEulerAngles = cameraRotation;
    }

    public void ChangeCameraRenderingSize(MinimapRenderingSize newSize)
    {
        currentRenderingSize = newSize;
        var newCameraPosition = cameraTransform.position;

        switch (newSize)
        {
            case MinimapRenderingSize.Small:
                newCameraPosition.y = smallRenderingSizeCameraYPosition;
                break;
            case MinimapRenderingSize.Middle:
                newCameraPosition.y = middleRenderingSizeCameraYPosition;
                break;
            case MinimapRenderingSize.Big:
                newCameraPosition.y = bigRenderingSizeCameraYPosition;
                break;
        }

        transform.position = newCameraPosition;
    }

    public void ChangeFollowingObject(Transform newFollowingObject)
    {
        followingObject = newFollowingObject;
    }

    public bool IsMinimapFixed()
    {
        return isMinimapFixed;
    }
}
