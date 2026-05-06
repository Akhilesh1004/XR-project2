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
    public GameObject final_door;
    void Start()
    {
        final_door.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(a.hasTriggered && b.hasTriggered && c.hasTriggered)
        {
            final_door.SetActive(true);
        }
    }
}
