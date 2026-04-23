using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleporter : MonoBehaviour
{
    [Header("portal settings")]
    [Tooltip("teleport to this scene when player enters the portal")]
    public string targetSceneName; 

    [Tooltip("give this portal a unique ID (e.g. Portal_1)")]
    public string portalID;        

    [Header("Object Control")]
    [Tooltip("If already cleared, which object to hide? (Please drag the portal model or parent object)")]
    public GameObject objectToHide;

    [Header("Fail-Safe Settings")]
    [Tooltip("Only objects with specific tags will trigger (default is Player)")]
    public string triggerTag = "Player";

    void OnEnable()
    {
        if (PlayerPrefs.GetInt(portalID, 0) == 1)
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
                Debug.Log($"{portalID} already cleared, hiding the object as per your logic.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Fail-safe mechanism: Check if the object entering the portal is the player (to prevent, for example, a cube from being teleported)
        if (other.CompareTag(triggerTag))
        {
            Debug.Log($"Player entered the portal! Saving record for {portalID} and preparing to teleport...");

            // 1. Write to memory according to your logic: Set the ID of this portal to 1
            PlayerPrefs.SetInt(portalID, 1);
            PlayerPrefs.Save(); 

            // 2. Execute scene transition
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("You forgot to fill in the target scene name!");
            }
        }
    }
}