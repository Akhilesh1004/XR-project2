using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTrigger : MonoBehaviour
{
    public Volume globalVolume;      // 拖入你的 Global Volume
    public float transitionSpeed = 30f; // 變色速度

    private ColorAdjustments colorAdjustments;
    private bool isTransitioning = false;

    void Start()
    {
        // 取得 Color Adjustments 效果
        globalVolume.profile.TryGet(out colorAdjustments);
    }

    // 當球碰到平台（這個物件）時觸發
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("music_box"))
        {
            isTransitioning = true;
        }
    }

    void Update()
    {
        if (isTransitioning)
        {
            // 每幀慢慢把飽和度從 -100 調回 0
            float current = colorAdjustments.saturation.value;
            if (current < 0)
            {
                colorAdjustments.saturation.value =
                    Mathf.MoveTowards(current, 0f, transitionSpeed * Time.deltaTime);
            }
            else
            {
                isTransitioning = false; // 到達 0 就停止
            }
        }
    }
}