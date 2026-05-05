using UnityEngine;
using TMPro;

public class NoteCompletionManager : MonoBehaviour
{
    [Header("Count")]
    public int totalNoteCount = 0;
    public int completedCount = 0;

    [Header("UI")]
    public TMP_Text progressText;
    public TMP_Text resultText;

    [Header("Mini Game")]
    public MiniGameUIManager uiManager;

    private bool gameFinished = false;

    private void Start()
    {
        UpdateUI();
    }

    public void AddCompletedCount()
    {
        if (gameFinished) return;

        completedCount++;
        Debug.Log("Complete: " + completedCount + " / " + totalNoteCount);

        UpdateUI();
        CheckGameFinish();
    }
    public void SubCompletedCount()
    {
        if (gameFinished) return;

        completedCount--;
        Debug.Log("Complete: " + completedCount + " / " + totalNoteCount);

        UpdateUI();
        CheckGameFinish();
    }

    public void ResetCount()
    {
        completedCount = 0;
        gameFinished = false;
        UpdateUI();

        if (resultText != null)
        {
            resultText.text = "";
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = "Complete: " + completedCount + " / " + totalNoteCount;
        }
    }

    private void CheckGameFinish()
    {
        if (gameFinished) return;
        if (totalNoteCount <= 0) return;

        if (completedCount >= totalNoteCount)
        {
            gameFinished = true;

            Debug.Log("全部 note 都已放到正確位置，小遊戲結束");

            if (resultText != null)
            {
                resultText.text = "Success";
            }

            if (uiManager != null)
            {
                uiManager.CloseMiniGame();
            }
        }
    }
}