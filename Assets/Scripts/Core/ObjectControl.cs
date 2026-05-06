using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class ObjectControl : MonoBehaviour
{
    public string portalID;
    public GameObject objectToHide;
    public GameObject objectToShow;
    private void Start()
    {
        if (PlayerPrefs.GetInt(portalID, 0) == 1)
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }
            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
            }
        }
    }
}