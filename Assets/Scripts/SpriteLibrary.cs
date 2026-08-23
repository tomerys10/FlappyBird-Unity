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
    /// Sprites created at runtime have to match the ones placed in the scene, which
    /// use an unlit material so they never depend on a 2D light being present.
    /// Reusing the scene material is the most reliable option because it is a real
    /// asset; building one from a shader only happens when the scene has no sprite.
    /// Returns null when nothing usable is found, so callers keep Unity's default
    /// sprite material rather than ending up with an invisible renderer.
    /// </summary>
    public static Material SpriteMaterial
    {
        get
        {
            if (materialResolved && spriteMaterial != null)
            {
                return spriteMaterial;
            }

            materialResolved = true;
            spriteMaterial = FindSceneSpriteMaterial() ?? BuildSpriteMaterial();
            return spriteMaterial;
        }
    }

    private static Material FindSceneSpriteMaterial()
    {
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i] != null ? renderers[i].sharedMaterial : null;
            if (material != null && IsUsable(material.shader))
            {
                return material;
            }
        }

        return null;
    }

    private static Material BuildSpriteMaterial()
    {
        for (int i = 0; i < ShaderCandidates.Length; i++)
        {
            Shader shader = Shader.Find(ShaderCandidates[i]);
            if (IsUsable(shader))
            {
                return new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
        }

        return null;
    }

    private static bool IsUsable(Shader shader)
    {
        return shader != null && shader.isSupported && shader.name != "Hidden/InternalErrorShader";
    }

    public static SpriteRenderer CreateRenderer(GameObject owner, Sprite sprite, string sortingLayer, int order)
    {
        Material material = SpriteMaterial;

        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;

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
