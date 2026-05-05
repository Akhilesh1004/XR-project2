using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
public class RotateWhenNotGrabbed : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotateSpeed = 60f;

    [Header("Support Check")]
    public float checkDistance = 0.15f;
    public LayerMask supportLayer = ~0; // 預設偵測所有 Layer

    [Header("Grab")]
    public Grabbable grabbable;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (grabbable == null)
            grabbable = GetComponent<Grabbable>();
    }

    private void FixedUpdate()
    {
        if (grabbable == null) return;

        bool isGrabbed = grabbable.SelectingPointsCount > 0;
        bool hasSupportBelow = Physics.Raycast(
            transform.position,
            Vector3.down,
            checkDistance,
            supportLayer,
            QueryTriggerInteraction.Ignore
        );

        if (isGrabbed)
        {
            rb.constraints = RigidbodyConstraints.None;
            return;
        }

        RigidbodyConstraints constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (hasSupportBelow)
        {
            constraints |= RigidbodyConstraints.FreezePositionX |
                           RigidbodyConstraints.FreezePositionY |
                           RigidbodyConstraints.FreezePositionZ;
        }

        rb.constraints = constraints;

        float newY = rb.rotation.eulerAngles.y + rotateSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.Euler(0f, newY, 0f));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * checkDistance
        );
    }
}