using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScenePreviewLoader : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName = "sub1";
    [SerializeField] private string portalBAnchorName = "PortalBAnchor";

    [Header("Portal Rendering")]
    [SerializeField] private Camera portalCamera;
    [SerializeField] private RenderTexture portalRenderTexture;
    [SerializeField] private Renderer portalSurfaceRenderer;
    [SerializeField] private string targetSceneLayerName = "Sub1Scene";

    [Header("View Sync")]
    [SerializeField] private PortalCameraViewSync viewSync;

    private bool isLoading;
    private bool isReady;
    
    private void OnEnable()
    {
        Debug.LogWarning("Enabled");
        InitializeBasicSettings();

        if (!isReady && !isLoading)
        {
            StartCoroutine(LoadTargetSceneAndStartPreview());
        }
        else if (isReady && portalCamera != null)
        {
            portalCamera.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (portalCamera != null)
        {
            portalCamera.enabled = false;
        }
    }

    private void InitializeBasicSettings()
    {
        if (portalCamera != null)
        {
            portalCamera.enabled = false;
            portalCamera.targetTexture = portalRenderTexture;

            int mask = LayerMask.GetMask(targetSceneLayerName);

            if (mask != 0)
            {
                portalCamera.cullingMask = mask;
            }
            else
            {
                Debug.LogWarning($"Layer not found: {targetSceneLayerName}");
            }
        }

        ApplyRenderTextureToSurface();
    }

    private IEnumerator LoadTargetSceneAndStartPreview()
    {
        isLoading = true;

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(
                targetSceneName,
                LoadSceneMode.Additive
            );

            while (!op.isDone)
            {
                yield return null;
            }
        }

        targetScene = SceneManager.GetSceneByName(targetSceneName);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogError($"Failed to load scene: {targetSceneName}");
            isLoading = false;
            yield break;
        }

        Transform portalBAnchor = FindObjectInScene(targetScene, portalBAnchorName);

        if (portalBAnchor == null)
        {
            Debug.LogError($"Cannot find {portalBAnchorName} in {targetSceneName}");
            isLoading = false;
            yield break;
        }

        if (viewSync != null)
        {
            viewSync.SetPortalBAnchor(portalBAnchor);
        }

        if (portalCamera != null)
        {
            portalCamera.targetTexture = portalRenderTexture;
            portalCamera.enabled = true;
        }

        ApplyRenderTextureToSurface();

        isReady = true;
        isLoading = false;

        Debug.Log($"Portal preview ready: {targetSceneName}");
    }

    private void ApplyRenderTextureToSurface()
    {
        if (portalSurfaceRenderer == null) return;
        if (portalRenderTexture == null) return;

        Material mat = portalSurfaceRenderer.material;

        // URP
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", portalRenderTexture);
        }

        // Built-in
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", portalRenderTexture);
        }
    }

    private Transform FindObjectInScene(Scene scene, string objectName)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;

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