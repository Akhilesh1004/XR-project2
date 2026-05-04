using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class DropAndEnableGrabbable : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("往下移動的目標位置")]
    public Transform targetPoint;

    [Tooltip("下降速度，單位: units/second")]
    public float moveSpeed = 0.5f;

    [Tooltip("是否使用世界座標移動到 targetPoint")]
    public bool useWorldPosition = true;

    [Header("Rotation")]
    [Tooltip("每秒繞 Y 軸旋轉幾度")]
    public float rotateSpeed = 90f;

    [Header("Enable On Arrive")]
    public Grabbable grabbable;
    public Rigidbody targetRigidbody;
    public List<Collider> targetColliders = new List<Collider>();

    [Header("Optional")]
    [Tooltip("到達目標後，是否停止旋轉")]
    public bool stopRotateWhenArrived = false;

    [Tooltip("到達判定誤差")]
    public float arriveThreshold = 0.01f;

    [Header("Start Control")]
    [Tooltip("勾選的話，遊戲一開始就自動開始下降；不勾則需手動呼叫 StartDrop()")]
    public bool startOnAwake = false;

    private bool hasStartedDrop = false;
    private bool hasArrived = false;

    private void Awake()
    {
        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (targetRigidbody == null)
        {
            targetRigidbody = GetComponent<Rigidbody>();
        }

        if (targetColliders.Count == 0)
        {
            Collider[] cols = GetComponents<Collider>();
            targetColliders.AddRange(cols);
        }

        DisableGrabComponentsAtStart();

        if (startOnAwake)
        {
            hasStartedDrop = true;
        }
    }

    private void Update()
    {
        if (!hasStartedDrop) return;

        if (!hasArrived)
        {
            RotateObject();
            MoveToTarget();

            if (CheckArrived())
            {
                ArriveAtTarget();
            }
        }
        else
        {
            if (!stopRotateWhenArrived)
            {
                RotateObject();
            }
        }
    }

    public void StartDrop()
    {
        if (hasStartedDrop) return;

        hasStartedDrop = true;
        Debug.Log($"{gameObject.name} 開始下降");
    }

    public void ResetDropState()
    {
        hasStartedDrop = false;
        hasArrived = false;

        DisableGrabComponentsAtStart();
        Debug.Log($"{gameObject.name} 已重置下降狀態");
    }

    private void RotateObject()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }

    private void MoveToTarget()
    {
        if (targetPoint == null) return;

        if (useWorldPosition)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetPoint.localPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private bool CheckArrived()
    {
        if (targetPoint == null) return false;

        if (useWorldPosition)
        {
            return Vector3.Distance(transform.position, targetPoint.position) <= arriveThreshold;
        }
        else
        {
            return Vector3.Distance(transform.localPosition, targetPoint.localPosition) <= arriveThreshold;
        }
    }

    private void ArriveAtTarget()
    {
        hasArrived = true;

        if (targetPoint != null)
        {
            if (useWorldPosition)
            {
                transform.position = targetPoint.position;
            }
            else
            {
                transform.localPosition = targetPoint.localPosition;
            }
        }

        EnableGrabComponents();
        Debug.Log($"{gameObject.name} 已到達目標位置並開啟抓取元件");
    }

    private void DisableGrabComponentsAtStart()
    {
        if (grabbable != null)
        {
            grabbable.enabled = false;
        }

        if (targetRigidbody != null)
        {
            targetRigidbody.isKinematic = true;
            targetRigidbody.detectCollisions = false;
        }

        for (int i = 0; i < targetColliders.Count; i++)
        {
            if (targetColliders[i] != null)
            {
                targetColliders[i].enabled = false;
            }
        }
    }

    private void EnableGrabComponents()
    {
        if (targetRigidbody != null)
        {
            targetRigidbody.isKinematic = false;
            targetRigidbody.detectCollisions = true;
        }

        for (int i = 0; i < targetColliders.Count; i++)
        {
            if (targetColliders[i] != null)
            {
                targetColliders[i].enabled = true;
            }
        }

        if (grabbable != null)
        {
            grabbable.enabled = true;
        }
    }
}