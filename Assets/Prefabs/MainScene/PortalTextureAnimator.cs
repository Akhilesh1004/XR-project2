using UnityEngine;

public class BallPitGenerator : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject[] ballPrefabs;
    public int ballCount = 300;
    public Vector3 areaSize = new Vector3(8f, 2f, 6f);
    public float yOffset = 0f;
    public Transform container;

    [Header("Randomness")]
    public float randomScaleMin = 0.9f;
    public float randomScaleMax = 1.1f;
    public bool randomRotation = true;

    void Start()
    {
        GenerateBalls();
    }

    [ContextMenu("Generate Balls")]
    public void GenerateBalls()
    {
        if (ballPrefabs == null || ballPrefabs.Length == 0)
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

        for (int i = 0; i < ballCount; i++)
        {
            GameObject prefab = ballPrefabs[Random.Range(0, ballPrefabs.Length)];

            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(0f, areaSize.y) + yOffset,
                Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
            );

            Quaternion rot = randomRotation
                ? Random.rotation
                : Quaternion.identity;

            GameObject ball = Instantiate(prefab, randomPos, rot, container);

            float randomScale = Random.Range(randomScaleMin, randomScaleMax);
            ball.transform.localScale *= randomScale;
        }
    }
}