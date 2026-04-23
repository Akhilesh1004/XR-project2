using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTriggerByRotation : MonoBehaviour
{
    [Header("Environment Settings")]
    public Volume globalVolume;      // Assign your Global Volume here
    public float transitionSpeed = 30f; // Color transition speed

    [Header("Rotation Trigger Settings")]
    public Transform rotatableItem;     // Assign your "rotatable object" here
    public float triggerAngle = 45f;    // Rotation threshold to trigger color change
    public GameObject[] objectsToReveal;

    private ColorAdjustments colorAdjustments;
    private bool isTransitioning = false;
    private bool hasRevealed = false;
    private float initialRotationX;

    void Start()
    {
        // Get the Color Adjustments effect
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        // Record the initial X-axis rotation of the object at the start of the game
        if (rotatableItem != null)
        {
            initialRotationX = rotatableItem.localEulerAngles.x;
        }

        if (objectsToReveal != null)
        {
            foreach (GameObject obj in objectsToReveal)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 1. If not yet started color transition, keep monitoring rotation angle
        if (!isTransitioning && rotatableItem != null)
        {
            float currentRotationX = rotatableItem.localEulerAngles.x;

            // Use Mathf.DeltaAngle to calculate "angle difference"
            // This is a trick to avoid bugs when Unity's angles jump between 0 and 360
            float rotatedAmount = Mathf.Abs(Mathf.DeltaAngle(initialRotationX, currentRotationX));

            // If the rotated amount exceeds the set threshold (e.g., 45 degrees)
            if (rotatedAmount >= triggerAngle)
            {
                isTransitioning = true;
                Debug.Log("Rotation threshold met! Starting color restoration!");
            }
        }

        // 2. Original color transition logic (automatically executes once triggered)
        if (isTransitioning && colorAdjustments != null)
        {
            float current = colorAdjustments.saturation.value;
            
            if (current < 0f)
            {
                float newValue = Mathf.MoveTowards(current, 0f, transitionSpeed * Time.deltaTime);
                colorAdjustments.saturation.Override(newValue);
                
                Debug.Log($"color change: {newValue}");
            }
            else
            {
                colorAdjustments.saturation.Override(0f);
                isTransitioning = false; 
                this.enabled = false;
                RevealAllObjects();
                Debug.Log("color fully restored! Transition complete.");
            }
        }
    }

    private void RevealAllObjects()
    {
        if (hasRevealed) return;

        Debug.Log("color fully restored! Revealing all hidden objects!");
        
        if (objectsToReveal != null)
        {
            foreach (GameObject obj in objectsToReveal)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        hasRevealed = true;
        this.enabled = false;
    }
}