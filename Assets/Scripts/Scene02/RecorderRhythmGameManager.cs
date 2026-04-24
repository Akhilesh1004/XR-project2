using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecorderRhythmGameManager : MonoBehaviour
{
    public enum HoleKey
    {
        X,
        Y,
        A,
        B
    }

    [System.Serializable]
    public class LaneData
    {
        public HoleKey holeKey;
        public RectTransform hitPoint;
    }

    [System.Serializable]
    public class NoteSpawnData
    {
        public HoleKey holeKey;
        public float spawnTime;
    }

    [Header("UI Reference")]
    public RectTransform canvasRect;
    public RectTransform spawnPoint;
    public GameObject notePrefab;

    [Header("Lane / Hit Point")]
    public List<LaneData> lanes = new List<LaneData>();

    [Header("Note Sprites")]
    public Sprite xSprite;
    public Sprite ySprite;
    public Sprite aSprite;
    public Sprite bSprite;

    [Header("Movement")]
    public float noteSpeed = 500f;
    public float perfectRange = 25f;
    public float goodRange = 50f;

    [Header("Song Data")]
    public List<NoteSpawnData> songNotes = new List<NoteSpawnData>();

    [Header("UI Text")]
    public TMP_Text scoreText;
    public TMP_Text resultText;

    private float timer = 0f;
    private int spawnIndex = 0;
    private int score = 0;

    private List<NoteUI> activeNotes = new List<NoteUI>();

    void Start()
    {
        UpdateScoreText();
        if (resultText != null)
            resultText.text = "";
    }

    void Update()
    {
        timer += Time.deltaTime;

        SpawnNotesByTime();
        HandleInput();
        CheckMissNotes();
    }

    void SpawnNotesByTime()
    {
        while (spawnIndex < songNotes.Count && timer >= songNotes[spawnIndex].spawnTime)
        {
            SpawnNote(songNotes[spawnIndex]);
            spawnIndex++;
        }
    }

    void SpawnNote(NoteSpawnData data)
    {
        LaneData lane = GetLane(data.holeKey);
        if (lane == null) return;

        GameObject obj = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, canvasRect);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(spawnPoint.anchoredPosition.x, lane.hitPoint.anchoredPosition.y);

        NoteUI note = obj.GetComponent<NoteUI>();
        note.Init(data.holeKey, GetSprite(data.holeKey), lane.hitPoint, noteSpeed);

        activeNotes.Add(note);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
            TryHit(HoleKey.X);

        if (Input.GetKeyDown(KeyCode.Y))
            TryHit(HoleKey.Y);

        if (Input.GetKeyDown(KeyCode.A))
            TryHit(HoleKey.A);

        if (Input.GetKeyDown(KeyCode.B))
            TryHit(HoleKey.B);
    }

    void TryHit(HoleKey key)
    {
        NoteUI target = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] == null) continue;
            if (activeNotes[i].isJudged) continue;
            if (activeNotes[i].holeKey != key) continue;

            float dist = Mathf.Abs(activeNotes[i].GetDistanceToHitPoint());

            if (dist < minDist)
            {
                minDist = dist;
                target = activeNotes[i];
            }
        }

        if (target == null) return;

        if (minDist <= perfectRange)
        {
            target.Hit();
            AddScore(100);
            ShowResult("Perfect");
        }
        else if (minDist <= goodRange)
        {
            target.Hit();
            AddScore(50);
            ShowResult("Good");
        }
    }

    void CheckMissNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] == null)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            if (activeNotes[i].isJudged) continue;

            if (activeNotes[i].HasPassedHitPoint())
            {
                activeNotes[i].Miss();
                ShowResult("Miss");
            }
        }
    }

    LaneData GetLane(HoleKey key)
    {
        for (int i = 0; i < lanes.Count; i++)
        {
            if (lanes[i].holeKey == key)
                return lanes[i];
        }
        return null;
    }

    Sprite GetSprite(HoleKey key)
    {
        switch (key)
        {
            case HoleKey.X: return xSprite;
            case HoleKey.Y: return ySprite;
            case HoleKey.A: return aSprite;
            case HoleKey.B: return bSprite;
        }
        return null;
    }

    void AddScore(int value)
    {
        score += value;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void ShowResult(string msg)
    {
        if (resultText != null)
            resultText.text = msg;
    }
}