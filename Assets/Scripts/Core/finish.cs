using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class finish : MonoBehaviour
{
    // Start is called before the first frame update
    public DistanceMoveAndSwap a;
    public DistanceMoveAndSwap b;
    public DistanceMoveAndSwap c;
    public string sceneName;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(a.hasTriggered && b.hasTriggered && c.hasTriggered)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                a.hasTriggered = false;
            }
        }
    }
}
