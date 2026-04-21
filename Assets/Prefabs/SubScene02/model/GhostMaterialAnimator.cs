using UnityEngine;

public class GhostMaterialAnimator : MonoBehaviour
{
    public Renderer targetRenderer;

    [Header("發光顏色")]
    public Color emissionColor = new Color(1f, 0.75f, 0.9f, 1f);

    [Header("整體發光強度")]
    public float minGlowIntensity = 2f;
    public float maxGlowIntensity = 7f;
    public float glowSpeed = 3.5f;

    [Header("點點貼圖基礎 Tiling")]
    public Vector2 baseTiling = new Vector2(5f, 5f);

    [Header("點點貼圖抖動幅度")]
    public float offsetJitterAmount = 0.15f;
    public float offsetJitterSpeed = 1.5f;

    [Header("Tiling 微變化幅度")]
    public float tilingPulseAmount = 0.4f;
    public float tilingPulseSpeed = 1.2f;

    private Material mat;
    private Vector2 baseOffset;

    void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
        }

        if (targetRenderer == null)
        {
            Debug.LogError("GhostMaterialAnimator：找不到 Renderer", this);
            return;
        }

        mat = targetRenderer.sharedMaterial;

        if (mat == null)
        {
            Debug.LogError("GhostMaterialAnimator：找不到材質", this);
            return;
        }

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * minGlowIntensity);
        }

        if (mat.HasProperty("_EmissionMap"))
        {
            baseOffset = mat.GetTextureOffset("_EmissionMap");
            mat.SetTextureScale("_EmissionMap", baseTiling);
            mat.SetTextureOffset("_EmissionMap", baseOffset);
        }

        Debug.Log("GhostMaterialAnimator 已綁定 Renderer: " + targetRenderer.name, this);
    }

    void Update()
    {
        if (mat == null) return;

        // 1. 整體發光呼吸
        float glow = Mathf.Lerp(
            minGlowIntensity,
            maxGlowIntensity,
            (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f
        );

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", emissionColor * glow);
        }

        // 2. Emission Map 小幅亂動
        if (mat.HasProperty("_EmissionMap"))
        {
            float jitterX = (Mathf.PerlinNoise(Time.time * offsetJitterSpeed, 0f) - 0.5f) * 2f * offsetJitterAmount;
            float jitterY = (Mathf.PerlinNoise(0f, Time.time * offsetJitterSpeed) - 0.5f) * 2f * offsetJitterAmount;

            Vector2 offset = baseOffset + new Vector2(jitterX, jitterY);
            mat.SetTextureOffset("_EmissionMap", offset);

            // 3. Tiling 微微脈動
            float pulse = 1f + Mathf.Sin(Time.time * tilingPulseSpeed) * tilingPulseAmount;
            Vector2 animatedTiling = baseTiling * pulse;
            mat.SetTextureScale("_EmissionMap", animatedTiling);
        }
    }
}