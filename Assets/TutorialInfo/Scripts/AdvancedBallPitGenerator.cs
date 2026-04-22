using UnityEngine;
using System.Collections.Generic;

public class AdvancedBallPitGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] staticBallPrefabs;   // 底層假球
    public GameObject[] dynamicBallPrefabs;  // 最上層真球

    [Header("Parent")]
    public Transform container;

    [Header("Area")]
    public float areaWidth = 8f;
    public float areaLength = 5f;
    public float baseY = 0.05f;

    [Header("Layer Settings")]
    public int staticLayerCount = 2;
    public int[] staticBallsPerLayer = new int[] { 180, 120 };
    public float staticLayerHeight = 0.14f;

    public int dynamicBallCount = 35;
    public float dynamicLayerHeight = 0.35f;

    [Header("Random")]
    public float randomOffsetXZ = 0.04f;
    public float randomHeightOffset = 0.02f;
    public float randomScaleMin = 0.95f;
    public float randomScaleMax = 1.05f;

    [Header("Collision Check")]
    public float checkRadius = 0.08f;
    public LayerMask obstacleLayer;
    public int maxTriesPerBall = 20;

    [Header("Optional Ground Snap")]
    public bool useGroundSnap = false;
    public LayerMask groundLayer;
    public float rayStartHeight = 3f;
    public float groundOffset = 0.02f;

    [ContextMenu("Generate Advanced Ball Pit")]
    public void GenerateBallPit()
    {
        if ((staticBallPrefabs == null || staticBallPrefabs.Length == 0) &&
            (dynamicBallPrefabs == null || dynamicBallPrefabs.Length == 0))
        {
            Debug.LogWarning("No ball prefabs assigned.");
            return;
        }

        if (container == null)
        {
            GameObject parent = new GameObject("BallPit_Container");
            parent.transform.SetParent(transform);
            parent.transform.localPosition = Vector3.zero;
            container = parent.transform;
        }

        ClearBallPit();

        GenerateStaticLayers();
        GenerateDynamicTopLayer();

        Debug.Log("Advanced ball pit generated.");
    }

    void GenerateStaticLayers()
    {
        if (staticBallPrefabs == null || staticBallPrefabs.Length == 0) return;

        for (int layer = 0; layer < staticLayerCount; layer++)
        {
            int count = GetStaticBallCount(layer);
            float y = baseY + layer * staticLayerHeight;

            for (int i = 0; i < count; i++)
            {
                TrySpawnBall(staticBallPrefabs, y, false);
            }
        }
    }

    void GenerateDynamicTopLayer()
    {
        if (dynamicBallPrefabs == null || dynamicBallPrefabs.Length == 0) return;

        float y = baseY + dynamicLayerHeight;

        for (int i = 0; i < dynamicBallCount; i++)
        {
            TrySpawnBall(dynamicBallPrefabs, y, true);
        }
    }

    void TrySpawnBall(GameObject[] prefabs, float layerY, bool isDynamic)
    {
        for (int attempt = 0; attempt < maxTriesPerBall; attempt++)
        {
            float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float z = Random.Range(-areaLength / 2f, areaLength / 2f);

            x += Random.Range(-randomOffsetXZ, randomOffsetXZ);
            z += Random.Range(-randomOffsetXZ, randomOffsetXZ);

            float y = layerY + Random.Range(-randomHeightOffset, randomHeightOffset);

            Vector3 spawnPos = transform.position + new Vector3(x, y, z);

            if (useGroundSnap)
            {
                Vector3 rayStart = new Vector3(spawnPos.x, transform.position.y + rayStartHeight, spawnPos.z);
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, groundLayer))
                {
                    spawnPos.y = hit.point.y + groundOffset + (layerY - baseY);
                }
            }

            if (Physics.CheckSphere(spawnPos, checkRadius, obstacleLayer))
            {
                continue;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Quaternion rot = Random.rotation;
            GameObject ball = Instantiate(prefab, spawnPos, rot, container);

            float randomScale = Random.Range(randomScaleMin, randomScaleMax);
            ball.transform.localScale *= randomScale;

            if (!isDynamic)
            {
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(rb);
#else
                    Destroy(rb);
#endif
                }
            }

            return;
        }
    }

    int GetStaticBallCount(int layerIndex)
    {
        if (staticBallsPerLayer != null && layerIndex < staticBallsPerLayer.Length)
            return staticBallsPerLayer[layerIndex];

        return 50;
    }

    [ContextMenu("Clear Ball Pit")]
    public void ClearBallPit()
    {
        if (container == null) return;

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in container)
        {
            children.Add(child.gameObject);
        }

        for (int i = 0; i < children.Count; i++)
        {
#if UNITY_EDITOR
            DestroyImmediate(children[i]);
#else
            Destroy(children[i]);
#endif
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(0, baseY, 0),
            new Vector3(areaWidth, 0.1f, areaLength)
        );
    }
}