using System.Collections;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    [Header("玩家")]
    public Transform playerLocomotion;

    public Vector3 startPosition = new Vector3(-30f, 1f, 18f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);

    public Vector3 targetPosition = new Vector3(23f, 3.77f, 33.5f);

    [Header("設定")]
    public float timeLimit = 180f;          // 3分鐘 = 180秒
    public float arriveDistance = 1.5f;     // 距離目標多近算到達

    private bool hasArrived = false;
    private float time = 0.0f;

    void Start()
    {
        if (playerLocomotion == null)
        {
            Debug.LogError("SceneEventManager: 請先在 Inspector 指定 playerLocomotion");
            return;
        }

        MovePlayerToStart();
        
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
            }
            time += UnityEngine.Time.deltaTime;
            if (time > timeLimit) {
                MovePlayerToStart();
            }
        }
    }

    void MovePlayerToStart()
    {
        CharacterController cc = playerLocomotion.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        playerLocomotion.position = startPosition;
        playerLocomotion.rotation = Quaternion.Euler(startRotationEuler);

        if (cc != null)
        {
            cc.enabled = true;
        }
        time = 0.0f;
    }   

}