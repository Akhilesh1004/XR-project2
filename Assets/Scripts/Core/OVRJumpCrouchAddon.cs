using UnityEngine;

[RequireComponent(typeof(OVRPlayerController))]
[RequireComponent(typeof(CharacterController))]
public class OVRJumpCrouchAddon : MonoBehaviour
{
    [Header("References")]
    public OVRPlayerController playerController;
    public CharacterController characterController;

    [Header("Jump")]
    public bool enableJump = true;
    public OVRInput.Button jumpButton = OVRInput.Button.PrimaryThumbstick;
    public OVRInput.Controller jumpController = OVRInput.Controller.RTouch;

    [Tooltip("跳躍力度")]
    public float jumpForce = 1.0f;

    [Tooltip("重力倍率，越小下落越慢")]
    public float gravityModifier = 0.18f;

    [Header("Crouch Input")]
    public bool enableCrouch = true;
    public bool toggleCrouch = true;

    [Tooltip("蹲下按鍵，右手 B = Button.Two + RTouch")]
    public OVRInput.Button crouchButton = OVRInput.Button.Two;
    public OVRInput.Controller crouchController = OVRInput.Controller.RTouch;

    [Header("Standing")]
    [Tooltip("站立時角色高度")]
    public float standingHeight = 1.7f;

    [Tooltip("站立時正常的 center.y 基準")]
    public float standingCenterY = 0.85f;

    [Tooltip("站立時額外視角下壓量，通常 0")]
    public float standingViewDrop = 0.0f;

    [Header("Crouching")]
    [Tooltip("蹲下時角色高度")]
    public float crouchingHeight = 1.0f;

    [Tooltip("蹲下時額外視角下壓量，值越大蹲得越低")]
    public float crouchingViewDrop = 0.35f;

    [Header("Speed")]
    [Tooltip("蹲下速度")]
    public float crouchDownSpeed = 12f;

    [Tooltip("站起速度")]
    public float standUpSpeed = 4f;

    [Header("Stand Check")]
    [Tooltip("站起來前是否檢查頭頂空間")]
    public bool checkHeadRoom = true;

    [Tooltip("頭頂檢查額外保留距離")]
    public float headCheckMargin = 0.05f;

    public LayerMask headBlockLayers = ~0;

    private bool isCrouching = false;

    private float currentHeight;
    private float currentViewDrop;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<OVRPlayerController>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // 這很重要：讓 OVRPlayerController 用它自己的公式處理高度
        playerController.useProfileData = true;

        // 跳躍手感
        playerController.JumpForce = jumpForce;
        playerController.GravityModifier = gravityModifier;

        // 初始化為站立狀態
        currentHeight = standingHeight;
        currentViewDrop = standingViewDrop;

        ApplyHeightToOVR();
    }

    private void Update()
    {
        HandleJumpInput();
        HandleCrouchInput();
        UpdateCrouchState();
    }

    private void HandleJumpInput()
    {
        if (!enableJump || playerController == null) return;

        if (OVRInput.GetDown(jumpButton, jumpController))
        {
            playerController.JumpForce = jumpForce;
            playerController.GravityModifier = gravityModifier;

            bool ok = playerController.Jump();
            Debug.Log("Jump pressed, success = " + ok);
        }
    }

    private void HandleCrouchInput()
    {
        if (!enableCrouch) return;

        if (toggleCrouch)
        {
            if (OVRInput.GetDown(crouchButton, crouchController))
            {
                if (isCrouching)
                {
                    if (CanStandUp())
                    {
                        isCrouching = false;
                        Debug.Log("Stand up");
                    }
                    else
                    {
                        Debug.Log("Blocked overhead, cannot stand");
                    }
                }
                else
                {
                    isCrouching = true;
                    Debug.Log("Crouch down");
                }
            }
        }
        else
        {
            bool wantCrouch = OVRInput.Get(crouchButton, crouchController);

            if (wantCrouch)
            {
                isCrouching = true;
            }
            else
            {
                if (CanStandUp())
                    isCrouching = false;
            }
        }
    }

    private void UpdateCrouchState()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        float targetViewDrop = isCrouching ? crouchingViewDrop : standingViewDrop;
        float speed = isCrouching ? crouchDownSpeed : standUpSpeed;

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * speed);
        currentViewDrop = Mathf.Lerp(currentViewDrop, targetViewDrop, Time.deltaTime * speed);

        currentHeight = Mathf.Max(0.2f, currentHeight);

        ApplyHeightToOVR();
    }

    private void ApplyHeightToOVR()
    {
        if (characterController == null) return;

        characterController.height = currentHeight;

        Vector3 c = characterController.center;

        // 關鍵：
        // 站立時維持正常 center.y 基準
        // 高度改變時補上一半高度差
        // 最後再扣掉 crouch 的 viewDrop
        float baseCenterY = standingCenterY + (currentHeight - standingHeight) * 0.5f;
        c.y = baseCenterY + currentViewDrop;

        characterController.center = c;
    }

    private bool CanStandUp()
    {
        if (!checkHeadRoom) return true;
        if (characterController == null) return true;

        float neededHeight = standingHeight - currentHeight;
        if (neededHeight <= 0.01f) return true;

        float radius = Mathf.Max(0.05f, characterController.radius * 0.95f);

        Vector3 worldCenter = transform.TransformPoint(characterController.center);
        Vector3 castOrigin = worldCenter + Vector3.up * (characterController.height * 0.5f - radius);

        bool blocked = Physics.SphereCast(
            castOrigin,
            radius,
            Vector3.up,
            out RaycastHit hit,
            neededHeight + headCheckMargin,
            headBlockLayers,
            QueryTriggerInteraction.Ignore
        );

        if (blocked)
        {
            Debug.Log("Cannot stand up, blocked by: " + hit.collider.name);
        }

        return !blocked;
    }

    public void SetCrouching(bool value)
    {
        if (!value)
        {
            if (CanStandUp())
                isCrouching = false;
        }
        else
        {
            isCrouching = true;
        }
    }

    public void ToggleCrouch()
    {
        if (isCrouching)
        {
            if (CanStandUp())
                isCrouching = false;
        }
        else
        {
            isCrouching = true;
        }
    }

    public void JumpNow()
    {
        if (playerController == null) return;

        playerController.JumpForce = jumpForce;
        playerController.GravityModifier = gravityModifier;
        playerController.Jump();
    }
}