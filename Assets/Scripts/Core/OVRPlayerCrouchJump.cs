using UnityEngine;

[RequireComponent(typeof(OVRPlayerController))]
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerCrouchJump : MonoBehaviour
{
    [Header("References")]
    public OVRPlayerController playerController;
    public CharacterController characterController;

    [Header("Jump")]
    public OVRInput.Button jumpButton = OVRInput.Button.One;
    public OVRInput.Controller jumpController = OVRInput.Controller.RTouch;

    [Tooltip("跳躍力度，越大跳越高")]
    public float jumpForce = 1.0f;

    [Tooltip("重力倍率，越小下落越慢")]
    public float gravityModifier = 0.18f;

    [Header("Crouch Input")]
    public OVRInput.Button crouchButton = OVRInput.Button.Two;
    public OVRInput.Controller crouchController = OVRInput.Controller.RTouch;

    [Tooltip("true = 按一下切換蹲/站；false = 按住蹲下，放開站起")]
    public bool toggleCrouch = true;

    [Header("Standing State")]
    [Tooltip("站立時 CharacterController 高度")]
    public float standingHeight = 1.7f;

    [Tooltip("站立時 CharacterController center.y")]
    public float standingCenterY = 0.85f;

    [Header("Crouching State")]
    [Tooltip("蹲下時 CharacterController 高度")]
    public float crouchingHeight = 1.0f;

    [Tooltip("蹲下時 CharacterController center.y")]
    public float crouchingCenterY = 0.5f;

    [Header("Visual / Position Effect")]
    [Tooltip("蹲下時角色根物件總共額外往下位移多少，形成自動下沉效果")]
    public float crouchDropDistance = 0.4f;

    [Tooltip("蹲下時下沉速度")]
    public float crouchDownSpeed = 12f;

    [Tooltip("站起來時恢復速度")]
    public float standUpSpeed = 4f;

    [Header("Options")]
    [Tooltip("是否在站起時檢查頭頂空間，避免頂到天花板還硬站起來")]
    public bool checkHeadRoom = true;

    [Tooltip("頭頂空間檢查額外距離")]
    public float headCheckMargin = 0.05f;

    [Tooltip("頭頂空間檢查 layer mask")]
    public LayerMask headBlockLayers = ~0;

    private bool isCrouching = false;

    // 目前已套用的額外下沉量
    private float currentDropOffset = 0f;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<OVRPlayerController>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // 初始化跳躍參數
        playerController.JumpForce = jumpForce;
        playerController.GravityModifier = gravityModifier;

        // 初始化站立狀態
        characterController.height = standingHeight;

        Vector3 center = characterController.center;
        center.y = standingCenterY;
        characterController.center = center;
    }

    private void Update()
    {
        HandleJump();
        HandleCrouchInput();
        UpdateCrouchState();
    }

    private void HandleJump()
    {
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
        if (toggleCrouch)
        {
            if (OVRInput.GetDown(crouchButton, crouchController))
            {
                if (isCrouching)
                {
                    // 想站起來時先檢查頭頂空間
                    if (CanStandUp())
                    {
                        isCrouching = false;
                    }
                }
                else
                {
                    isCrouching = true;
                }

                Debug.Log("Toggle crouch = " + isCrouching);
            }
        }
        else
        {
            bool wantCrouch = OVRInput.Get(crouchButton, crouchController);

            if (!wantCrouch && isCrouching)
            {
                // 放開想站起來時，先檢查頭頂空間
                if (CanStandUp())
                {
                    isCrouching = false;
                }
            }
            else if (wantCrouch)
            {
                isCrouching = true;
            }
        }
    }

    private void UpdateCrouchState()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        float targetCenterY = isCrouching ? crouchingCenterY : standingCenterY;

        // 蹲下和站起用不同速度
        float shapeSpeed = isCrouching ? crouchDownSpeed : standUpSpeed;

        // 1. 調整 collider 高度
        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            Time.deltaTime * shapeSpeed
        );

        Vector3 center = characterController.center;
        center.y = Mathf.Lerp(center.y, targetCenterY, Time.deltaTime * shapeSpeed);
        characterController.center = center;

        // 2. 調整角色根物件座標，形成自動下沉 / 緩慢起立
        float targetDropOffset = isCrouching ? crouchDropDistance : 0f;
        float moveSpeed = isCrouching ? crouchDownSpeed : standUpSpeed;

        float newDropOffset = Mathf.Lerp(
            currentDropOffset,
            targetDropOffset,
            Time.deltaTime * moveSpeed
        );

        // 只套用這一幀 offset 的差值，所以移動中也能正常使用
        float deltaOffset = newDropOffset - currentDropOffset;
        currentDropOffset = newDropOffset;

        transform.position -= new Vector3(0f, deltaOffset, 0f);
    }

    private bool CanStandUp()
    {
        if (!checkHeadRoom) return true;
        if (characterController == null) return true;

        float neededHeight = standingHeight - characterController.height;
        if (neededHeight <= 0.01f) return true;

        Vector3 worldCenter = transform.TransformPoint(characterController.center);

        float radius = Mathf.Max(0.05f, characterController.radius * 0.95f);
        float castDistance = neededHeight + headCheckMargin;

        // 從目前頭部附近往上檢查有沒有障礙
        Vector3 castOrigin = worldCenter + Vector3.up * (characterController.height * 0.5f - radius);

        bool blocked = Physics.SphereCast(
            castOrigin,
            radius,
            Vector3.up,
            out RaycastHit hit,
            castDistance,
            headBlockLayers,
            QueryTriggerInteraction.Ignore
        );

        if (blocked)
        {
            Debug.Log("Cannot stand up, blocked by: " + hit.collider.name);
            return false;
        }

        return true;
    }

    public void SetCrouching(bool value)
    {
        if (!value)
        {
            if (CanStandUp())
            {
                isCrouching = false;
            }
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
            {
                isCrouching = false;
            }
        }
        else
        {
            isCrouching = true;
        }
    }

    public void JumpNow()
    {
        //playerController.JumpForce = jumpForce;
        //playerController.GravityModifier = gravityModifier;
        playerController.Jump();
    }
}