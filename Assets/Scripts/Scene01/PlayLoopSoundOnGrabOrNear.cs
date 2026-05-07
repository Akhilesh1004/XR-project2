using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayLoopSoundOnGrabOrNear : MonoBehaviour
{
    [Header("Grab Detection")]
    public Grabbable grabbable;

    [Header("Near Detection")]
    public Transform targetObject;
    public float nearDistance = 1f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Tooltip("若有指定，播放這個 AudioClip；否則使用 AudioSource 原本的 clip")]
    public AudioClip loopClip;

    [Tooltip("是否在開始時自動找 Grabbable")]
    public bool autoFindGrabbable = true;

    [Tooltip("是否用 2D 距離忽略高度差")]
    public bool ignoreYDistance = false;

    public NoteCompletionManager noteCompletionManager;

    [Header("--- Wwise 音效設定 ---")]
    [Tooltip("請依照音符填入對應的抓取聲，例如 Play_SFX_Note_Pickup_01")]
    public string pickupEventName = "Play_SFX_Note_Pickup_01";
    public string successEventName = "Play_SFX_Note_Place_Success";
    public string failEventName = "Play_SFX_Note_Place_Fail";

    private bool count = false;
    private bool wasGrabbed = false; // 用來記錄上一幀的抓取狀態

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (autoFindGrabbable && grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            if (loopClip != null)
            {
                audioSource.clip = loopClip;
            }
        }
    }

    private void Update()
    {
        // Debug.Log(audioSource.clip.loadState); // 建議註解掉，避免每幀印出造成 VR 卡頓

        bool grabbed = IsGrabbed();
        bool near = IsNearTarget();

        // ==========================================
        // Wwise: 抓取與放開音效判定
        // ==========================================
        if (grabbed && !wasGrabbed)
        {
            // 剛抓起的那一瞬間
            AkSoundEngine.PostEvent(pickupEventName, gameObject);
        }
        else if (!grabbed && wasGrabbed)
        {
            // 剛鬆開手的那一瞬間
            if (!near)
            {
                // 如果鬆開手時，音符不在目標區域內 -> 放錯了
                AkSoundEngine.PostEvent(failEventName, gameObject);
            }
        }
        wasGrabbed = grabbed; // 紀錄狀態供下一幀比對


        // ==========================================
        // 原本的 Loop 音效邏輯
        // ==========================================
        if (grabbed || near)
        {
            // Debug.Log("START");
            StartLoopSound();
        }
        else
        {
            // Debug.Log("STOP");
            StopLoopSound();
        }

        // ==========================================
        // 遊戲計分與 Wwise: 放對音效判定
        // ==========================================
        if (near && !count)
        {
            count = true;
            noteCompletionManager.AddCompletedCount();

            // 剛進入正確範圍 -> 放對了
            AkSoundEngine.PostEvent(successEventName, gameObject);
        }

        if (!near && count)
        {
            count = false;
            noteCompletionManager.SubCompletedCount();
        }
    }

    private bool IsGrabbed()
    {
        if (grabbable == null) return false;

        return grabbable.SelectingPointsCount > 0;
    }

    private bool IsNearTarget()
    {
        if (targetObject == null) return false;

        Vector3 a = transform.position;
        Vector3 b = targetObject.position;

        if (ignoreYDistance)
        {
            a.y = 0f;
            b.y = 0f;
        }

        float dist = Vector3.Distance(a, b);
        // Debug.Log(dist); // 建議註解掉，避免造成 VR 卡頓
        return dist <= nearDistance;
    }

    private void StartLoopSound()
    {
        if (audioSource == null) return;
        if (audioSource.clip == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            // Debug.Log(audioSource.isPlaying);
        }
    }

    private void StopLoopSound()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (targetObject == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetObject.position, nearDistance);
    }
}