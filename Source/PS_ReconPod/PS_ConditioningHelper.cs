using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public static class PS_ConditioningHelper
{
    private static HediffDef _ReconHediffDef;

    public static HediffDef ReconHefiffDef
    {
        get
        {
            if (_ReconHediffDef == null)
            {
                _ReconHediffDef = DefDatabase<HediffDef>.GetNamed("PS_Hediff_Reconditioned");
            }

            return _ReconHediffDef;
        }
    }

    public static void DirtyNeedFall(Pawn pawn)
    {
        if (!IsReconditioned(pawn))
        {
            return;
        }

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
        {
            return DaysToCondition(0);
        }

        var count = GetConditioningDataFromHediff(pawn)?.Count ?? 0;
        if (Deconditioning && count > 0)
        {
            count--;
        }

        return DaysToCondition(count);
    }

    public static float DaysToCondition(int CurrentConCount)
    {
        return (0.5f * CurrentConCount) + 1f;
    }

    public static float GetSucessChance(Pawn pawn)
    {
        if (!IsReconditioned(pawn))
        {
            return GetSucessChance(0);
        }

        var count = GetConditioningDataFromHediff(pawn)?.Count ?? 0;
        return GetSucessChance(count);
    }

    public static float GetSucessChance(int CurrentConCount)
    {
        return 0.9f - (0.1f * CurrentConCount);
    }

    public static float GetRefreshPerDay(int CurrentConCount)
    {
        if (CurrentConCount == 0)
        {
            return 0;
        }
        //if (CurrentConCount == 1)
        //    return 2;

        //return 2f/ (float)(Math.Pow(2d, (double)(CurrentConCount - 1)));

        return 0.5f / GetNeedFallPerDay(CurrentConCount);
    }

    public static float GetNeedFallPerDay(Pawn pawn)
    {
        if (!IsReconditioned(pawn))
        {
            return GetNeedFallPerDay(0);
        }

        var count = GetConditioningDataFromHediff(pawn)?.Count ?? 0;
        return GetNeedFallPerDay(count);
    }

    public static float GetNeedFallPerDay(int CurrentConCount)
    {
        if (CurrentConCount == 0)
        {
            return 0;
        }

        return 0.125f * (float)Math.Pow(2d, CurrentConCount - 1);
    }


    public static bool IsReconditioned(Pawn pawn)
    {
        var hasHediff = pawn.health.hediffSet.hediffs.Any(x => x.def.defName == "PS_Hediff_Reconditioned");
        var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
        var hasNeed = need != null;
        if (hasHediff != hasNeed)
        {
            Log.Error(
                $"PS_ConditioningHelper: hasNeed hasHediff miss match. Pawn: {pawn.LabelShort} hasNeed: {hasNeed} has hediff: {hasHediff}");
        }

        return hasHediff && hasNeed;
    }

    public static bool IsCemented(Pawn pawn)
    {
        return pawn.health.hediffSet.hediffs.Any(x => x.def.defName == "PS_Hediff_NeuralCement");
    }

    public static List<PS_Conditioning_Data> GetConditioningDataFromHediff(Pawn pawn, bool ThrowError = true)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
        if (hediff != null)
        {
            return hediff.ConditioningDataList?.Where(x => x?.IsValid() ?? false).ToList();
        }

        if (ThrowError)
        {
            Log.Error(
                "PS_ConditioningHelper: Tried to GetConditioningDataFromHediff but failed to find hediff");
        }

        return null;
    }

    public static float GetHediffSeverity(Pawn pawn)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
        if (hediff != null)
        {
            return hediff.Severity;
        }

        Log.Error("PS_Hediff_Giver: Tried to GetReconServerity but failed to find hediff");
        return -1f;
    }

    public static bool SetHediffSeverity(Pawn pawn, float severity)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
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
        {
            return -1f;
        }

        var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
        if (need != null)
        {
            return need.CurLevel;
        }

        Log.Error("PS_Hediff_Giver: Tried to GetCurrentConditioningLevel but failed to find need");
        return -1f;
    }

    public static bool SetCurrentNeedLevel(Pawn pawn, float level)
    {
        if (!IsReconditioned(pawn))
        {
            return false;
        }

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
        if (Prefs.DevMode)
        {
            Log.Message(
                $"PS_ConditioningHelper: Doing Conditioning Pawn: {pawn.LabelShort} AddedTrait: {conData.AddedTraitDefName} OrigonalTrait: {conData.OriginalTraitDefName} AlterType: {conData.AlterType}");
        }

        if (!IsReconditioned(pawn))
        {
            var hediff = TryGiveReconditioning(pawn, conData);
            if (hediff != null)
            {
                DoTraitChange(pawn, conData);
            }
            else
            {
                Log.Error("PS_ConditioningHelper: Failed to create hediff");
            }
        }
        else
        {
            var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
            if (hediff == null)
            {
                Log.Error(
                    "PS_CondidioningHelper: Attempted to add conditioning, but failed to find reconditioned hediff");
                return;
            }

            hediff.ConditioningDataList.Add(conData);
            DoTraitChange(pawn, conData);
        }
    }

    public static void UndoConditioning(Pawn pawn, PS_Buildings_ReconPod pod, PS_Conditioning_Data conData)
    {
        UndoTraitChange(pawn, conData);
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
        hediff?.ConditioningDataList.Remove(conData);
        if (hediff != null && !hediff.ConditioningDataList.Any(x => x.IsValid()))
        {
            TryRemoveConditioning(pawn);
        }
    }

    public static void UndoAllConditioning(Pawn pawn)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
        if (hediff == null)
        {
            Log.Error(
                "PS_CondidioningHelper: Attempted to undo all conditioning, but failed to find reconditioned hediff");
            return;
        }

        foreach (var data in hediff.ConditioningDataList)
        {
            UndoTraitChange(pawn, data);
        }

        TryRemoveConditioning(pawn);
    }

    public static PS_Hediff_Reconditioned TryGiveReconditioning(Pawn pawn, PS_Conditioning_Data conData)
    {
        var diff = (PS_Hediff_Reconditioned)HediffMaker.MakeHediff(ReconHefiffDef, pawn);
        diff.Severity = 1f;
        diff.ConditioningDataList = new List<PS_Conditioning_Data> { conData };
        var brain = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def.defName == "Brain");
        if (brain != null)
        {
            diff.Part = brain;
        }

        pawn.health.AddHediff(diff);
        pawn.needs.AddOrRemoveNeedsAsAppropriate();

        var need = pawn.needs.AllNeeds.FirstOrDefault(x => x.def.defName == diff.def.causesNeed.defName);
        if (need == null)
        {
            Log.Error("PS_ConditioningHelper: Failed to find added need after giving hediff");
        }
        else
        {
            need.CurLevel = 1f;
        }

        return diff;
    }

    public static void TryRemoveConditioning(Pawn pawn)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
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
        if (conData.AlterType is TraitAlterType.Removed or TraitAlterType.Altered)
        {
            if (Prefs.DevMode)
            {
                Log.Message($"Will try to remove trait {conData.OriginalTraitDefName}");
            }

            var traitList = new List<Trait>();
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.def.defName != conData.OriginalTraitDefName || trait.Degree != conData.OriginalDegree)
                {
                    traitList.Add(trait);
                }
            }

            pawn.story.traits.allTraits = traitList;
        }

        if (conData.AlterType is TraitAlterType.Added or TraitAlterType.Altered)
        {
            if (Prefs.DevMode)
            {
                Log.Message($"Will try to add trait {conData.AddedTraitDefName}");
            }

            pawn.story.traits.allTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(conData.AddedTraitDefName),
                conData.AddedDegree));
        }

        pawn.workSettings?.Notify_DisabledWorkTypesChanged();

        //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
        if (Prefs.DevMode)
        {
            Log.Message("Will try to update story");
        }

        try
        {
            typeof(Pawn_StoryTracker)
                .GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(pawn.story, null);
        }
        catch (Exception)
        {
            // ignored
        }

        pawn.skills?.Notify_SkillDisablesChanged();

        if (!pawn.Dead && pawn.RaceProps.Humanlike)
        {
            pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
        }
    }

    public static void RemoveBotch(Pawn pawn)
    {
        var traitList = new List<Trait>();
        foreach (var currentTrait in pawn.story.traits.allTraits)
        {
            if (currentTrait.def.defName != "PS_Trait_BotchedConditioning")
            {
                traitList.Add(currentTrait);
            }
        }

        pawn.story.traits.allTraits = traitList;

        pawn.workSettings?.Notify_DisabledWorkTypesChanged();

        //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
        try
        {
            typeof(Pawn_StoryTracker)
                .GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(pawn.story, null);
        }
        catch (Exception)
        {
            // ignored
        }

        pawn.skills?.Notify_SkillDisablesChanged();

        if (!pawn.Dead && pawn.RaceProps.Humanlike)
        {
            pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
        }
    }

    public static void UndoTraitChange(Pawn pawn, PS_Conditioning_Data conData)
    {
        //Log.Message(string.Format("PS_ReconPod: Undoing Trait Change Pawn: {0}, ConData: {1}", pawn.LabelShort, conData.ToString()));
        if (conData.AlterType is TraitAlterType.Added or TraitAlterType.Altered)
        {
            var traitList = new List<Trait>();
            foreach (var currentTrait in pawn.story.traits.allTraits)
            {
                if (currentTrait.def.defName != conData.AddedTraitDefName ||
                    currentTrait.Degree != conData.AddedDegree)
                {
                    traitList.Add(currentTrait);
                }
            }

            pawn.story.traits.allTraits = traitList;
        }

        if (conData.AlterType is TraitAlterType.Removed or TraitAlterType.Altered)
        {
            pawn.story.traits.allTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(conData.OriginalTraitDefName),
                conData.OriginalDegree));
            //Log.Message("PS_ConditoningHelper: UndoTraitChange added trait " + conData.OrigonalTraitDefName + " degree: " + conData.OrigonalDegree);
        }

        pawn.workSettings?.Notify_DisabledWorkTypesChanged();

        //pawn.story.Notify_TraitChanged(); <- Internal method, need to use reflection
        try
        {
            typeof(Pawn_StoryTracker)
                .GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(pawn.story, null);
        }
        catch (Exception)
        {
            // ignored
        }

        pawn.skills?.Notify_SkillDisablesChanged();

        if (!pawn.Dead && pawn.RaceProps.Humanlike)
        {
            pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
        }
    }

    public static List<Trait> GetNaturalTraits(Pawn pawn)
    {
        var outTraits = new List<Trait>();
        foreach (var trait in pawn.story.traits.allTraits)
        {
            outTraits.Add(trait);
        }

        var condata = GetConditioningDataFromHediff(pawn);

        if (condata == null)
        {
            return outTraits;
        }

        foreach (var data in condata)
        {
            if (data.AlterType is TraitAlterType.Added or TraitAlterType.Altered)
            {
                outTraits.Remove(outTraits
                    .FirstOrDefault(x => x.def.defName != data.AddedTraitDefName));
            }

            if (data.AlterType is TraitAlterType.Removed or TraitAlterType.Altered)
            {
                outTraits.Add(new Trait(DefDatabase<TraitDef>.GetNamed(data.OriginalTraitDefName)));
            }
        }

        return outTraits;
    }

    public static void ClearBuggedConditioning(Pawn pawn)
    {
        var hediffs = pawn?.health?.hediffSet?.hediffs.Where(hediff => hediff is PS_Hediff_Reconditioned);
        if (hediffs == null || !hediffs.Any())
        {
            return;
        }

        foreach (var hediff in hediffs)
        {
            pawn.health.RemoveHediff(hediff);
        }

        pawn.needs.AddOrRemoveNeedsAsAppropriate();

        var pod = PS_PodFinder.FindMyPod(pawn);

        pod?.ForceUnassignPawn(pawn);
    }
}