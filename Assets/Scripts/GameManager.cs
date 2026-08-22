using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameConfig config;
    [SerializeField] private BirdController bird;
    [SerializeField] private PipeSpawner pipeSpawner;
    [SerializeField] private ScrollRepeater[] scrollers;
    [SerializeField] private GameUI gameUi;
    [SerializeField] private GameAudio gameAudio;

    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public GameConfig Config => config;

    private const string BestScoreKey = "FlappyBird.BestScore";

    private GameEffects effects;
    private Dragon dragon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
        }

        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

        HideLoosePipes();
        FixCamera();
        FixSceneSpriteMaterials();
        CreateHelpers();
    }

    private void CreateHelpers()
    {
        effects = FindFirstObjectByType<GameEffects>();
        if (effects == null)
        {
            effects = new GameObject("GameEffects").AddComponent<GameEffects>();
        }

        dragon = FindFirstObjectByType<Dragon>();
        if (dragon == null)
        {
            dragon = new GameObject("Dragon").AddComponent<Dragon>();
        }
    }

    private static void FixCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(78f / 255f, 192f / 255f, 202f / 255f);
    }

    private static void FixSceneSpriteMaterials()
    {
        Material material = SpriteLibrary.SpriteMaterial;
        if (material == null)
        {
            return;
        }

        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    /// <summary>
    /// Pipe halves left in the scene root while building it would sit in front of
    /// the camera forever, so they are switched off before the first frame.
    /// </summary>
    private static void HideLoosePipes()
    {
        HideIfLoose("PipeTop");
        HideIfLoose("PipeBottom");
    }

    private static void HideIfLoose(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        if (found != null && found.GetComponent<PipePair>() == null && found.transform.parent == null)
        {
            found.SetActive(false);
        }
    }

    private void Start()
    {
        EnterReady();
    }

    private void Update()
    {
        if (State == GameState.Ready && FlapInput.WasPressedThisFrame() && !IsClickingUi())
        {
            StartRun();
        }
    }

    private static bool IsClickingUi()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void StartRun()
    {
        Score = 0;
        State = GameState.Playing;

        if (bird != null)
        {
            bird.StartPlaying();
            bird.ApplyScoreLook(Score);
        }

        if (pipeSpawner != null)
        {
            pipeSpawner.StartSpawning();
        }

        SetScrolling(true);

        if (gameUi != null)
        {
            gameUi.ShowPlaying(Score);
        }
    }

    public void AddPoint()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        Score++;

        if (gameUi != null)
        {
            gameUi.UpdateScore(Score);
        }

        if (gameAudio != null)
        {
            gameAudio.PlayPoint();
        }

        HandleMilestones();
    }

    private void HandleMilestones()
    {
        if (config.comboSoundEvery > 0 && Score % config.comboSoundEvery == 0)
        {
            if (effects != null)
            {
                effects.PlayCombo();

                if (Score >= config.fireworksFromScore)
                {
                    Vector3 center = bird != null ? bird.transform.position : Vector3.zero;
                    effects.PlayFireworks(center);
                }
            }

            if (gameUi != null)
            {
                gameUi.ShowCheerForScore(Score);
            }
        }

        if (bird != null)
        {
            bird.ApplyScoreLook(Score);
        }

        if (dragon != null && !dragon.IsActive && Score >= config.dragonFromScore)
        {
            dragon.Activate(config, bird != null ? bird.transform : null);
        }
    }

    public void OnPlayerHit()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        State = GameState.GameOver;

        if (effects != null && bird != null)
        {
            effects.BurstFeathers(bird.transform.position, bird.GetTint());
        }

        if (pipeSpawner != null)
        {
            pipeSpawner.StopSpawning();
        }

        SetScrolling(false);

        if (dragon != null)
        {
            dragon.Deactivate();
        }

        if (bird != null)
        {
            bird.Die();
        }

        if (gameAudio != null)
        {
            gameAudio.PlayHit();
        }

        StartCoroutine(ShowGameOverAfterDelay());
    }

    public void Restart()
    {
        StopAllCoroutines();
        Score = 0;

        if (pipeSpawner != null)
        {
            pipeSpawner.ResetPipes();
        }

        if (dragon != null)
        {
            dragon.Deactivate();
        }

        if (effects != null)
        {
            effects.StopAll();
        }

        if (bird != null)
        {
            bird.ResetBird();
            bird.ApplyScoreLook(0);
        }

        EnterReady();
    }

    private void EnterReady()
    {
        State = GameState.Ready;
        SetScrolling(true);

        if (gameUi != null)
        {
            gameUi.ShowReady(BestScore);
        }
    }

    private void SetScrolling(bool enabled)
    {
        if (scrollers == null)
        {
            return;
        }

        for (int i = 0; i < scrollers.Length; i++)
        {
            if (scrollers[i] != null)
            {
                scrollers[i].SetScrolling(enabled);
            }
        }
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(config.gameOverUiDelay);

        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        if (gameUi != null)
        {
            gameUi.ShowGameOver(Score, BestScore);
        }

        if (gameAudio != null)
        {
            gameAudio.PlayDie();
        }
    }
}
