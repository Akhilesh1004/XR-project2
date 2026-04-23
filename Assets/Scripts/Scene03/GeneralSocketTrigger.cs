using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GeneralSocketTrigger : MonoBehaviour
{
    [Header("Socket Trigger Settings")]
    public List<GameObject> correctObjects;
    public List<GameObject> wrongObjects;

    [Header("Actions on Object Connect")]
    public UnityEvent onObjectPlaced;

    [Header("Actions on Object Wrong")]
    public UnityEvent onObjectRemoved;

    private void OnTriggerEnter(Collider other)
    {
        GameObject incomingObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (correctObjects.Contains(incomingObject))
        {
            Debug.Log("Check2");
            onObjectPlaced?.Invoke();
        } else if (wrongObjects.Contains(incomingObject))
        {
            Debug.Log("Check3");
            onObjectRemoved?.Invoke();
        }
    }
}