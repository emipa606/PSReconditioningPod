using UnityEngine;
using Verse;

namespace PS_ReconPod;

[StaticConstructorOnStartup]
public static class PS_TextureLoader
{
    public static Texture2D Warning;

    static PS_TextureLoader()
    {
        LoadTextures();
    }

    public static bool Loaded { get; private set; }

    public static void Reset()
    {
        LongEventHandler.ExecuteWhenFinished(LoadTextures);
    }

    private static void LoadTextures()
    {
        Loaded = false;
        Warning = ContentFinder<Texture2D>.Get("UI/Warning");
        //Textures.TextureAlternateRow = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
        //Textures.TextureSkillBarFill = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
        Loaded = true;
    }
}