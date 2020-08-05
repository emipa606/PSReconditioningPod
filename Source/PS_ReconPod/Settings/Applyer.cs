using System;
using Verse;

namespace PS_ReconPod
{
    [StaticConstructorOnStartup]
    public class RecondApplyer
    {
        static RecondApplyer()
        {
            Apply();
        }

        public static void Apply()
        {
            var def = DefDatabase<HediffDef>.GetNamed("PS_Hediff_Reconditioned");
            if (def.isBad != LoadedModManager.GetMod<PS_ReconPodMod>().GetSettings<PS_ReconPodSettings>().RecondIsBad)
            {
                def.isBad = LoadedModManager.GetMod<PS_ReconPodMod>().GetSettings<PS_ReconPodSettings>().RecondIsBad;
                Log.Message("PS Reconditioning Pod: reconditioning counts as bad: " + def.isBad);
            }
        }
    }
}
