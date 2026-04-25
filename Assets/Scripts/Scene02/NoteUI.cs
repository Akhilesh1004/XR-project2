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
    public Vector2 noteSize = new Vector2(80f, 80f);

    public void Init(RecorderRhythmGameManager.HoleKey key, Sprite sprite, RectTransform targetHitPoint, float moveSpeed)
    {
        Debug.Log("Init: " + key);
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        rectTransform.sizeDelta = noteSize;
        holeKey = key;
        hitPoint = targetHitPoint;
        speed = moveSpeed;

        if (image != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.enabled = true;
            image.preserveAspect = true;
        }

        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        rectTransform.localScale = Vector3.one * 0.2f;
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