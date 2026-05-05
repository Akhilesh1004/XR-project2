using UnityEngine;

public class PortalCameraViewSync : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera portalCamera;

    [Header("Anchors")]
    [SerializeField] private Transform portalAAnchor;
    [SerializeField] private Transform portalBAnchor;

    [Header("Window Style Parallax")]
    [Tooltip("玩家位移對 PortalCamera 位移的影響。0 = 完全固定，1 = 完全跟隨。建議 0.2~0.5")]
    [SerializeField] private float positionInfluence = 0.35f;

    [Tooltip("玩家旋轉對 PortalCamera 旋轉的影響。0 = 固定看門內，1 = 完全同步玩家視角。建議先用 0")]
    [SerializeField] private float rotationInfluence = 0.0f;

    [Tooltip("限制 PortalCamera 最大偏移，避免像在操控另一台攝影機。")]
    [SerializeField] private float maxLocalOffset = 1.0f;

    [Header("Options")]
    [SerializeField] private bool use180Flip = false;

    public void SetPortalBAnchor(Transform anchor)
    {
        portalBAnchor = anchor;
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;
        if (portalCamera == null) return;
        if (portalAAnchor == null) return;
        if (portalBAnchor == null) return;
        if (!portalCamera.enabled) return;

        SyncWindowPosition();
        SyncWindowRotation();

        portalCamera.fieldOfView = playerCamera.fieldOfView;
        portalCamera.nearClipPlane = 0.05f;
    }

    private void SyncWindowPosition()
    {
        Vector3 localOffset =
            portalAAnchor.InverseTransformPoint(playerCamera.transform.position);

        localOffset = Vector3.ClampMagnitude(localOffset, maxLocalOffset);

        localOffset *= positionInfluence;

        if (use180Flip)
        {
            localOffset = Quaternion.Euler(0f, 180f, 0f) * localOffset;
        }

        portalCamera.transform.position =
            portalBAnchor.TransformPoint(localOffset);
    }

    private void SyncWindowRotation()
    {
        Quaternion baseRotation = portalBAnchor.rotation;

        if (use180Flip)
        {
            baseRotation = portalBAnchor.rotation * Quaternion.Euler(0f, 180f, 0f);
        }

        if (rotationInfluence <= 0f)
        {
            portalCamera.transform.rotation = baseRotation;
            return;
        }

        Quaternion relativeRotation =
            Quaternion.Inverse(portalAAnchor.rotation) * playerCamera.transform.rotation;

        Quaternion targetRotation = portalBAnchor.rotation * relativeRotation;

        if (use180Flip)
        {
            targetRotation = portalBAnchor.rotation *
                             Quaternion.Euler(0f, 180f, 0f) *
                             relativeRotation;
        }

        portalCamera.transform.rotation =
            Quaternion.Slerp(baseRotation, targetRotation, rotationInfluence);
    }
}