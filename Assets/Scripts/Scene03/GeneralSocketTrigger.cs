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

    [Header("Wrong Action (Color)")]
    public Volume globalVolume;
    public Color wrongColor = new Color(1f, 0.2f, 0.2f);

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

            if (objectToActivate != null)
                objectToActivate.SetActive(true);
        }

        else if (wrongObjects.Contains(incomingObject))
        {
            Debug.Log("Wrong object entered");

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