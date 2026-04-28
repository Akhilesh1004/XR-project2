using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlow1 : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("ª±®a")]
    public Transform playerLocomotion;

    public Vector3 startPosition = new Vector3(-8.7f, -4f, -18.33f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);
    void Start()
    {
        playerLocomotion.position = startPosition;
        playerLocomotion.rotation = Quaternion.Euler(startRotationEuler);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
