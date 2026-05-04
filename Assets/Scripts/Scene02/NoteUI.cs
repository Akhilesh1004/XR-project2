using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    public RecorderRhythmGameManager.HoleKey holeKey;
    public bool isJudged = false;

    [Header("UI")]
    public Image headImage;
    public RectTransform tailRect;
    public Image tailImage;

    [Header("Size")]
    public Vector2 headSize = new Vector2(80f, 80f);
    public float tailHeight = 30f;

    private RectTransform rectTransform;
    private RectTransform hitPoint;
    private float speed;

    private bool isHoldNote = false;
    private float holdDuration = 0f;
    private float holdLength = 0f;

    private bool isHolding = false;
    private bool holdStarted = false;

    private float noteLeftX;
    private float noteRightX;

    private float coveredLeftX;
    private float coveredRightX;

    public void Init(
        RecorderRhythmGameManager.HoleKey key,
        Sprite sprite,
        RectTransform targetHitPoint,
        float moveSpeed,
        float inputHoldDuration
    )
    {
        Debug.Log("Init: " + key);

        rectTransform = GetComponent<RectTransform>();
        holeKey = key;
        hitPoint = targetHitPoint;
        speed = moveSpeed;

        rectTransform.sizeDelta = headSize;

        if (headImage == null)
        {
            headImage = GetComponentInChildren<Image>();
        }

        if (headImage != null)
        {
            headImage.sprite = sprite;
            headImage.color = Color.white;
            headImage.enabled = true;
            headImage.preserveAspect = true;
        }

        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one * 0.2f;

        holdDuration = inputHoldDuration;
        isHoldNote = holdDuration > 0.01f;

        if (isHoldNote)
        {
            holdLength = speed * holdDuration;

            if (tailRect != null)
            {
                tailRect.gameObject.SetActive(true);

                tailRect.anchorMin = new Vector2(0.5f, 0.5f);
                tailRect.anchorMax = new Vector2(0.5f, 0.5f);
                tailRect.pivot = new Vector2(1f, 0.5f);

                // 尾巴改成往左延伸
                tailRect.anchoredPosition = new Vector2(-headSize.x * 0.5f, 0f);
                tailRect.sizeDelta = new Vector2(holdLength, tailHeight);
                tailRect.localRotation = Quaternion.identity;
                tailRect.localScale = Vector3.one;
            }

            if (tailImage != null)
            {
                tailImage.color = new Color(1f, 1f, 1f, 0.85f);
                tailImage.enabled = true;
            }
        }
        else
        {
            if (tailRect != null)
            {
                tailRect.gameObject.SetActive(false);
            }
        }

        UpdateWorldRange();
    }

    void Update()
    {
        if (isJudged) return;

        rectTransform.anchoredPosition += Vector2.right * speed * Time.deltaTime;
        UpdateWorldRange();
    }

    void UpdateWorldRange()
    {
        float headCenterX = rectTransform.anchoredPosition.x;
        float headHalf = headSize.x * 0.5f;

        if (isHoldNote)
        {
            // 尾巴在左邊，所以整顆 note 的左界要往左延伸 holdLength
            noteLeftX = headCenterX - headHalf - holdLength;
            noteRightX = headCenterX + headHalf;
        }
        else
        {
            noteLeftX = headCenterX - headHalf;
            noteRightX = headCenterX + headHalf;
        }
    }

    public float GetDistanceToHitPoint()
    {
        return rectTransform.anchoredPosition.x - hitPoint.anchoredPosition.x;
    }

    public bool HasPassedHitPoint()
    {
        // 頭已經超過判定點很多時，算 miss
        return rectTransform.anchoredPosition.x > hitPoint.anchoredPosition.x + 60f;
    }

    public bool IsHoldNote()
    {
        return isHoldNote;
    }

    public bool CanStartHold(float startRange)
    {
        if (!isHoldNote || hitPoint == null) return false;

        float hitX = hitPoint.anchoredPosition.x;
        float headCenterX = rectTransform.anchoredPosition.x;

        // 用按鍵頭接近判定點來開始按住
        return Mathf.Abs(headCenterX - hitX) <= startRange;
    }

    public void StartHold()
    {
        if (!isHoldNote || holdStarted) return;

        holdStarted = true;
        isHolding = true;

        float hitX = hitPoint.anchoredPosition.x;

        // 尾巴在左邊，所以覆蓋是從尾巴左側往右累積到 hit line
        coveredRightX = Mathf.Min(noteRightX, hitX);
        coveredLeftX = coveredRightX;
    }

    public void UpdateHolding()
    {
        if (!isHoldNote || !isHolding || isJudged) return;

        float hitX = hitPoint.anchoredPosition.x;

        // 尾巴在左邊，note 持續往右移時，左邊界會慢慢右移
        coveredLeftX = Mathf.Clamp(hitX - holdLength, noteLeftX, coveredRightX);
    }

    public int ReleaseHoldAndScore()
    {
        if (!isHoldNote || !holdStarted) return 0;

        isHolding = false;

        float coveredLength = Mathf.Max(0f, coveredRightX - coveredLeftX);
        float ratio = holdLength > 0f ? coveredLength / holdLength : 0f;

        int score = 0;

        if (ratio >= 0.9f)
        {
            score = 100;
        }
        else if (ratio >= 0.6f)
        {
            score = 70;
        }
        else if (ratio >= 0.3f)
        {
            score = 40;
        }
        else
        {
            score = 10;
        }

        Hit();
        return score;
    }

    public float GetCoveredRatio()
    {
        if (!isHoldNote) return 0f;

        float coveredLength = Mathf.Max(0f, coveredRightX - coveredLeftX);
        return holdLength > 0f ? Mathf.Clamp01(coveredLength / holdLength) : 0f;
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