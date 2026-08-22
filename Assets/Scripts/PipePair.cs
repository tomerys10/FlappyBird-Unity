using UnityEngine;

public class PipePair : MonoBehaviour
{
    [SerializeField] private Transform topPipe;
    [SerializeField] private Transform bottomPipe;

    private static Transform bird;
    private static Sprite pipeSprite;

    private bool scored;

    private void Awake()
    {
        EnsurePipes();

        if (bird == null)
        {
            BirdController found = FindFirstObjectByType<BirdController>();
            if (found != null)
            {
                bird = found.transform;
            }
        }
    }

    public void Place(float x, float gapCenterY)
    {
        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config == null || topPipe == null || bottomPipe == null)
        {
            return;
        }

        scored = false;
        float halfGap = config.pipeGap * 0.5f;

        transform.position = new Vector3(x, 0f, 0f);
        topPipe.localPosition = new Vector3(0f, gapCenterY + halfGap, 0f);
        bottomPipe.localPosition = new Vector3(0f, gapCenterY - halfGap, 0f);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.State != GameState.Playing)
        {
            return;
        }

        GameConfig config = manager.Config;
        transform.Translate(Vector3.left * config.scrollSpeed * Time.deltaTime);

        float birdX = bird != null ? bird.position.x : -1.4f;
        if (!scored && transform.position.x <= birdX)
        {
            scored = true;
            manager.AddPoint();
        }

        if (transform.position.x < config.pipeDespawnX)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// The prefab may reach the game without its two halves linked, so the pair
    /// rebuilds anything that is missing instead of throwing every frame.
    /// </summary>
    private void EnsurePipes()
    {
        if (bottomPipe == null)
        {
            bottomPipe = transform.Find("PipeBottom");
        }

        if (topPipe == null)
        {
            topPipe = transform.Find("PipeTop");
        }

        if (bottomPipe == null)
        {
            bottomPipe = CreatePipe("PipeBottom", false);
        }

        if (topPipe == null)
        {
            topPipe = CreatePipe("PipeTop", true);
        }
    }

    private Transform CreatePipe(string childName, bool flipped)
    {
        var go = new GameObject(childName);
        go.tag = "Hazard";
        go.transform.SetParent(transform, false);
        go.transform.localScale = flipped ? new Vector3(1f, -1f, 1f) : Vector3.one;

        SpriteLibrary.CreateRenderer(go, GetPipeSprite(), "Pipes", 10);
        go.AddComponent<BoxCollider2D>();
        return go.transform;
    }

    private static Sprite GetPipeSprite()
    {
        if (pipeSprite != null)
        {
            return pipeSprite;
        }

        pipeSprite = SpriteLibrary.Load("pipe");
        if (pipeSprite != null)
        {
            return pipeSprite;
        }

        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Sprite sprite = renderers[i].sprite;
            if (sprite != null && sprite.name.StartsWith("pipe", System.StringComparison.OrdinalIgnoreCase))
            {
                pipeSprite = sprite;
                return pipeSprite;
            }
        }

        return null;
    }
}
