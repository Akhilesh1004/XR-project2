using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow1 : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("ª±®a")]
    public OVRPlayerController locomotor;

    public Vector3 startPosition = new Vector3(-8.7f, -4f, -18.33f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);
    private void Start()
    {
        Debug.Log("Now Scene: " + SceneManager.GetActiveScene().name);

        if (SceneManager.GetActiveScene().name == "SubScene_01")
        {
            Scene1Start();
        }
    }
    void Scene1Start()
    {
        locomotor = FindObjectOfType<OVRPlayerController>();
        locomotor.transform.position = startPosition;
        locomotor.transform.rotation = Quaternion.Euler(startRotationEuler);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
