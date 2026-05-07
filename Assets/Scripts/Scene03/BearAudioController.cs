using UnityEngine;
using System.Collections;

public class BearAudioController : MonoBehaviour
{
    [Header("娃娃類型設定")]
    [Tooltip("好娃娃打勾，壞娃娃不要勾")]
    public bool isGoodBear = false;

    [Header("Wwise 音效名稱")]
    public string screamEvent = "Play_SFX_Doll_Bad_Scream";
    public string giggleEvent = "Play_SFX_Doll_Bad_Giggle";
    public string cryEvent = "Play_AMB_Doll_Good_Cry_Loop";

    private bool isGrabbed = false;

    private void Start()
    {
        if (isGoodBear)
        {
            // 好娃娃：一開始就哭
            AkSoundEngine.PostEvent(cryEvent, gameObject);
        }
        else
        {
            // 壞娃娃：開始隨機尖叫
            StartCoroutine(RandomScreamRoutine());
        }
    }

    private IEnumerator RandomScreamRoutine()
    {
        while (true)
        {
            // 隨機等待 15 到 25 秒 (過久一點才響)
            yield return new WaitForSeconds(Random.Range(15f, 25f));

            // 只有在「沒有被抓起」的時候才尖叫
            if (!isGrabbed && !isGoodBear)
            {
                AkSoundEngine.PostEvent(screamEvent, gameObject);
            }
        }
    }

    // ==========================================
    // 給 VR 抓取事件 (When Select / Unselect) 綁定的功能
    // ==========================================

    public void OnGrabbed()
    {
        isGrabbed = true;

        if (!isGoodBear)
        {
            // 抓起壞娃娃：停止尖叫，並發出 Giggle 笑聲
            AkSoundEngine.ExecuteActionOnEvent(screamEvent, AkActionOnEventType.AkActionOnEventType_Stop, gameObject, 100);
            AkSoundEngine.PostEvent(giggleEvent, gameObject);
        }
    }

    public void OnReleased()
    {
        isGrabbed = false;

        if (!isGoodBear)
        {
            // 放下/丟掉壞娃娃：發出 Giggle 笑聲
            AkSoundEngine.PostEvent(giggleEvent, gameObject);
        }
    }

    public void StopGoodBearCry()
    {
        if (isGoodBear)
        {
            AkSoundEngine.ExecuteActionOnEvent(cryEvent, AkActionOnEventType.AkActionOnEventType_Stop, gameObject, 500);
        }
    }
}