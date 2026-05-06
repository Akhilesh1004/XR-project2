using UnityEngine;

public class DistanceMoveAndSwap : MonoBehaviour
{
    [Header("Targets")]
    public GameObject objectA;
    public GameObject objectB;

    [Header("Trigger")]
    public float triggerDistance = 1.0f;
    public bool triggerOnlyOnce = true;

    [Header("Move")]
    [Tooltip("A 移到本物件位置所需秒數")]
    public float moveDuration = 1.0f;

    [Tooltip("A 是否也一起旋轉到本物件方向")]
    public bool rotateToTarget = true;

    [Tooltip("旋轉時的速度感，越大越快")]
    public float rotationLerpSpeed = 6f;

    [Header("Options")]
    [Tooltip("如果為 true，一開始會先把 B 關掉")]
    public bool hideBOnStart = true;

    public bool hasTriggered = false;
    private bool isMoving = false;

    private Vector3 moveStartPos;
    private Quaternion moveStartRot;
    private float moveTimer = 0f;

    private void Start()
    {
        if (hideBOnStart && objectB != null)
        {
            objectB.SetActive(false);
        }
    }

    private void Update()
    {
        if (objectA == null || objectB == null) return;

        if (isMoving)
        {
            UpdateMove();
            return;
        }

        if (triggerOnlyOnce && hasTriggered) return;

        float distance = Vector3.Distance(transform.position, objectA.transform.position);

        if (distance <= triggerDistance)
        {
            StartMove();
        }
    }

    private void StartMove()
    {
        if (objectA == null) return;

        hasTriggered = true;
        isMoving = true;
        moveTimer = 0f;

        moveStartPos = objectA.transform.position;
        moveStartRot = objectA.transform.rotation;

        Debug.Log("A 物件開始移向本物件");
    }

    private void UpdateMove()
    {
        if (objectA == null) return;

        moveTimer += Time.deltaTime;
        float t = Mathf.Clamp01(moveTimer / moveDuration);

        // 位置：用 SmoothStep 做出比線性更柔和的靠近感
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // 如果起點和終點都不是零向量，可以用 Slerp 做出弧線/球面插值感
        // 否則退回 Lerp，避免 Slerp 在零向量附近不穩定
        Vector3 start = moveStartPos;
        Vector3 end = transform.position;

        if (start != Vector3.zero && end != Vector3.zero)
        {
            objectA.transform.position = Vector3.Slerp(start, end, smoothT);
        }
        else
        {
            objectA.transform.position = Vector3.Lerp(start, end, smoothT);
        }

        // 旋轉：慢慢轉向本物件方向
        if (rotateToTarget)
        {
            Quaternion targetRot = transform.rotation;
            objectA.transform.rotation = Quaternion.Slerp(
                moveStartRot,
                targetRot,
                smoothT
            );
        }

        if (t >= 1f)
        {
            FinishSwap();
        }
    }

    private void FinishSwap()
    {
        isMoving = false;

        if (objectA != null)
        {
            objectA.SetActive(false);
        }

        if (objectB != null)
        {
            objectB.SetActive(true);
        }

        Debug.Log("A 物件已消失，B 物件已出現");
    }
}