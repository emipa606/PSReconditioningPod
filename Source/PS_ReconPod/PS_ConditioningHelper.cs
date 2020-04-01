using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace PS_ReconPod
{
    public enum TraitAlterType
    {
        UNSET = -1,
        Added = 0,
        Removed = 1,
        Altered = 2
    }

    public class PS_Conditioning_Data : IExposable
    {
        public string PawnId;
        public string AddedTraitDefName;
        public string OriginalTraitDefName;
        public int AddedDegree = 0;
        public int OriginalDegree = 0;
        public TraitAlterType AlterType = TraitAlterType.UNSET;

        public string AddedTraitLabel
        {
            get
            {
                return new Trait(DefDatabase<TraitDef>.GetNamed(this.AddedTraitDefName), degree: this.AddedDegree).Label;
            }
        }

        public string OriginalTraitLabel
        {
            get
            {
                return new Trait(DefDatabase<TraitDef>.GetNamed(this.OriginalTraitDefName), degree: this.OriginalDegree).Label;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.PawnId, "PawnId");
            Scribe_Values.Look<string>(ref this.AddedTraitDefName, "AddedTraitDefName");
            Scribe_Values.Look<string>(ref this.OriginalTraitDefName, "OrigonalTraitDefName");
            Scribe_Values.Look<int>(ref this.AddedDegree, "AddedDegree");
            Scribe_Values.Look<int>(ref this.OriginalDegree, "OrigonalDegree");
            Scribe_Values.Look<TraitAlterType>(ref this.AlterType, "AlterType");
        }

        public override string ToString()
        {
            return string.Format("[PawnId: {0}, Change: {1} OrgTrait: {2}:{3} AddTrait: {4}:{5}]", this.PawnId, this.AlterType.ToString(), this.OriginalTraitDefName, this.OriginalDegree, this.AddedTraitDefName, this.AddedDegree);
        }

        public string ToPrettyString()
        {
            switch (this.AlterType)
            {
                case TraitAlterType.Added:
                    return "PS_HediffReconditionedAddedLab".Translate() + " " + AddedTraitLabel;

                case TraitAlterType.Removed:
                    return "PS_HediffReconditionedRemovedLab".Translate() + " " + OriginalTraitLabel;

                case TraitAlterType.Altered:
                    return "PS_HediffReconditionedAlteredLab".Translate() + " " + OriginalTraitLabel;

                case TraitAlterType.UNSET:
                    Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with ChangeType = UNSET.");
                    return "ERROR";

                default:
                    Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with unknown ChangeType.");
                    return "ERROR";
            }
        }

        public string ToShortPrettyString()
        {
            switch (this.AlterType)
            {
                case TraitAlterType.Added:
                    return "PS_Added".Translate() + " " + AddedTraitLabel;

                case TraitAlterType.Removed:
                    return "PS_Removed".Translate() + " " + OriginalTraitLabel;

                case TraitAlterType.Altered:
                    return "PS_Changed".Translate() + " " + OriginalTraitLabel;

                case TraitAlterType.UNSET:
                    Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with ChangeType = UNSET.");
                    return "ERROR";

                default:
                    Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with unknown ChangeType.");
                    return "ERROR";
            }
        }

        public bool IsValid()
        {
            switch (this.AlterType)
            {
                case TraitAlterType.UNSET:
                    return false;
                case TraitAlterType.Added:
                    return !string.IsNullOrEmpty(this.AddedTraitDefName);
                case TraitAlterType.Removed:
                    return !string.IsNullOrEmpty(this.OriginalTraitDefName);
                case TraitAlterType.Altered:
                    return !string.IsNullOrEmpty(this.AddedTraitDefName) && !string.IsNullOrEmpty(this.OriginalTraitDefName);
                default:
                    return false;
            }
            //return (this.AlterType != TraitAlterType.UNSET) && (!string.IsNullOrEmpty(this.OrigonalTraitDefName) || !string.IsNullOrEmpty(this.AddedTraitDefName));
        }

        public bool IsSame(PS_Conditioning_Data conData)
        {
            return (this.AlterType == conData.AlterType && this.OriginalTraitDefName == conData.OriginalTraitDefName && this.OriginalDegree == conData.OriginalDegree && this.AddedTraitDefName == conData.AddedTraitDefName && this.AddedDegree == conData.AddedDegree);
        }
    }

    public static class PS_ConditioningHelper
    {

        private static HediffDef _ReconHediffDef;
        public static HediffDef ReconHefiffDef
        {
            get
            {
                if (_ReconHediffDef == null)
                    _ReconHediffDef = DefDatabase<HediffDef>.GetNamed("PS_Hediff_Reconditioned");
                return _ReconHediffDef;
            }
        }

        public static void DirtyNeedFall(Pawn pawn)
        {
            if (!IsReconditioned(pawn))
                return;
            var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
            if (need == null)
            {
                Log.Error("PS_Hediff_Giver: Tried to DIRTYNeedFall but failed to find need");
                return;
            }
            need.FallPerDayIsDirty = true;
        }

        public static float DaysToCondition(Pawn pawn, bool Deconditioning = false)
        {
            if (!IsReconditioned(pawn))
                return DaysToCondition(0);
            else
            {
                var count = GetConditioningDataFromHediff(pawn)?.Count() ?? 0;
                if (Deconditioning && count > 0)
                    count--;
                return DaysToCondition(count);
            }
        }

        public static float DaysToCondition(int CurrentConCount)
        {
            return 0.5f * ((float)CurrentConCount) + 1f;
        }

        public static float GetSucessChance(Pawn pawn)
        {
            if (!IsReconditioned(pawn))
                return GetSucessChance(0);
            else
            {
                var count = GetConditioningDataFromHediff(pawn)?.Count() ?? 0;
                return GetSucessChance(count);
            }
        }

        public static float GetSucessChance(int CurrentConCount)
        {
            return 0.9f - (0.1f * ((float)CurrentConCount));
        }

        public static float GetRefreshPerDay(int CurrentConCount)
        {
            if (CurrentConCount == 0)
                return 0;
            //if (CurrentConCount == 1)
            //    return 2;

            //return 2f/ (float)(Math.Pow(2d, (double)(CurrentConCount - 1)));
            else
                return 0.5f / GetNeedFallPerDay(CurrentConCount);
        }

        public static float GetNeedFallPerDay(Pawn pawn)
        {
            if (!IsReconditioned(pawn))
                return GetNeedFallPerDay(0);
            else
            {
                var count = GetConditioningDataFromHediff(pawn)?.Count() ?? 0;
                return GetNeedFallPerDay(count);
            }
        }

        public static float GetNeedFallPerDay(int CurrentConCount)
        {
            if (CurrentConCount == 0)
                return 0;
            return 0.125f * (float)(Math.Pow(2d, (double)(CurrentConCount - 1)));

        }


        public static bool IsReconditioned(Pawn pawn)
        {
            var hasHediff = pawn.health.hediffSet.hediffs.Where(x => x.def.defName == "PS_Hediff_Reconditioned").Any();
            var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
            var hasNeed = (need != null);
            if (hasHediff != hasNeed)
            {
                Log.Error("PS_ConditioningHelper: hasNeed hasHediff miss match. Pawn: " + pawn.LabelShort + " hasNeed: " + hasNeed + " has hediff: " + hasHediff);
            }
            return (hasHediff && hasNeed);
        }

        public static bool IsCemented(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Where(x => x.def.defName == "PS_Hediff_NeuralCement").Any();
        }

        public static List<PS_Conditioning_Data> GetConditioningDataFromHediff(Pawn pawn, bool ThrowError = true)
        {
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                if (ThrowError)
                    Log.Error("PS_ConditioningHelper: Tried to GetConditioningDataFromHediff but failed to find hediff");
                return null;
            }
            return hediff.ConditioningDataList?.Where(x => x?.IsValid() ?? false).ToList();
        }

        public static float GetHediffSeverity(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                Log.Error("PS_Hediff_Giver: Tried to GetReconServerity but failed to find hediff");
                return -1f;
            }
            return hediff.Severity;
        }

        public static bool SetHediffSeverity(Pawn pawn, float severity)
        {
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                Log.Error("PS_Hediff_Giver: Tried to SetReconServerity but failed to find hediff");
                return false;
            }
            hediff.Severity = severity;
            return true;
        }

        public static float GetCurrentNeedLevel(Pawn pawn)
        {
            if (!IsReconditioned(pawn))
                return -1f;

            var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
            if (need == null)
            {
                Log.Error("PS_Hediff_Giver: Tried to GetCurrentConditioningLevel but failed to find need");
                return -1f;
            }
            return need.CurLevel;
        }

        public static bool SetCurrentNeedLevel(Pawn pawn, float level)
        {
            if (!IsReconditioned(pawn))
                return false;

            var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
            if (need == null)
            {
                Log.Error("PS_Hediff_Giver: Tried to SetCurrentConditioningLevel but failed to find need");
                return false;
            }
            need.CurLevel = level;
            return true;
        }

        public static void DoConditioning(Pawn pawn, PS_Buildings_ReconPod pod, PS_Conditioning_Data conData)
        {
            if (Prefs.DevMode) Log.Message("PS_ConditioningHelper: Doing Conditioning Pawn: " + pawn.LabelShort + " AddedTrait: " + conData.AddedTraitDefName + " OrigonalTrait: " + conData.OriginalTraitDefName + " AlterType: " + conData.AlterType.ToString());
            if (!IsReconditioned(pawn))
            {
                var hediff = TryGiveReconditioning(pawn, conData);
                if (hediff != null)
                {
                    DoTraitChange(pawn, conData);
                }
                else
                    Log.Error("PS_ConditioningHelper: Failed to create hediff");
            }
            else
            {
                var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
                if (hediff == null)
                {
                    Log.Error("PS_CondidioningHelper: Attempted to add conditioning, but failed to find reconditioned hediff");
                    return;
                }
                hediff.ConditioningDataList.Add(conData);
                DoTraitChange(pawn, conData);

            }
        }

        public static void UndoConditioning(Pawn pawn, PS_Buildings_ReconPod pod, PS_Conditioning_Data conData)
        {
            UndoTraitChange(pawn, conData);
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            hediff.ConditioningDataList.Remove(conData);
            if (!hediff.ConditioningDataList.Where(x => x.IsValid()).Any())
                TryRemoveConditioning(pawn);

        }

        public static void UndoAllConditioning(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                Log.Error("PS_CondidioningHelper: Attempted to undo all conditioning, but failed to find reconditioned hediff");
                return;
            }
            foreach (var data in hediff.ConditioningDataList)
                UndoTraitChange(pawn, data);

            TryRemoveConditioning(pawn);

        }
        public static PS_Hediff_Reconditioned TryGiveReconditioning(Pawn pawn, PS_Conditioning_Data conData)
        {
            var diff = (PS_Hediff_Reconditioned)HediffMaker.MakeHediff(ReconHefiffDef, pawn);
            diff.Severity = 1f;
            diff.ConditioningDataList = new List<PS_Conditioning_Data> { conData };
            var brain = pawn.RaceProps.body.AllParts.Where(x => x.def.defName == "Brain").FirstOrDefault();
            if (brain != null)
                diff.Part = brain;

            pawn.health.AddHediff(diff);
            pawn.needs.AddOrRemoveNeedsAsAppropriate();

            var need = pawn.needs.AllNeeds.Where(x => x.def.defName == diff.def.causesNeed.defName).FirstOrDefault();
            if (need == null)
                Log.Error("PS_ConditioningHelper: Failed to find added need after giving hediff");
            else
                need.CurLevel = 1f;
            return diff;
        }

        public static void TryRemoveConditioning(Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                Log.Error("PS_ConditioningHelper: Tried to remove conditioning hediff but failed to find hediff");
                return;
            }

            pawn.health.hediffSet.hediffs.Remove(hediff);
            pawn.needs.AddOrRemoveNeedsAsAppropriate();
        }

        public static void DoTraitChange(Pawn pawn, PS_Conditioning_Data conData)
        {
            if (conData.AlterType == TraitAlterType.Removed || conData.AlterType == TraitAlterType.Altered)
            {
                if (Prefs.DevMode) Log.Message($"Will try to remove trait {conData.OriginalTraitDefName}");
                var traitList = new List<Trait>();
                foreach(Trait trait in pawn.story.traits.allTraits)
                {
                    if(trait.def.defName != conData.OriginalTraitDefName || trait.Degree != conData.OriginalDegree)
                    {
                        traitList.Add(trait);
                    }
                }
                pawn.story.traits.allTraits = traitList;
            }

            if (conData.AlterType == TraitAlterType.Added || conData.AlterType == TraitAlterType.Altered)
            {
                if (Prefs.DevMode) Log.Message($"Will try to add trait {conData.AddedTraitDefName}");
                pawn.story.traits.allTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(conData.AddedTraitDefName), degree: conData.AddedDegree));
            }

            if (pawn.workSettings != null)
            {
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
            }
            //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
            if (Prefs.DevMode) Log.Message($"Will try to update story");
            try
            {
                typeof(Pawn_StoryTracker).GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, null);
            }
            catch (Exception)
            {
            }
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        public static void RemoveBotch(Pawn pawn)
        {
            var traitList = new List<Trait>();
            foreach (Trait currentTrait in pawn.story.traits.allTraits)
            {
                if (currentTrait.def.defName != "PS_Trait_BotchedConditioning")
                {
                    traitList.Add(currentTrait);
                }
            }
            pawn.story.traits.allTraits = traitList;

            if (pawn.workSettings != null)
            {
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
            }
            //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
            try
            {
                typeof(Pawn_StoryTracker).GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, null);
            }
            catch (Exception)
            {
            }
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        public static void UndoTraitChange(Pawn pawn, PS_Conditioning_Data conData)
        {
            //Log.Message(string.Format("PS_ReconPod: Undoing Trait Change Pawn: {0}, ConData: {1}", pawn.LabelShort, conData.ToString()));
            if (conData.AlterType == TraitAlterType.Added || conData.AlterType == TraitAlterType.Altered)
            {
                var traitList = new List<Trait>();
                foreach (Trait currentTrait in pawn.story.traits.allTraits)
                {
                    if (currentTrait.def.defName != conData.AddedTraitDefName || currentTrait.Degree != conData.AddedDegree)
                    {
                        traitList.Add(currentTrait);
                    }
                }
                pawn.story.traits.allTraits = traitList;
            }

            if (conData.AlterType == TraitAlterType.Removed || conData.AlterType == TraitAlterType.Altered)
            {
                pawn.story.traits.allTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(conData.OriginalTraitDefName), degree: conData.OriginalDegree));
                //Log.Message("PS_ConditoningHelper: UndoTraitChange added trait " + conData.OrigonalTraitDefName + " degree: " + conData.OrigonalDegree);
            }

            if (pawn.workSettings != null)
            {
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
            }
            //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
            try
            {
                typeof(Pawn_StoryTracker).GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, null);
            }
            catch (Exception)
            {
            }
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        public static List<Trait> GetNaturalTraits(Pawn pawn)
        {
            var outTraits = new List<Trait>();
            foreach (var trait in pawn.story.traits.allTraits)
                outTraits.Add(trait);

            var condata = GetConditioningDataFromHediff(pawn);

            if (condata != null)
            {
                foreach (var data in condata)
                {
                    if (data.AlterType == TraitAlterType.Added || data.AlterType == TraitAlterType.Altered)
                        outTraits.Remove(outTraits.Where(x => x.def.defName != data.AddedTraitDefName).FirstOrDefault());

                    if (data.AlterType == TraitAlterType.Removed || data.AlterType == TraitAlterType.Altered)
                        outTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(data.OriginalTraitDefName)));
                }
            }
            return outTraits;
        }

        public static void ClearBuggedConditioning(Pawn pawn)
        {
            var hediffs = pawn?.health?.hediffSet?.GetHediffs<PS_Hediff_Reconditioned>();
            if (hediffs == null || hediffs.Count() <= 0)
                return;

            foreach (var hediff in hediffs)
                pawn.health.RemoveHediff(hediff);

            pawn.needs.AddOrRemoveNeedsAsAppropriate();

            var pod = PS_PodFinder.FindMyPod(pawn);
            if (pod == null)
                return;

            pod.ForceUnassignPawn(pawn);
        }
    }
}
