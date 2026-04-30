using UnityEngine;

public class MagicSpriteAudioController : MonoBehaviour
{
    [Header("Wwise RTPC 名稱")]
    public string rtpcName = "Player_Hand_Height";

    [Header("追蹤的手把")]
    public OVRInput.Controller targetController = OVRInput.Controller.RTouch;

    [Header("坐姿高度補償 (數字越大，手不用放太低就變悶)")]
    public float heightOffset = 0.6f; // 預設幫你把虛擬地板墊高 60 公分

    void Update()
    {
        // 抓取右手把的真實高度
        float rawHeight = OVRInput.GetLocalControllerPosition(targetController).y;

        // 扣掉坐姿補償 (原本手在 0.6 公尺，現在程式會把它當作 0)
        float adjustedHeight = rawHeight - heightOffset;

        // 加大變化幅度，並確保數值乖乖待在 0 到 2 之間
        float finalRTPC = Mathf.Clamp(adjustedHeight * 2.0f, 0f, 2f);

        // 傳給 Wwise！
        AkSoundEngine.SetRTPCValue(rtpcName, finalRTPC, gameObject);
    }
}