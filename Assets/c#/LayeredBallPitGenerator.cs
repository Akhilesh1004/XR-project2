using UnityEngine;
using System.Collections.Generic;

public class LayeredBallPitGenerator : MonoBehaviour
{
    [Header("Ball Prefabs")]
    public GameObject[] ballPrefabs;

    [Header("Parent")]
    public Transform container;

    [Header("Area Size")]
    public float areaWidth = 8f;
    public float areaLength = 6f;

    [Header("Ball Size")]
    public float ballSpacing = 0.22f;
    public float randomOffsetXZ = 0.05f;
    public float randomHeightOffset = 0.03f;
    public float randomScaleMin = 0.9f;
    public float randomScaleMax = 1.1f;

    [Header("Layer Settings")]
    public int layerCount = 3;
    public int[] ballsPerLayer = new int[] { 180, 120, 70 };
    public float layerHeight = 0.16f;

    [Header("Base Position")]
    public float baseY = 0f;

    [ContextMenu("Generate Layered Ball Pit")]
    public void GenerateBallPit()
    {
        if (ballPrefabs == null || ballPrefabs.Length == 0)
        {
            Debug.LogWarning("No ball prefabs assigned.");
            return;
        }

        if (container == null)
        {
            GameObject newContainer = new GameObject("BallPit_Container");
            newContainer.transform.SetParent(transform);
            newContainer.transform.localPosition = Vector3.zero;
            container = newContainer.transform;
        }

        ClearBallPit();

        for (int layer = 0; layer < layerCount; layer++)
        {
            int count = GetBallCountForLayer(layer);
            float y = baseY + (layer * layerHeight);

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = ballPrefabs[Random.Range(0, ballPrefabs.Length)];

                float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
                float z = Random.Range(-areaLength / 2f, areaLength / 2f);

                x += Random.Range(-randomOffsetXZ, randomOffsetXZ);
                z += Random.Range(-randomOffsetXZ, randomOffsetXZ);

                float yOffset = Random.Range(-randomHeightOffset, randomHeightOffset);

                Vector3 spawnPos = transform.position + new Vector3(x, y + yOffset, z);

                Quaternion rot = Random.rotation;

                GameObject ball = Instantiate(prefab, spawnPos, rot, container);

                float scaleRandom = Random.Range(randomScaleMin, randomScaleMax);
                ball.transform.localScale *= scaleRandom;
            }
        }

        Debug.Log("Layered ball pit generated.");
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

    int GetBallCountForLayer(int layerIndex)
    {
        if (ballsPerLayer != null && layerIndex < ballsPerLayer.Length)
            return ballsPerLayer[layerIndex];

        return 50;
    }
}