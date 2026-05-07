using UnityEngine;

public class PlacedNote : MonoBehaviour
{
    public NoteCompletionManager manager;

    [Header("Audio Settings (Wwise)")]
    [Tooltip("填入這個音符專屬的抓取音效，例如：Play_SFX_Note_Pickup_01")]
    public string pickupEventName = "Play_SFX_Note_Pickup_01";

    private bool counted = false;

    // ==========================================
    // 音效呼叫功能區 (供 Unity 事件綁定)
    // ==========================================

    /// <summary>
    /// 抓起音符時播放 (請綁定在音符的 Pointable Unity Event Wrapper -> When Select)
    /// </summary>
    public void PlayPickupSound()
    {
        AkSoundEngine.PostEvent(pickupEventName, gameObject);
    }

    /// <summary>
    /// 放錯位置時播放 (請綁定在判定放錯的事件上)
    /// </summary>
    public void PlayFailSound()
    {
        AkSoundEngine.PostEvent("Play_SFX_Note_Place_Fail", gameObject);
    }

    // ==========================================
    // 遊戲邏輯區
    // ==========================================

    public void SetPlacedCorrectly()
    {
        if (counted) return;

        counted = true;
        Debug.Log(gameObject.name + " 放置正確");

        // --- 🎵 自動播放放對位置的音效 ---
        AkSoundEngine.PostEvent("Play_SFX_Note_Place_Success", gameObject);

        if (manager != null)
        {
            manager.AddCompletedCount();
        }
        else
        {
            Debug.LogWarning("PlacedNote 的 manager 未設定");
        }
    }

    public void ResetPlacedState()
    {
        counted = false;
    }
}