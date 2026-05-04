using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public Vector3 startPosition = new Vector3(-30f, 1f, 18f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);
    public Vector3 targetPosition = new Vector3(19f, 3.561244f, 36.734f);
    public Vector3 targetRotationEuler = new Vector3(0f, 90f, 0f);

    [Header("設定")]
    public float timeLimit = 180f;
    public float arriveDistance = 1.5f;
    public OVRPlayerController locomotor;

    public float autoMoveSpeed = 2f;
    public float autoTurnSpeed = 2f;

    private UnityEngine.CharacterController cc;

    private bool hasArrived = false;
    private bool autoMoving = false;
    private bool autoTurning = false;
    private bool level2 = false;
    private float time = 0f;

    private void Start()
    {
        Debug.Log("Now Scene: " + SceneManager.GetActiveScene().name);

        if (SceneManager.GetActiveScene().name == "SubScene_02")
        {
            Scene2Start();
        }
    }

    void Scene2Start()
    {
        locomotor = FindObjectOfType<OVRPlayerController>();

        if (locomotor == null)
        {
            Debug.LogError("找不到 OVRPlayerController");
            return;
        }

        cc = locomotor.GetComponent<UnityEngine.CharacterController>();

        MovePlayerToStart();

        locomotor.SetHaltUpdateMovement(false);
        locomotor.EnableRotation = true;

        level2 = true;
    }

    void Update()
    {
        if (!level2 || locomotor == null) return;

        if (!hasArrived)
        {
            Vector3 playerPos = locomotor.transform.position;

            // 只比較 XZ 距離，不看 Y
            Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);
            Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);

            float dis = Vector2.Distance(playerXZ, targetXZ);

            if (dis <= arriveDistance)
            {
                hasArrived = true;
                autoMoving = true;

                locomotor.Stop();
                locomotor.SetHaltUpdateMovement(true);
                locomotor.EnableRotation = false;

                Debug.Log("玩家已到達目標點附近，開始自動就位");
            }

            time += Time.deltaTime;

            if (time > timeLimit)
            {
                ResetFlowAndMoveToStart();
            }
        }
        else if (autoMoving)
        {
            Vector3 current = locomotor.transform.position;

            // 只移動 XZ，Y 保持目前值
            Vector3 desired = new Vector3(targetPosition.x, current.y, targetPosition.z);

            Vector3 next = Vector3.MoveTowards(current, desired, autoMoveSpeed * Time.deltaTime);

            SetPlayerPositionSafely(next);

            Vector2 currentXZ = new Vector2(locomotor.transform.position.x, locomotor.transform.position.z);
            Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);

            if (Vector2.Distance(currentXZ, targetXZ) <= 0.05f)
            {
                autoMoving = false;
                autoTurning = true;
                Debug.Log("自動移動完成，開始自動轉向");
            }
        }
        else if (autoTurning)
        {
            Quaternion targetRot = Quaternion.Euler(targetRotationEuler);

            Quaternion nextRot = Quaternion.Slerp(
                locomotor.transform.rotation,
                targetRot,
                autoTurnSpeed * Time.deltaTime
            );

            SetPlayerRotationSafely(nextRot);

            if (Quaternion.Angle(locomotor.transform.rotation, targetRot) < 1f)
            {
                SetPlayerRotationSafely(targetRot);
                autoTurning = false;
                Debug.Log("自動轉向完成");
            }
        }
    }

    void MovePlayerToStart()
    {
        SetPlayerPositionSafely(startPosition);
        SetPlayerRotationSafely(Quaternion.Euler(startRotationEuler));
        time = 0f;
    }

    void ResetFlowAndMoveToStart()
    {
        hasArrived = false;
        autoMoving = false;
        autoTurning = false;

        locomotor.Stop();
        locomotor.SetHaltUpdateMovement(false);
        locomotor.EnableRotation = true;

        MovePlayerToStart();
    }

    void SetPlayerPositionSafely(Vector3 pos)
    {
        if (cc != null) cc.enabled = false;
        locomotor.transform.position = pos;
        if (cc != null) cc.enabled = true;
    }

    void SetPlayerRotationSafely(Quaternion rot)
    {
        if (cc != null) cc.enabled = false;
        locomotor.transform.rotation = rot;
        if (cc != null) cc.enabled = true;
    }
}