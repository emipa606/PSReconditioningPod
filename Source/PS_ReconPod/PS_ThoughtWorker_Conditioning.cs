using System;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_ThoughtWorker_Conditioning : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var need = (PS_Needs_Reconditioning)p.needs.AllNeeds
            .FirstOrDefault(x => x.def.defName == "PS_Needs_Reconditioning");
        if (need == null)
        {
            return ThoughtState.Inactive;
        }

        switch (need.CurCategory)
        {
            case ConditioningCategory.Fresh:
            case ConditioningCategory.Strong:
                return ThoughtState.Inactive;
            case ConditioningCategory.Weakened:
                return ThoughtState.ActiveAtStage(0);
            case ConditioningCategory.Slipping:
                return ThoughtState.ActiveAtStage(1);
            default:
                throw new NotImplementedException();
        }
    }
}