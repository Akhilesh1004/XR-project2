using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GeneralSocketTrigger : MonoBehaviour
{
    [Header("Socket Settings")]
    public List<GameObject> correctObjects;
    public List<GameObject> wrongObjects;

    [Header("Correct Action")]
    public GameObject objectToActivate;

    [Header("Wrong Action (Color & Audio)")]
    public Volume globalVolume;
    public Color wrongColor = new Color(1f, 0.2f, 0.2f);
    public string failEventName = "Play_SFX_Doll_Bad_Scream"; // 放錯時尖叫

    private ColorAdjustments colorAdjustments;
    private Color originalColor;

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            originalColor = colorAdjustments.colorFilter.value;
        }
        if (objectToActivate != null)
            objectToActivate.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject incomingObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        if (correctObjects.Contains(incomingObject))
        {
            Debug.Log("Correct object entered");

            // --- 🎵 停止好娃娃的哭聲 ---
            BearAudioController bearAudio = incomingObject.GetComponent<BearAudioController>();
            if (bearAudio != null) bearAudio.StopGoodBearCry();

            if (objectToActivate != null)
                objectToActivate.SetActive(true);
        }
        else if (wrongObjects.Contains(incomingObject))
        {
            Debug.Log("Wrong object entered");

            // --- 🎵 壞娃娃放錯位置，發出尖叫 ---
            AkSoundEngine.PostEvent(failEventName, gameObject);

            if (colorAdjustments != null)
                colorAdjustments.colorFilter.value = wrongColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject exitingObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        if (wrongObjects.Contains(exitingObject))
        {
            Debug.Log("Wrong object exited");

            if (colorAdjustments != null)
                colorAdjustments.colorFilter.value = originalColor;
        }
    }
}