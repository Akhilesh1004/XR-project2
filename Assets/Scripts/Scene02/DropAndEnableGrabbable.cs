using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class DropAndEnableGrabbable : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("���U���ʪ��ؼЦ�m")]
    public Transform targetPoint;

    [Tooltip("�U���t�סA���: units/second")]
    public float moveSpeed = 0.5f;

    [Tooltip("�O�_�ϥΥ@�ɮy�в��ʨ� targetPoint")]
    public bool useWorldPosition = true;

    [Header("Rotation")]
    [Tooltip("�C��¶ Y �b����X��")]
    public float rotateSpeed = 90f;

    [Header("Enable On Arrive")]
    public Grabbable grabbable;
    public Rigidbody targetRigidbody;
    public List<Collider> targetColliders = new List<Collider>();

    [Header("Optional")]
    [Tooltip("��F�ؼЫ�A�O�_�������")]
    public bool stopRotateWhenArrived = false;

    [Tooltip("��F�P�w�~�t")]
    public float arriveThreshold = 0.01f;

    [Header("Start Control")]
    [Tooltip("�Ŀ諸�ܡA�C���@�}�l�N�۰ʶ}�l�U���F���īh�ݤ�ʩI�s StartDrop()")]
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
        Debug.Log($"{gameObject.name} start drop");
    }

    public void ResetDropState()
    {
        hasStartedDrop = false;
        hasArrived = false;

        DisableGrabComponentsAtStart();
        Debug.Log($"{gameObject.name} reset drop state");
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
        Debug.Log($"{gameObject.name} arrived at target");
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