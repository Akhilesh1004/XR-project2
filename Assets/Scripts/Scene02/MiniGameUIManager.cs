using JetBrains.Rider.Unity.Editor;
using UnityEngine;

public class MiniGameUIManager : MonoBehaviour
{
    public GameObject miniGamePanel;
    public GameObject recorder;
    public RecorderRhythmGameManager miniGame;
    public GameFlow gameFlow;
    public DropAndEnableGrabbable gameDrop;
    void Start()
    {
        if(miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }
    }
    public void OpenMiniGame()
    {
        recorder.GetComponent<Renderer>().enabled = false;
        Debug.Log("Open");
        Transform[] children = gameFlow.locomotor.gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == "CenterEyeAnchor")
            {
                miniGamePanel.GetComponent<Canvas>().worldCamera = t.GetComponent<Camera>();
                break;
            }
        }
        miniGamePanel.SetActive(true);
        Debug.Log("SetActive");
        if (miniGame != null)
        {
            miniGame.StartMiniGame();
            Debug.Log("StartMiniGame");
        }

    }

    public void CloseMiniGame()
    {
        recorder.GetComponent<Renderer>().enabled = true;
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);
        if (miniGame != null)
            miniGame.StopMiniGame();
        gameFlow.locomotor.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameFlow.locomotor.SetHaltUpdateMovement(false); ;
        gameDrop.StartDrop();
    }
}