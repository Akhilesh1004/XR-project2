using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("捲動設定")]
    [Tooltip("數值越大，往上捲動的速度越快")]
    public float scrollSpeed = 10f;

    private RectTransform rectTransform;

    void Start()
    {
        // 取得 UI 的 RectTransform 元件
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 讓文字隨著時間往上移動 (Vector2.up 即 Y 軸正向)
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }
}