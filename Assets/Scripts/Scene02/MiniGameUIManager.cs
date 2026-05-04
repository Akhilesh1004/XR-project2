using UnityEngine;

public class MiniGameUIManager : MonoBehaviour
{
    public GameObject miniGamePanel;
    public GameObject recorder;
    public RecorderRhythmGameManager miniGame;
    public GameFlow gameFlow;

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
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(true);
            Debug.Log("SetActive");
        }
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
        gameFlow.locomotor.Velocity = Vector3.zero;
        gameFlow.locomotor.enabled = true;
    }
}