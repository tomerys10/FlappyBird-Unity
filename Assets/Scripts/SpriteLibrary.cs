using UnityEngine;

/// <summary>
/// Loads the generated art and audio that is not wired through the inspector.
/// Everything lives under Resources so builds on other machines behave the same.
/// </summary>
public static class SpriteLibrary
{
    private const string ArtPath = "FlappyArt/";
    private const string AudioPath = "FlappyAudio/";

    private static Material spriteMaterial;

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
    /// </summary>
    public static Material SpriteMaterial
    {
        get
        {
            if (spriteMaterial != null)
            {
                return spriteMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                spriteMaterial = new Material(shader);
            }

            return spriteMaterial;
        }
    }

    public static SpriteRenderer CreateRenderer(GameObject owner, Sprite sprite, string sortingLayer, int order)
    {
        SpriteRenderer renderer = owner.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = SpriteMaterial;
        renderer.sortingOrder = order;

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
