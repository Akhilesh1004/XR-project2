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
        public float holdDuration;
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

    [Header("Hold Note")]
    public float holdStartRange = 40f;

    private NoteUI currentHoldX;
    private NoteUI currentHoldY;
    private NoteUI currentHoldA;
    private NoteUI currentHoldB;

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

        // 【新增】：確保場景一載入時，環境音狀態是正常的 (沒有被靜音)
        AkSoundEngine.SetState("Env_State", "Normal");
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

        // 【新增】：小遊戲開始，播放鋼琴伴奏
        AkSoundEngine.PostEvent("Play_Piano_BGM", this.gameObject);

        // 【新增】：把全域狀態切換到 InGame，環境底噪會根據 Wwise 設定慢慢淡出！
        AkSoundEngine.SetState("Env_State", "InGame");
    }
    public void StopMiniGame()
    {
        Debug.Log("Mini game stop");
        isPlaying = false;
        isGameFinished = true;
        StopAllCoroutines();
        ClearAllNotes();

        // 【新增】：小遊戲中斷，停止鋼琴伴奏
        AkSoundEngine.PostEvent("Stop_Piano_BGM", this.gameObject);

        // 【新增】：把全域狀態切換回 Normal，環境底噪會慢慢淡入回來！
        AkSoundEngine.SetState("Env_State", "Normal");
    }
    public void LoadTestSong()
    {
        Debug.Log("LoadTestSong called");

        songNotes.Clear();

        // 載入瑪莉有隻小綿羊 - 玩家直笛譜版 (60 BPM)
        // 規則：
        // 一般一拍音符 -> 0.8f
        // 句尾兩拍延長 -> 1.8f
        // 最後收尾音 -> 2.8f

        // 第一句：si la so la (A Y X Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 1.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 2.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 3.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 4.0f, holdDuration = 0.8f });

        // 第二句：si si si (A A A)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 5.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 6.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 7.0f, holdDuration = 1.8f });

        // 第三句：la la la (Y Y Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 9.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 10.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 11.0f, holdDuration = 1.8f });

        // 第四句：si re re (A B B)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 13.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 14.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 15.0f, holdDuration = 1.8f });

        // 第五句：si la so la (A Y X Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 17.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 18.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 19.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 20.0f, holdDuration = 0.8f });

        // 第六句：si si si (A A A)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 21.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 22.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 23.0f, holdDuration = 1.8f });

        // 第七句 (完美收尾)：la la si la so (Y Y A Y X)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 25.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 26.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 27.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 28.0f, holdDuration = 0.8f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 29.0f, holdDuration = 2.8f });

        Debug.Log("Test notes loaded: " + songNotes.Count);
    }
    /*public void LoadTestSong()
    {
        Debug.Log("LoadTestSong called");

        songNotes.Clear();

        // 載入瑪莉有隻小綿羊 - 玩家直笛譜版 (60 BPM)

        // 第一句：si la so la (A Y X Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 1.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 2.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 3.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 4.0f });

        // 第二句：si si si (A A A)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 5.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 6.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 7.0f });

        // 第三句：la la la (Y Y Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 9.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 10.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 11.0f });

        // 第四句：si re re (A B B)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 13.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 14.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.B, spawnTime = 15.0f });

        // 第五句：si la so la (A Y X Y)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 17.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 18.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 19.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 20.0f });

        // 第六句：si si si (A A A)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 21.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 22.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 23.0f });

        // 第七句 (完美收尾)：la la si la so (Y Y A Y X)
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 25.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 26.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.A, spawnTime = 27.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.Y, spawnTime = 28.0f });
        songNotes.Add(new NoteSpawnData { holeKey = HoleKey.X, spawnTime = 29.0f });

        Debug.Log("Test notes loaded: " + songNotes.Count);
    }*/
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
        note.Init(data.holeKey, GetSprite(data.holeKey), lane.hitPoint, noteSpeed, data.holdDuration);

        activeNotes.Add(note);
    }

    void HandleInput()
    {
        // X
        if (OVRInput.GetDown(OVRInput.Button.One, controller_L))
        {
            Debug.Log("Press X");
            AkSoundEngine.PostEvent("Play_Note_X", this.gameObject);
            TryTapHit(HoleKey.X);
        }
        if (OVRInput.Get(OVRInput.Button.One, controller_L))
        {
            if (currentHoldX != null) currentHoldX.UpdateHolding();
        }
        if (OVRInput.GetUp(OVRInput.Button.One, controller_L))
        {
            ReleaseHold(HoleKey.X);
        }

        // Y
        if (OVRInput.GetDown(OVRInput.Button.Two, controller_L))
        {
            Debug.Log("Press Y");
            AkSoundEngine.PostEvent("Play_Note_Y", this.gameObject);
            TryTapHit(HoleKey.Y);
        }
        if (OVRInput.Get(OVRInput.Button.Two, controller_L))
        {
            if (currentHoldY != null) currentHoldY.UpdateHolding();
        }
        if (OVRInput.GetUp(OVRInput.Button.Two, controller_L))
        {
            ReleaseHold(HoleKey.Y);
        }

        // A
        if (OVRInput.GetDown(OVRInput.Button.One, controller_R))
        {
            Debug.Log("Press A");
            AkSoundEngine.PostEvent("Play_Note_A", this.gameObject);
            TryTapHit(HoleKey.A);
        }
        if (OVRInput.Get(OVRInput.Button.One, controller_R))
        {
            if (currentHoldA != null) currentHoldA.UpdateHolding();
        }
        if (OVRInput.GetUp(OVRInput.Button.One, controller_R))
        {
            ReleaseHold(HoleKey.A);
        }

        // B
        if (OVRInput.GetDown(OVRInput.Button.Two, controller_R))
        {
            Debug.Log("Press B");
            AkSoundEngine.PostEvent("Play_Note_B", this.gameObject);
            TryTapHit(HoleKey.B);
        }
        if (OVRInput.Get(OVRInput.Button.Two, controller_R))
        {
            if (currentHoldB != null) currentHoldB.UpdateHolding();
        }
        if (OVRInput.GetUp(OVRInput.Button.Two, controller_R))
        {
            ReleaseHold(HoleKey.B);
        }
    }
    void UpdateHoldNotes()
    {
        if (currentHoldX != null) currentHoldX.UpdateHolding();
        if (currentHoldY != null) currentHoldY.UpdateHolding();
        if (currentHoldA != null) currentHoldA.UpdateHolding();
        if (currentHoldB != null) currentHoldB.UpdateHolding();
    }
    void ReleaseHold(HoleKey key)
    {
        NoteUI note = null;

        switch (key)
        {
            case HoleKey.X: note = currentHoldX; break;
            case HoleKey.Y: note = currentHoldY; break;
            case HoleKey.A: note = currentHoldA; break;
            case HoleKey.B: note = currentHoldB; break;
        }

        if (note == null) return;

        int gain = note.ReleaseHoldAndScore();
        AddScore(gain);

        float ratio = note.GetCoveredRatio();

        if (ratio >= 0.9f)
            ShowResult("Perfect Hold");
        else if (ratio >= 0.6f)
            ShowResult("Good Hold");
        else if (ratio >= 0.3f)
            ShowResult("OK Hold");
        else
            ShowResult("Poor Hold");

        switch (key)
        {
            case HoleKey.X: currentHoldX = null; break;
            case HoleKey.Y: currentHoldY = null; break;
            case HoleKey.A: currentHoldA = null; break;
            case HoleKey.B: currentHoldB = null; break;
        }
    }
    void TryTapHit(HoleKey key)
    {
        NoteUI target = FindClosestNote(key);
        if (target == null) return;

        float minDist = Mathf.Abs(target.GetDistanceToHitPoint());

        if (target.IsHoldNote())
        {
            if (target.CanStartHold(holdStartRange))
            {
                target.StartHold();

                switch (key)
                {
                    case HoleKey.X: currentHoldX = target; break;
                    case HoleKey.Y: currentHoldY = target; break;
                    case HoleKey.A: currentHoldA = target; break;
                    case HoleKey.B: currentHoldB = target; break;
                }

                ShowResult("Hold Start");
            }

            return;
        }

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
    NoteUI FindClosestNote(HoleKey key)
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

        return target;
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

        // 【新增】：遊戲正常結束時，停止鋼琴伴奏
        AkSoundEngine.PostEvent("Stop_Piano_BGM", this.gameObject);

        // 【新增】：把全域狀態切換回 Normal，環境底噪會慢慢淡入回來！
        AkSoundEngine.SetState("Env_State", "Normal");

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