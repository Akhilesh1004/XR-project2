using UnityEngine;

public class MiniGameUIManager : MonoBehaviour
{
    public GameObject miniGamePanel;
    void Start()
    {
        if(miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }
    }
    public void OpenMiniGame()
    {
        Debug.Log("Open");
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(true);
            Debug.Log("SetActive");
        }
            
    }

    public void CloseMiniGame()
    {
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);
    }
}