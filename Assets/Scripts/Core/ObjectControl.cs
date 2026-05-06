using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class ObjectControl : MonoBehaviour
{
    public string portalID;
    public GameObject objectToHide;
    private void Start()
    {
        if (PlayerPrefs.GetInt(portalID, 0) == 1)
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }
        }
    }
}