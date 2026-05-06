using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class PortalTeleporter : MonoBehaviour
{
    private static bool globalTeleporting = false;

    [Header("Portal Settings")]
    public string targetSceneName;
    public string portalID;

    [Header("Spawn Settings")]
    public string spawnPointName = "PlayerSpawnPoint";

    [Header("Camera Rig")]
    public GameObject cameraRigRoot;

    [Header("Camera Layer")]
    public Camera playerCamera;
    public string targetSceneLayerName = "Sub1Scene";

    [Header("Fail-Safe Settings")]
    public string triggerTag = "Player";

    public GameObject final_object;

    private bool localTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (localTriggered) return;
        if (globalTeleporting) return;
        if (!other.CompareTag(triggerTag)) return;

        if (final_object != null) final_object.SetActive(true);
        localTriggered = true;
        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        globalTeleporting = true;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[PortalTeleporter] Target Scene Name is empty.");
            ResetTeleportLock();
            yield break;
        }

        if (cameraRigRoot == null)
        {
            Debug.LogError("[PortalTeleporter] Camera Rig Root is not assigned.");
            ResetTeleportLock();
            yield break;
        }

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);

        // 如果場景還沒載入，則進行疊加載入
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogWarning($"[PortalTeleporter] {targetSceneName} not loaded. Loading additively.");

            AsyncOperation op = SceneManager.LoadSceneAsync(
                targetSceneName,
                LoadSceneMode.Additive
            );

            while (!op.isDone)
            {
                yield return null;
            }

            yield return null;
            targetScene = SceneManager.GetSceneByName(targetSceneName);
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogError($"[PortalTeleporter] Failed to find loaded scene: {targetSceneName}");
            ResetTeleportLock();
            yield break;
        }

        Transform spawnPoint = FindObjectInScene(targetScene, spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogError($"[PortalTeleporter] Cannot find spawn point '{spawnPointName}' in scene '{targetSceneName}'.");
            ResetTeleportLock();
            yield break;
        }

        // --- 傳送邏輯開始 ---

        // 1. 調整相機 Culling Mask
        if (playerCamera != null && !string.IsNullOrEmpty(targetSceneLayerName))
        {
            int mask = LayerMask.GetMask(targetSceneLayerName);
            UniversalAdditionalCameraData cameraData = playerCamera.GetComponent<UniversalAdditionalCameraData>();

            if (mask != 0 && cameraData != null)
            {
                playerCamera.cullingMask |= mask;
                cameraData.volumeLayerMask |= mask;
            }
        }

        // 2. 暫時關閉 CharacterController 以進行位移
        CharacterController[] controllers = cameraRigRoot.GetComponentsInChildren<CharacterController>(true);
        bool[] controllerStates = new bool[controllers.Length];

        for (int i = 0; i < controllers.Length; i++)
        {
            controllerStates[i] = controllers[i].enabled;
            controllers[i].enabled = false;
        }

        // 3. 清除物理速度
        Rigidbody[] rigidbodies = cameraRigRoot.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 4. 執行位移與旋轉
        cameraRigRoot.transform.SetParent(null, true);
        cameraRigRoot.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        Physics.SyncTransforms();

        // 5. 轉移物件所屬場景並設為作用中
        SceneManager.MoveGameObjectToScene(cameraRigRoot, targetScene);
        SceneManager.SetActiveScene(targetScene);

        // --- 新增：Wwise BGM 狀態切換邏輯 ---
        SwitchWwiseBGMState(targetSceneName);

        yield return null;

        // 6. 恢復 CharacterController 狀態
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null)
            {
                controllers[i].enabled = controllerStates[i];
            }
        }

        ResetTeleportLock();
        PlayerPrefs.SetInt(portalID, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 根據目標場景名稱切換 Wwise 的 BGM 狀態
    /// </summary>
    private void SwitchWwiseBGMState(string sceneName)
    {
        string stateName = "Scene0"; // 預設

        if (sceneName == "main") stateName = "Scene0";
        else if (sceneName == "SubScene_01") stateName = "Scene1";
        else if (sceneName == "SubScene_02") stateName = "Scene2";
        else if (sceneName == "SubScene_03") stateName = "Scene3";

        // 執行 Wwise 狀態切換指令
        AkSoundEngine.SetState("BGM_State", stateName);
        Debug.Log($"[Wwise] BGM State switched to: {stateName} (from Scene: {sceneName})");
    }

    private void ResetTeleportLock()
    {
        globalTeleporting = false;
    }

    private Transform FindObjectInScene(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform result = FindChildRecursive(root.transform, objectName);
            if (result != null) return result;
        }
        return null;
    }

    private Transform FindChildRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, objectName);
            if (result != null) return result;
        }
        return null;
    }
}