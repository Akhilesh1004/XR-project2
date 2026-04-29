using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ColorTriggerByRotation : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("ID")]
    public string puzzleID = "MainHub_ColorPuzzle";

    [Header("Environment Settings")]
    public Volume globalVolume;      // Assign your Global Volume here
    public float transitionSpeed = 30f; // Color transition speed
    public float revealDuration = 5f;

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

        if (PlayerPrefs.GetInt(puzzleID, 0) == 1)
        {
            Debug.Log($"{puzzleID} already solved! Restoring color and revealing objects immediately.");

            // Directly set color to full color (0)
            if (colorAdjustments != null) colorAdjustments.saturation.Override(0f);

            // Directly open all hidden objects (portals), this will trigger their OnEnable()
            if (objectsToReveal != null)
            {
                foreach (GameObject obj in objectsToReveal)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            hasRevealed = true;
            this.enabled = false;
            return;
        }

        if (colorAdjustments != null) colorAdjustments.saturation.Override(-100f);

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

    private IEnumerator FadeInObject(GameObject obj)
    {
        obj.SetActive(true);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        float timer = 0f;

        while (timer < revealDuration)
        {
            float alpha = Mathf.SmoothStep(0f, 1f, timer / revealDuration);

            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(block);

                if (r.sharedMaterial.HasProperty("_Color"))
                {
                    Color c = r.sharedMaterial.color;
                    c.a = alpha;
                    block.SetColor("_Color", c);
                }

                r.SetPropertyBlock(block);
            }

            timer += Time.deltaTime;
            yield return null;
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

        PlayerPrefs.SetInt(puzzleID, 1);
        PlayerPrefs.Save();
        Debug.Log($"Puzzle state saved: {puzzleID} = 1");

        hasRevealed = true;
        this.enabled = false;
    }
}