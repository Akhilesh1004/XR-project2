using UnityEngine;

public class RecorderRhythmTestData : MonoBehaviour
{
    public RecorderRhythmGameManager gameManager;

    void Awake()
    {
        if (gameManager == null) return;

        gameManager.songNotes.Clear();

        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.X, spawnTime = 1f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.Y, spawnTime = 2f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.A, spawnTime = 3f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.B, spawnTime = 4f });

        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.X, spawnTime = 5f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.A, spawnTime = 5.8f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.Y, spawnTime = 6.5f });
        gameManager.songNotes.Add(new RecorderRhythmGameManager.NoteSpawnData { holeKey = RecorderRhythmGameManager.HoleKey.B, spawnTime = 7.2f });
    }
}