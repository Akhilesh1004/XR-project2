using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
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
    public OVRInput.Controller controller_R = OVRInput.Controller.RTouch;
    public OVRInput.Controller controller_L = OVRInput.Controller.LTouch;
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

    [Header("UI Manager")]
    public MiniGameUIManager uiManager;

    [Header("Debug")]
    public bool useTestSong = true;

    private float timer = 0f;
    private int spawnIndex = 0;
    private int score = 0;
    private bool isPlaying = false;
    private List<NoteUI> activeNotes = new List<NoteUI>();

    private bool isGameFinished = false;
    void Start()
    {
        UpdateScoreText();
        if (resultText != null)
            resultText.text = "";
    }

    void Update()
    {
        if (!isPlaying) return;
        if (isGameFinished) return;

        timer += Time.deltaTime;

        SpawnNotesByTime();
        HandleInput();
        CheckMissNotes();
        CheckGameEnd();
    }

    void SpawnNotesByTime()
    {
        while (spawnIndex < songNotes.Count && timer >= songNotes[spawnIndex].spawnTime)
        {
            SpawnNote(songNotes[spawnIndex]);
            spawnIndex++;
        }
    }
    public void StartMiniGame()
    {
        Debug.Log("Mini game start");

        if (useTestSong)
        {
            LoadTestSong();
        }
        StopAllCoroutines();
        ClearAllNotes();

        timer = 0f;
        spawnIndex = 0;
        score = 0;
        isPlaying = true;
        isGameFinished = false;

        UpdateScoreText();

        if (resultText != null)
            resultText.text = "";
    }
    public void StopMiniGame()
    {
        Debug.Log("Mini game stop");
        isPlaying = false;
        isGameFinished = true;
        StopAllCoroutines();
        ClearAllNotes();
    }
    public void LoadTestSong()
    {
        Debug.Log("LoadTestSong called");

        songNotes.Clear();

        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 1f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 2f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 3f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 4f });

        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 5f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 5.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 6.5f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 7.2f });

        Debug.Log("Test notes loaded: " + songNotes.Count);
    }
    void ClearAllNotes()
    {
        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] != null)
            {
                Destroy(activeNotes[i].gameObject);
            }
        }

        activeNotes.Clear();
    }

    void SpawnNote(NoteSpawnData data)
    {
        Debug.Log("SpawnNote");
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
        if(OVRInput.GetDown(OVRInput.Button.One, controller_L))
        {
            Debug.Log("Press X");
            TryHit(HoleKey.X);
            
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, controller_L))
        {
            Debug.Log("Press Y");
            TryHit(HoleKey.Y);
        }
        if (OVRInput.GetDown(OVRInput.Button.One, controller_R))
        {
            Debug.Log("Press A");
            TryHit(HoleKey.A);
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, controller_R))
        {
            Debug.Log("Press B");
            TryHit(HoleKey.B);
        }
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
    void CheckGameEnd()
    {
        bool allSpawned = spawnIndex >= songNotes.Count;
        bool noActiveNotes = activeNotes.Count == 0;

        if (allSpawned && noActiveNotes)
        {
            EndMiniGame();
        }
    }
    void EndMiniGame()
    {
        if (isGameFinished) return;

        isGameFinished = true;
        isPlaying = false;

        Debug.Log("Mini game finished. Score = " + score);

        if (score >= songNotes.Count * 60)
        {
            ShowResult("Success");
            Debug.Log("Score reached target");

            if (uiManager != null)
            {
                uiManager.CloseMiniGame();
            }
        }
        else
        {
            ShowResult("Fail - Restart");
            Debug.Log("Score not enough, restart");

            StartCoroutine(RestartMiniGameAfterDelay());
        }
    }

    IEnumerator RestartMiniGameAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        StartMiniGame();
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
            scoreText.text = "Score: " + score + " / " + (songNotes.Count * 60);
    }

    void ShowResult(string msg)
    {
        if (resultText != null)
            resultText.text = msg;
    }
}