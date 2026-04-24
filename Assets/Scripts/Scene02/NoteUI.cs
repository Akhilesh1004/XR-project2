using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    public RecorderRhythmGameManager.HoleKey holeKey;
    public bool isJudged = false;

    private RectTransform rectTransform;
    private Image image;
    private RectTransform hitPoint;
    private float speed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Init(RecorderRhythmGameManager.HoleKey key, Sprite sprite, RectTransform targetHitPoint, float moveSpeed)
    {
        holeKey = key;
        hitPoint = targetHitPoint;
        speed = moveSpeed;

        if (image != null)
            image.sprite = sprite;
    }

    void Update()
    {
        if (isJudged) return;

        rectTransform.anchoredPosition += Vector2.right * speed * Time.deltaTime;
    }

    public float GetDistanceToHitPoint()
    {
        return rectTransform.anchoredPosition.x - hitPoint.anchoredPosition.x;
    }

    public bool HasPassedHitPoint()
    {
        return rectTransform.anchoredPosition.x > hitPoint.anchoredPosition.x + 60f;
    }

    public void Hit()
    {
        isJudged = true;
        Destroy(gameObject);
    }

    public void Miss()
    {
        isJudged = true;
        Destroy(gameObject);
    }
}