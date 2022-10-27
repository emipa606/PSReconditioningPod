using Mlie;
using UnityEngine;
using Verse;

namespace PS_ReconPod;

[StaticConstructorOnStartup]
internal class PS_ReconPodMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static PS_ReconPodMod instance;

    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    private PS_ReconPodSettings settings;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public PS_ReconPodMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.PSReconditioningPod"));
        //var hediff = DefDatabase<HediffDef>.GetNamedSilentFail("PS_Hediff_Reconditioned");
        //Log.Message(hediff.label + ". IsBad: " + hediff.isBad);
        //hediff.isBad = LoadedModManager.GetMod<PS_ReconPodMod>().GetSettings<PS_ReconPodSettings>().RecondIsBad;
        //Log.Message(hediff.label + ". IsBad: " + hediff.isBad);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal PS_ReconPodSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<PS_ReconPodSettings>();
            }

            return settings;
        }
        set => settings = value;
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "PS Reconditioning Pod";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.CheckboxLabeled("PS_IsBad".Translate(), ref Settings.RecondIsBad,
            "PS_IsBadInfo".Translate());
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("PS_ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        RecondApplyer.Apply();
    }
}