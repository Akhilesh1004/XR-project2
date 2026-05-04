using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnGrab : MonoBehaviour
{
    public string sceneName;

    public void OnGrab()
    {

        Debug.Log("Grab detected");

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}