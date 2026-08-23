using UnityEngine;

/// <summary>
/// Loads the generated art and audio that is not wired through the inspector.
/// Everything lives under Resources so builds on other machines behave the same.
/// </summary>
public static class SpriteLibrary
{
    private const string ArtPath = "FlappyArt/";
    private const string AudioPath = "FlappyAudio/";

    private static readonly string[] ShaderCandidates =
    {
        "Universal Render Pipeline/2D/Sprite-Unlit-Default",
        "Universal Render Pipeline/2D/Sprite-Lit-Default",
        "Sprites/Default"
    };

    private static Material spriteMaterial;
    private static bool materialResolved;

    public static Sprite Load(string spriteName)
    {
        return Resources.Load<Sprite>(ArtPath + spriteName);
    }

    public static AudioClip LoadClip(string clipName)
    {
        return Resources.Load<AudioClip>(AudioPath + clipName);
    }

    /// <summary>
    /// URP 2D renders sprites black when they use a lit material without a light.
    /// A single shared unlit material keeps every runtime sprite visible and cheap.
    /// Returns null when no usable shader exists, so callers keep Unity's default
    /// sprite material instead of ending up with an invisible renderer.
    /// </summary>
    public static Material SpriteMaterial
    {
        get
        {
            if (materialResolved)
            {
                return spriteMaterial;
            }

            materialResolved = true;

            for (int i = 0; i < ShaderCandidates.Length; i++)
            {
                Shader shader = Shader.Find(ShaderCandidates[i]);
                if (!IsUsable(shader))
                {
                    continue;
                }

                spriteMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                return spriteMaterial;
            }

            return spriteMaterial;
        }
    }

    private static bool IsUsable(Shader shader)
    {
        return shader != null && shader.isSupported && shader.name != "Hidden/InternalErrorShader";
    }

    public static SpriteRenderer CreateRenderer(GameObject owner, Sprite sprite, string sortingLayer, int order)
    {
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;

        Material material = SpriteMaterial;
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        if (!string.IsNullOrEmpty(sortingLayer) && SortingLayerExists(sortingLayer))
        {
            renderer.sortingLayerName = sortingLayer;
        }

        return renderer;
    }

    private static bool SortingLayerExists(string layerName)
    {
        SortingLayer[] layers = SortingLayer.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name == layerName)
            {
                return true;
            }
        }

        return false;
    }
}
