using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PS_ReconPod
{
    public class PS_Recipes_AdministerConditionall : IngestionOutcomeDoer
    {
        //public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        //{
        //    if (PS_ConditioningHelper.IsReconditioned(pawn))
        //    {
        //        yield return pawn.RaceProps.body.corePart;
        //    }
        //    else
        //        yield break;
        //}
        
        //public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        //{
        //    if (!PS_ConditioningHelper.IsReconditioned(pawn))
        //        return;

        //    if (!PS_ConditioningHelper.SetCurrentNeedLevel(pawn, 1f))
        //        Log.Error("PS_Recipes_AdministerConditionall: Failed to set need level for unknown reason.");
        //}

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (!PS_ConditioningHelper.IsReconditioned(pawn))
                return;

            if (!PS_ConditioningHelper.SetCurrentNeedLevel(pawn, PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 0.25f))
                Log.Error("PS_Recipes_AdministerConditionall: Failed to set need level for unknown reason.");
        }
    }
}
