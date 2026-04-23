using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PuzzleResultHandler : MonoBehaviour
{
    [Header("Puzzle Result Settings")]
    [Tooltip("Set to True if the player's action is correct, or False if it's wrong. This will be set by the GeneralSocketTrigger's UnityEvent.")]
    public bool isCorrect = false;

    [Header("Actions on Correct Placement (True)")]
    //public AudioSource audioToStop;
    public string nextSceneName;

    [Header("Actions on Incorrect Placement (False)")]
    public Volume globalVolume;
    public Color wrongColor = new Color(1f, 0.2f, 0.2f);
    private ColorAdjustments colorAdjustments;
    private Color originalColor;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            originalColor = colorAdjustments.colorFilter.value;
        }
        else
        {
            Debug.LogWarning("找不到 Color Adjustments，請確認你的 Global Volume 有加入這個 Override！");
        }
    }

    public void CheckResult()
    {
        Debug.Log("Check1");
        if (isCorrect)
        {
            HandleCorrectAction();
        }
        else
        {
            StartCoroutine(HandleWrongAction());
        }
    }


    private void HandleCorrectAction()
    {
        Debug.Log("Correct action!");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Incorrect action is set to True, but nextSceneName is empty! Please assign a scene name to load.");
        }
    }

    private IEnumerator HandleWrongAction()
    {
        Debug.Log("Wrong action! Starting punishment...");

        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.value = wrongColor;

            yield return new WaitForSeconds(5f);

            colorAdjustments.colorFilter.value = originalColor;
            Debug.Log("Punishment ended, screen restored.");
        }
    }
}