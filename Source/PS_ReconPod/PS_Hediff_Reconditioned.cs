using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PS_ReconPod;

public class PS_Hediff_Reconditioned : Hediff
{
    private bool CheckedConData;
    //public TraitAlterType ChangeType;
    //public TraitDef OrigonalTraitDef;
    //public TraitDef AddedTraitDef;

    // Old: to support pre-multi-conditioning
    private PS_Conditioning_Data ConditioningData;
    public List<PS_Conditioning_Data> ConditioningDataList;

    public PS_Hediff_Reconditioned()
    {
        def = DefDatabase<HediffDef>.GetNamed("PS_Hediff_Reconditioned");
        CheckedConData = false;
        ConditioningDataList = [];
    }

    public override string LabelBase => "Reconditioned";

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ConditioningData, "ConditionData");
        Scribe_Collections.Look(ref ConditioningDataList, "ConditionDataList");
    }

    private void CheckForOldConData()
    {
        if (ConditioningData == null)
        {
            return;
        }

        if (ConditioningDataList == null || !ConditioningDataList.Any())
        {
            Log.Message(
                $"PS_Hediff_Reconditoning: Condition data for {pawn.LabelShort} found as single and not list. This is due to multi-condition update. Building list now.");
            ConditioningDataList =
            [
                new PS_Conditioning_Data
                {
                    PawnId = ConditioningData.PawnId,
                    AddedTraitDefName = ConditioningData.AddedTraitDefName,
                    OriginalTraitDefName = ConditioningData.OriginalTraitDefName,
                    AddedDegree = ConditioningData.AddedDegree,
                    OriginalDegree = ConditioningData.OriginalDegree,
                    AlterType = ConditioningData.AlterType
                }
            ];
            ConditioningData = null;
            return;
        }

        if (!ConditioningDataList.Any(x => x.IsSame(ConditioningData)))
        {
            Log.Message(
                $"PS_Hediff_Reconditoning: Condition data for {pawn.LabelShort} found as single and not list due to update. But list was already occupied.");
            ConditioningDataList.Add(new PS_Conditioning_Data
            {
                PawnId = ConditioningData.PawnId,
                AddedTraitDefName = ConditioningData.AddedTraitDefName,
                OriginalTraitDefName = ConditioningData.OriginalTraitDefName,
                AddedDegree = ConditioningData.AddedDegree,
                OriginalDegree = ConditioningData.OriginalDegree,
                AlterType = ConditioningData.AlterType
            });
            ConditioningData = null;
        }

        var tempList = ConditioningDataList.Where(x => x.IsValid()).ToList();
        ConditioningDataList.Clear();
        ConditioningDataList = tempList;
    }

    public override void Tick()
    {
        base.Tick();
        if (CheckedConData)
        {
            return;
        }

        CheckForOldConData();
        CheckedConData = true;
    }
}