using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("XR Head Reference")]
    public Transform head; // 指向 Quest 頭盔 transform

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Continuous Turn Settings")]
    public float turnSpeed = 90f; // 每秒旋轉角度 (可依喜好調整，通常 90~120 較舒適)
    public float turnDeadzone = 0.1f; // 搖桿死區，避免輕微觸碰導致畫面飄移

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("PlayerLocomotion need Rigidbody component. Please add one to the player object.");
        }
    }

    void Update()
    {
        // 以頭盔面向計算移動方向（不影響角色本身 rotation）
        Transform moveRef = head;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Debug.Log(Camera.main.transform.forward);
        Vector3 move = (moveRef.right * h + moveRef.forward * v).normalized;
        Vector3 velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);
        rb.velocity = velocity;

        // 連續轉向：使用右搖桿的水平軸來控制旋轉
        float turnInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
        if (Mathf.Abs(turnInput) > turnDeadzone)
        {
            float turnAmount = turnInput * turnSpeed * Time.deltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAmount, 0f));
        }
    }
}
