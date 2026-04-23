using System.Collections;
using Oculus.Interaction.Locomotion;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [Header("玩家")]
    public Transform playerLocomotion;

    public Vector3 startPosition = new Vector3(-30f, 1f, 18f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);
    public Vector3 targetPosition = new Vector3(19f, 3.561244f, 36.734f);
    public Vector3 targetRotationEuler = new Vector3(0f, 90f, 0f);

    [Header("設定")]
    public float timeLimit = 180f;          // 3分鐘 = 180秒
    public float arriveDistance = 1.5f;     // 距離目標多近算到達
    public FirstPersonLocomotor locomotor;

    private bool hasArrived = false;
    private bool AutoMoving = false;
    private bool AutoTurning = false;
    private float autoMoveSpeed = 2f;
    private float autoTurnSpeed = 2f;
    private float time = 0.0f;

    void Start()
    {
        if (playerLocomotion == null)
        {
            Debug.LogError("SceneEventManager: 請先在 Inspector 指定 playerLocomotion");
            return;
        }

        MovePlayerToStart();
        locomotor.enabled = true;
    }

    void Update()
    {
        if (!hasArrived)
        {
            float dis = Vector3.Distance(playerLocomotion.position, targetPosition);
            if (dis <= arriveDistance)
            {
                hasArrived = true;
                Debug.Log("玩家已到達目標點");
                AutoMoving = true;
                locomotor.enabled = false;
            }
            time += UnityEngine.Time.deltaTime;
            if (time > timeLimit)
            {
                MovePlayerToStart();
            }
        }
        else if (AutoMoving)
        {

            playerLocomotion.position = Vector3.MoveTowards(
                    playerLocomotion.position,
                    targetPosition,
                    autoMoveSpeed * Time.deltaTime
            );
            if (Vector3.Distance(playerLocomotion.position, targetPosition) <= 0.1f)
            {
                playerLocomotion.position = targetPosition;
                AutoMoving = false;
                AutoTurning = true;
            }
        }
        else if (AutoTurning)
        {
            Quaternion targetRot = Quaternion.Euler(targetRotationEuler);
            playerLocomotion.rotation = Quaternion.Slerp(
                playerLocomotion.rotation,
                targetRot,
                autoTurnSpeed * Time.deltaTime
            );
            if(Quaternion.Angle(playerLocomotion.rotation, targetRot) < 0.01f)
            {
                AutoTurning = false;
            }
        }
    }

    void MovePlayerToStart()
    { 
        playerLocomotion.position = startPosition;
        playerLocomotion.rotation = Quaternion.Euler(startRotationEuler);

        time = 0.0f;
    }   

}