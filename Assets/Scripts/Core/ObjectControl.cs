using UnityEngine;

public class ObjectControl : MonoBehaviour
{
    [Header("--- 通用設定 ---")]
    [Tooltip("如果掛腳本的物件自己不會轉，請把『真正會轉的門板/把手』拖進這裡。留空則偵測自己。")]
    public Transform targetToWatch;

    public string portalID;
    public GameObject objectToHide;
    public GameObject objectToShow;

    [Header("--- 門的設定 ---")]
    public GameObject portalObject;
    public float openAngleThreshold = 5f; // 降低門檻，稍微推開 5 度就響

    [Header("--- 音樂盒設定 ---")]
    public bool isMusicBox = false;
    public float rotationThreshold = 0.5f;
    public string musicBoxEventName = "Play_SFX_MusicBox_Crank";

    private bool hasDoorOpened = false;
    private bool isSoundPlaying = false;
    private Quaternion initialRotation;
    private Quaternion lastFrameRotation;

    private void Start()
    {
        Transform activeTarget = targetToWatch != null ? targetToWatch : transform;
        initialRotation = activeTarget.localRotation;
        lastFrameRotation = activeTarget.localRotation;

        if (PlayerPrefs.GetInt(portalID, 0) == 1)
        {
            if (objectToHide != null) objectToHide.SetActive(false);
            if (objectToShow != null) objectToShow.SetActive(true);
        }
    }

    private void Update()
    {
        Transform activeTarget = targetToWatch != null ? targetToWatch : transform;

        // 1. 門的偵測邏輯
        if (!hasDoorOpened && !isMusicBox)
        {
            float angle = Quaternion.Angle(activeTarget.localRotation, initialRotation);
            if (angle > openAngleThreshold)
            {
                TriggerDoorOpenAndPortalHum();
            }
        }

        // 2. 音樂盒「任何軸向」的持續旋轉偵測
        if (isMusicBox)
        {
            // 使用 Quaternion.Angle 可以完美偵測 X, Y, Z 任何角度的變化
            float deltaAngle = Quaternion.Angle(activeTarget.localRotation, lastFrameRotation);
            bool isMoving = (deltaAngle / Time.deltaTime) > rotationThreshold;

            if (isMoving && !isSoundPlaying)
            {
                AkSoundEngine.PostEvent(musicBoxEventName, gameObject);
                isSoundPlaying = true;
                Debug.Log("🎵 [Wwise] 音樂盒開始轉動！觸發: " + musicBoxEventName);
            }
            else if (!isMoving && isSoundPlaying)
            {
                AkSoundEngine.ExecuteActionOnEvent(musicBoxEventName, AkActionOnEventType.AkActionOnEventType_Stop, gameObject, 200);
                isSoundPlaying = false;
                Debug.Log("🔇 [Wwise] 音樂盒停止轉動！");
            }

            lastFrameRotation = activeTarget.localRotation;
        }
    }

    public void PlayMusicBoxCrankSound() { AkSoundEngine.PostEvent(musicBoxEventName, gameObject); }
    public void PlayDoorHandleSound() { AkSoundEngine.PostEvent("Play_SFX_Door_Handle_Turn", gameObject); }

    public void TriggerDoorOpenAndPortalHum()
    {
        if (!hasDoorOpened)
        {
            AkSoundEngine.PostEvent("Play_SFX_Door_Wood_Open", gameObject);
            if (portalObject != null)
            {
                AkSoundEngine.PostEvent("Play_AMB_Portal_Hum_Loop", portalObject);
            }
            hasDoorOpened = true;
            Debug.Log("🚪 [Wwise] 門被推開了！播放木門聲與傳送門嗡嗡聲。");
        }
    }
}