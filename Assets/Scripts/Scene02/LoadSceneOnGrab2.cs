using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnGrab2 : MonoBehaviour
{
    [Header("Grab")]
    public Grabbable grabbable;
    public bool autoFindGrabbable = true;

    [Header("Scene")]
    public string sceneName;
    public float loadDelay = 0f;
    public bool onlyOnce = true;

    private bool wasGrabbedLastFrame = false;
    private bool hasTriggered = false;

    private void Awake()
    {
        if (autoFindGrabbable && grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (grabbable == null)
        {
            Debug.LogError("[LoadSceneOnGrab] 找不到 Grabbable: " + gameObject.name);
        }
    }

    private void Update()
    {
        if (grabbable == null) return;
        if (onlyOnce && hasTriggered) return;

        bool isGrabbedNow = grabbable.SelectingPointsCount > 0;

        if (!wasGrabbedLastFrame && isGrabbedNow)
        {
            hasTriggered = true;
            Debug.Log("[LoadSceneOnGrab] 物件被抓到，準備切換場景: " + sceneName);

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(sceneName);
            }
            else
            {
                Debug.LogError("SceneTransitionManager.Instance is null");
            }
        }

        wasGrabbedLastFrame = isGrabbedNow;
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[LoadSceneOnGrab] sceneName 沒有設定");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}