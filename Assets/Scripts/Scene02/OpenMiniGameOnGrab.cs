using UnityEngine;

public class OpenMiniGameOnGrab : MonoBehaviour
{
    public MiniGameUIManager uiManager;
    private bool opened = false;

    public void TriggerMiniGame()
    {
        if (opened) return;
        opened = true;

        if (uiManager != null)
        {
            Debug.Log("Grab");
            uiManager.OpenMiniGame();
            
        }
    }
}