using RimWorld;
using UnityEngine;
using Verse;

namespace PS_ReconPod
{
    [StaticConstructorOnStartup]

    internal class PS_ReconPodMod : Mod
    {
        /// <summary>
        /// Cunstructor
        /// </summary>
        /// <param name="content"></param>
        public PS_ReconPodMod(ModContentPack content) : base(content)
        {
            instance = this;
            //var hediff = DefDatabase<HediffDef>.GetNamedSilentFail("PS_Hediff_Reconditioned");
            //Log.Message(hediff.label + ". IsBad: " + hediff.isBad);
            //hediff.isBad = LoadedModManager.GetMod<PS_ReconPodMod>().GetSettings<PS_ReconPodSettings>().RecondIsBad;
            //Log.Message(hediff.label + ". IsBad: " + hediff.isBad);
        }

        /// <summary>
        /// The instance-settings for the mod
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
            set
            {
                settings = value;
            }
        }

        /// <summary>
        /// The title for the mod-settings
        /// </summary>
        /// <returns></returns>
        public override string SettingsCategory()
        {
            return "PS Reconditioning Pod";
        }

        /// <summary>
        /// The settings-window
        /// For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
        /// </summary>
        /// <param name="rect"></param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.CheckboxLabeled("Count 'reconditioned' as bad", ref Settings.RecondIsBad, "This makes it possible to remove with mods that remove bad Hediffs, such as MedPod");
            listing_Standard.End();
            Settings.Write();
            RecondApplyer.Apply();
        }

        /// <summary>
        /// The instance of the settings to be read by the mod
        /// </summary>
        public static PS_ReconPodMod instance;

        /// <summary>
        /// The private settings
        /// </summary>
        private PS_ReconPodSettings settings;

    }
}
