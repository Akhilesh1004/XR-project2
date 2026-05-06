using UnityEngine;

public class RecorderPoseCheckerUI : MonoBehaviour
{
    [Header("References")]
    public OVRCameraRig cameraRig;
    public Camera uiCamera;

    public Transform leftController;
    public Transform rightController;

    [Header("Canvas / UI")]
    public RectTransform canvasRect;
    public RectTransform leftHandTargetUI;
    public RectTransform rightHandTargetUI;

    [Header("Position Threshold (UI local distance)")]
    public float leftHandPositionTolerance = 80f;
    public float rightHandPositionTolerance = 80f;

    [Header("Rotation Threshold (Local Rotation)")]
    public Vector3 leftHandTargetEuler = new Vector3(39f, -65f, -53.5f);
    public Vector3 rightHandTargetEuler = new Vector3(51.4f, 69.35f, 64.25f);

    public float leftHandAngleTolerance = 30f;
    public float rightHandAngleTolerance = 30f;

    [Header("Canvas Type")]
    [Tooltip("如果 Canvas 是 Screen Space - Overlay，勾選這個")]
    public bool isOverlayCanvas = false;

    [Header("Runtime")]
    public bool debugLog = false;

    [Header("Output")]
    public bool canPlay = false;

    private void Awake()
    {
        if (cameraRig == null)
            cameraRig = FindObjectOfType<OVRCameraRig>();

        if (uiCamera == null && cameraRig != null && cameraRig.centerEyeAnchor != null)
            uiCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();

        if (leftController == null && cameraRig != null && cameraRig.leftHandAnchor != null)
            leftController = cameraRig.leftHandAnchor;

        if (rightController == null && cameraRig != null && cameraRig.rightHandAnchor != null)
            rightController = cameraRig.rightHandAnchor;
    }

    private void Update()
    {
        canPlay = CheckPlayPose();
    }

    public bool CheckPlayPose()
    {
        if (canvasRect == null || leftController == null || rightController == null)
            return false;

        if (leftHandTargetUI == null || rightHandTargetUI == null)
            return false;

        bool leftPosOK = IsWorldPointNearUITarget(
            leftController.position,
            leftHandTargetUI,
            leftHandPositionTolerance
        );

        bool rightPosOK = IsWorldPointNearUITarget(
            rightController.position,
            rightHandTargetUI,
            rightHandPositionTolerance
        );

        Quaternion leftTargetRot = Quaternion.Euler(leftHandTargetEuler);
        Quaternion rightTargetRot = Quaternion.Euler(rightHandTargetEuler);

        // 改成比 localRotation
        float leftAngle = Quaternion.Angle(leftController.localRotation, leftTargetRot);
        float rightAngle = Quaternion.Angle(rightController.localRotation, rightTargetRot);

        bool leftRotOK = leftAngle <= leftHandAngleTolerance;
        bool rightRotOK = rightAngle <= rightHandAngleTolerance;

        if (debugLog)
        {
            Debug.Log(
                $"leftPosOK={leftPosOK}, rightPosOK={rightPosOK}, " +
                $"leftRotOK={leftRotOK}, rightRotOK={rightRotOK}, " +
                $"leftAngle={leftAngle:F1}, rightAngle={rightAngle:F1}, " +
                $"leftLocalEuler={leftController.localEulerAngles}, rightLocalEuler={rightController.localEulerAngles}"
            );
        }

        return leftPosOK && rightPosOK && leftRotOK && rightRotOK;
    }

    private bool IsWorldPointNearUITarget(Vector3 worldPos, RectTransform targetUI, float tolerance)
    {
        if (!isOverlayCanvas && uiCamera == null)
            return false;

        Vector3 screenPoint = isOverlayCanvas
            ? RectTransformUtility.WorldToScreenPoint(null, worldPos)
            : uiCamera.WorldToScreenPoint(worldPos);

        if (!isOverlayCanvas && screenPoint.z < 0f)
            return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            isOverlayCanvas ? null : uiCamera,
            out Vector2 localPoint
        );

        float dist = Vector2.Distance(localPoint, targetUI.anchoredPosition);
        return dist <= tolerance;
    }
}