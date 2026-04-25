using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalPulseOnlyGrow : MonoBehaviour
{
    [Header("Scale Pulse")]
    public float pulseSpeed = 1.5f;
    public float minScale = 1.0f;   // 一定要 >= 1
    public float maxScale = 1.08f;  // 不要太大，建議 1.03 ~ 1.12

    [Header("Emission Pulse")]
    public float emissionMin = 1.2f;
    public float emissionMax = 2.0f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        // 保險：避免一開始數值亂掉
        if (minScale < 1f)
            minScale = 1f;

        if (maxScale < minScale)
            maxScale = minScale;
    }

    void Update()
    {
        if (mat == null) return;

        // t 會在 0~1 間變化
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // scale 只會在 minScale ~ maxScale 間變化，而且 minScale >= 1
        float scale = Mathf.Lerp(minScale, maxScale, t);

        // 以中心為基準放大
        Vector2 tiling = new Vector2(scale, scale);
        Vector2 offset = new Vector2((1f - scale) * 0.5f, (1f - scale) * 0.5f);

        mat.SetTextureScale("_BaseMap", tiling);
        mat.SetTextureOffset("_BaseMap", offset);

        // 如果你不是 URP，而是 Built-in shader，改成 _MainTex
        // mat.SetTextureScale("_MainTex", tiling);
        // mat.SetTextureOffset("_MainTex", offset);

        if (mat.HasProperty("_EmissionColor"))
        {
            float e = Mathf.Lerp(emissionMin, emissionMax, t);
            mat.SetColor("_EmissionColor", Color.white * e);
        }
    }
}