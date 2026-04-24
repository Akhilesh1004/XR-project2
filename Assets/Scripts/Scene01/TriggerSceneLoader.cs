using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSceneLoader : MonoBehaviour
{
    [Header("Scene Trigger Settings")]
    [Tooltip("Name of the target scene to switch to")]
    public string targetSceneName;

    [Tooltip("Only objects with this Tag will trigger the scene change (usually the Player)")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that collided with this trigger is the player
        if (other.CompareTag(playerTag))
        {
            // Ensure the target scene name is not empty
            if (!string.IsNullOrWhiteSpace(targetSceneName))
            {
                SceneManager.LoadSceneAsync(targetSceneName);
            }
            else
            {
                Debug.LogWarning("TriggerSceneLoader: 請在 Inspector 中填寫目標場景名稱！");
            }
        }
    }
}