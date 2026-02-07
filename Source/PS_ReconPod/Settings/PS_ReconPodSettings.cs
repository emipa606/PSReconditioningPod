using Verse;

namespace PS_ReconPod;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class PS_ReconPodSettings : ModSettings
{
    public bool RecondDecays;
    public bool RecondIsBad;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref RecondIsBad, "RecondIsBad");
        Scribe_Values.Look(ref RecondDecays, "RecondDecays", true);
    }
}