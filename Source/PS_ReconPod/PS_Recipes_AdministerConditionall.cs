using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_Recipes_AdministerConditionall : IngestionOutcomeDoer
{
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
    {
        if (!PS_ConditioningHelper.IsReconditioned(pawn))
        {
            return;
        }

        if (!PS_ConditioningHelper.SetCurrentNeedLevel(pawn,
                PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 0.25f))
        {
            Log.Error("PS_Recipes_AdministerConditionall: Failed to set need level for unknown reason.");
        }
    }
}