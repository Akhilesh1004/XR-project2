using UnityEngine;

public class RevealByRotationZ : MonoBehaviour
{
    [Header("rotation settings")]
    [Tooltip("The object that is being rotated (usually the empty object this script is attached to)")]
    public Transform rotatableItem;

    [Tooltip("How many degrees of Z-axis rotation to trigger?")]
    public float triggerAngle = 90f; // Default to 90 degrees

    [Header("reveal settings")]
    [Tooltip("The hidden object to reveal when the rotation threshold is met")]
    public GameObject objectToReveal;

    private float initialRotationZ;
    private bool hasTriggered = false;

    void Start()
    {
        if (rotatableItem != null)
        {
            initialRotationZ = rotatableItem.localEulerAngles.z;
        }

        if (objectToReveal != null)
        {
            objectToReveal.SetActive(false);
        }
    }

    void Update()
    {
        // If not triggered yet and all objects are set, keep detecting
        if (!hasTriggered && rotatableItem != null && objectToReveal != null)
        {
            // Get the current Z-axis rotation
            float currentRotationZ = rotatableItem.localEulerAngles.z;

            // Calculate the true rotation difference (to avoid the 0 and 360 degree jump bug)
            float rotatedAmount = Mathf.Abs(Mathf.DeltaAngle(initialRotationZ, currentRotationZ));

            // If the rotated amount reaches the trigger angle
            if (rotatedAmount >= triggerAngle)
            {
                hasTriggered = true; // Lock the switch to avoid repeated triggering

                // Reveal the specified object!
                objectToReveal.SetActive(true);
                
                Debug.Log("Rotation threshold met! Revealing the object.");
            }
        }
    }
}