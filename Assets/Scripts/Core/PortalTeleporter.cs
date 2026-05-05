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

    [Header("Object Control")]
    public GameObject objectToHide;

    [Header("Fail-Safe Settings")]
    public string triggerTag = "Player";

    private bool localTriggered = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt(portalID, 0) == 1)
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (localTriggered) return;
        if (globalTeleporting) return;
        if (!other.CompareTag(triggerTag)) return;

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

        Debug.Log($"[PortalTeleporter] Teleporting to scene = {targetScene.name}, spawn = {spawnPoint.name}, pos = {spawnPoint.position}");

        // 讓玩家主相機看得到目標 sub scene layer
        if (playerCamera != null && !string.IsNullOrEmpty(targetSceneLayerName))
        {
            int mask = LayerMask.GetMask(targetSceneLayerName);
            UniversalAdditionalCameraData cameraData =
                playerCamera.GetComponent<UniversalAdditionalCameraData>();

            if (mask != 0 && cameraData != null)
            {
                playerCamera.cullingMask |= mask;
                cameraData.volumeLayerMask |= mask;
            }
            else
            {
                Debug.LogWarning($"[PortalTeleporter] Layer not found: {targetSceneLayerName}");
            }
        }

        // 關掉 CharacterController，避免 SetPosition 後又被 controller 推回去
        CharacterController[] controllers =
            cameraRigRoot.GetComponentsInChildren<CharacterController>(true);

        bool[] controllerStates = new bool[controllers.Length];

        for (int i = 0; i < controllers.Length; i++)
        {
            controllerStates[i] = controllers[i].enabled;
            controllers[i].enabled = false;
        }

        // 清掉 Rigidbody 速度，避免傳送後繼續掉落/彈飛
        Rigidbody[] rigidbodies =
            cameraRigRoot.GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 直接移整個 Camera Rig Root 到指定 SpawnPoint
        cameraRigRoot.transform.SetParent(null, true);

        cameraRigRoot.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        Physics.SyncTransforms();

        // 把 Rig 所屬 Scene 移到 target scene
        SceneManager.MoveGameObjectToScene(cameraRigRoot, targetScene);
        SceneManager.SetActiveScene(targetScene);

        yield return null;

        // 恢復 CharacterController
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null)
            {
                controllers[i].enabled = controllerStates[i];
            }
        }

        Debug.Log($"[PortalTeleporter] Final Rig Position = {cameraRigRoot.transform.position}");
        Debug.Log($"[PortalTeleporter] Active Scene = {SceneManager.GetActiveScene().name}");

        ResetTeleportLock();
        PlayerPrefs.SetInt(portalID, 1);
        PlayerPrefs.Save();
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

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, objectName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}