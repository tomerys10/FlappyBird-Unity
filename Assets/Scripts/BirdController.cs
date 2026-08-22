using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BirdController : MonoBehaviour
{
    [SerializeField] private Sprite[] flapFrames;
    [SerializeField] private GameAudio gameAudio;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Sprite[] altFrames;
    private Sprite[] activeFrames;
    private Vector3 startPosition;
    private float idleTime;
    private float animTime;
    private bool dead;

    private GameConfig Config => GameManager.Instance.Config;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        activeFrames = flapFrames;
        altFrames = LoadAltFrames();
    }

    private static Sprite[] LoadAltFrames()
    {
        Sprite frame0 = SpriteLibrary.Load("bird_alt_0");
        Sprite frame1 = SpriteLibrary.Load("bird_alt_1");
        Sprite frame2 = SpriteLibrary.Load("bird_alt_2");

        if (frame0 == null || frame1 == null || frame2 == null)
        {
            return null;
        }

        return new[] { frame0, frame1, frame2 };
    }

    private void Start()
    {
        ResetBird();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameState state = GameManager.Instance.State;

        if (state == GameState.Ready)
        {
            idleTime += Time.deltaTime;
            float bob = Mathf.Sin(idleTime * Config.idleBobSpeed) * Config.idleBobAmplitude;
            transform.position = new Vector3(startPosition.x, startPosition.y + bob, startPosition.z);
            transform.rotation = Quaternion.identity;
            AnimateFlap();
            return;
        }

        if (state == GameState.Playing && FlapInput.WasPressedThisFrame())
        {
            Flap();
        }

        AnimateFlap();
        UpdateTilt();
        ClampInsideWorld();
    }

    public void SetTint(Color tint)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.color = tint;
    }

    public Color GetTint()
    {
        return spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    /// <summary>
    /// Swaps to the second bird design once the player passes the milestone score.
    /// </summary>
    public void ApplyScoreLook(int score)
    {
        bool useAlt = altFrames != null && GameManager.Instance != null && score >= Config.birdSwapScore;
        Sprite[] wanted = useAlt ? altFrames : flapFrames;

        if (wanted == null || wanted.Length == 0 || wanted == activeFrames)
        {
            return;
        }

        activeFrames = wanted;
        spriteRenderer.sprite = activeFrames[0];
    }

    public void StartPlaying()
    {
        dead = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Config.gravityScale;
        Flap();
    }

    public void ResetBird()
    {
        dead = false;
        idleTime = 0f;
        animTime = 0f;
        transform.SetPositionAndRotation(startPosition, Quaternion.identity);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (activeFrames != null && activeFrames.Length > 0)
        {
            spriteRenderer.sprite = activeFrames[0];
        }

        spriteRenderer.color = PlayerPrefsHasTint()
            ? ReadSavedTint()
            : spriteRenderer.color;
    }

    private static bool PlayerPrefsHasTint()
    {
        return PlayerPrefs.HasKey("FlappyBird.TintR");
    }

    private static Color ReadSavedTint()
    {
        return new Color(
            PlayerPrefs.GetFloat("FlappyBird.TintR", 1f),
            PlayerPrefs.GetFloat("FlappyBird.TintG", 1f),
            PlayerPrefs.GetFloat("FlappyBird.TintB", 1f),
            1f);
    }

    public void Die()
    {
        dead = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Config.gravityScale * 1.15f;
    }

    private void Flap()
    {
        if (dead)
        {
            return;
        }

        rb.linearVelocity = new Vector2(0f, Config.flapVelocity);

        if (gameAudio != null)
        {
            gameAudio.PlayFlap();
        }
    }

    private void AnimateFlap()
    {
        if (activeFrames == null || activeFrames.Length == 0 || dead)
        {
            return;
        }

        animTime += Time.deltaTime * Config.flapAnimFps;
        int frame = Mathf.FloorToInt(animTime) % activeFrames.Length;
        spriteRenderer.sprite = activeFrames[frame];
    }

    private void UpdateTilt()
    {
        float t = Mathf.InverseLerp(Config.maxFallSpeed, Config.flapVelocity, rb.linearVelocity.y);
        float angle = Mathf.Lerp(Config.rotateDownAngle, Config.rotateUpAngle, t);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0f, 0f, angle),
            Config.rotateLerp * Time.deltaTime);
    }

    private void ClampInsideWorld()
    {
        if (transform.position.y > Config.ceilingY)
        {
            transform.position = new Vector3(transform.position.x, Config.ceilingY, transform.position.z);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReportHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ReportHit(other);
    }

    private void ReportHit(Collider2D other)
    {
        if (other != null && other.CompareTag("Hazard") && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerHit();
        }
    }
}
