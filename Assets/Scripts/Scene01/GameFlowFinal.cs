using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowFinal : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("玩家")]
    public OVRPlayerController locomotor;

    public Vector3 startPosition = new Vector3(-8.7f, -4f, -18.33f);
    public Vector3 startRotationEuler = new Vector3(0f, 0f, 0f);

    private bool hasTriggered = false;

    private void OnEnable()
    {
        // 註冊 Active Scene 切換事件
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        // 解除註冊
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    private void Start()
    {
        Debug.Log("Now Scene: " + SceneManager.GetActiveScene().name);

        if (SceneManager.GetActiveScene().name == "FinalScene")
        {
            Scene1Start();
        }
    }
    void Scene4Start()
    {
        locomotor = FindObjectOfType<OVRPlayerController>();
        locomotor.transform.position = startPosition;
        locomotor.transform.rotation = Quaternion.Euler(startRotationEuler);
    }

    // Update is called once per frame
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "FinalScene")
        {
            Scene4Start();
        }
    }
}
