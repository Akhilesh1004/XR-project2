using Oculus.Interaction;
using UnityEngine;

public class OpenMiniGameOnGrab : MonoBehaviour
{
    public Grabbable grabbable;
    public MiniGameUIManager uiManager;

    private bool opened = false;

    void Update()
    {
        if (opened) return;

        if (grabbable.SelectingPointsCount > 0 && uiManager != null)
        {
            opened = true;
            Debug.Log("Grab");
            uiManager.OpenMiniGame();
            
        }
    }
}