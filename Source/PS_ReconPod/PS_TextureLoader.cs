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
        Loaded = true;
    }
}