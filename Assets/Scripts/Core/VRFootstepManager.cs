using UnityEngine;

public class VRFootstepManager : MonoBehaviour
{
    [Header("追蹤設定")]
    [Tooltip("請把 VR 頭盔 (CenterEyeAnchor 或 Main Camera) 拖進來")]
    public Transform playerCamera;

    [Tooltip("移動多遠算一步 (建議 0.4 到 0.6)")]
    public float stepDistance = 0.5f;

    private Vector3 lastPosition;
    private float lastStepTime;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;
        lastPosition = playerCamera.position;
        lastPosition.y = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TriggerFootstep();

        if (playerCamera == null) return;

        Vector3 currentPos = playerCamera.position;
        currentPos.y = 0;

        // 判斷移動距離是否超過一步
        if (Vector3.Distance(lastPosition, currentPos) >= stepDistance)
        {
            // 增加 0.3 秒冷卻時間，避免傳送或轉向時產生「連發」的雜音
            if (Time.time - lastStepTime > 0.3f)
            {
                TriggerFootstep();
                lastStepTime = Time.time;
            }
            lastPosition = currentPos;
        }
    }

    void TriggerFootstep()
    {
        // 🌟 升級版：使用 RaycastAll 發射「X光穿透射線」
        RaycastHit[] hits = Physics.RaycastAll(playerCamera.position, Vector3.down, 10.0f);

        string floorTag = "Dream"; // 預設值
        bool hitValidFloor = false;

        // 從打到的所有東西裡，篩選出「真正的地板」
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue; // 忽略傳送門之類的隱形感應區
            if (hit.collider.CompareTag("Player")) continue; // 忽略玩家自己的身體！

            // 只要不是自己、不是感應區，那就是真正的地板了！
            floorTag = hit.collider.tag;
            hitValidFloor = true;
            break;
        }

        if (hitValidFloor)
        {
            switch (floorTag)
            {
                case "Water":
                    AkSoundEngine.SetSwitch("Footstep_Material", "Water", gameObject);
                    break;
                case "BallPit":
                    AkSoundEngine.SetSwitch("Footstep_Material", "BallPit", gameObject);
                    break;
                case "School":
                    AkSoundEngine.SetSwitch("Footstep_Material", "School", gameObject);
                    break;
                case "FoamMat":
                    AkSoundEngine.SetSwitch("Footstep_Material", "FoamMat", gameObject);
                    break;
                default:
                    AkSoundEngine.SetSwitch("Footstep_Material", "Dream", gameObject);
                    break;
            }
        }
        else
        {
            // 如果真的在空中（跳躍）
            AkSoundEngine.SetSwitch("Footstep_Material", "Dream", gameObject);
        }

        AkSoundEngine.PostEvent("Play_Footstep", gameObject);
    }
}