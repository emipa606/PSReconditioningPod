using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_Recipes_Surgery_NeuralCement : Recipe_Surgery
{
    protected virtual List<int> AllowedDegrees()
    {
        return [0];
    }

    protected virtual int GetChange()
    {
        return 0;
    }

    protected virtual int GetFailBeauty()
    {
        return 0;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        if (PS_ConditioningHelper.IsReconditioned(pawn))
        {
            yield return pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>().Part;
        }
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        if (billDoer == null)
        {
            return;
        }

        if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
        {
            return;
        }

        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        PS_ConditioningHelper.TryRemoveConditioning(pawn);
        var brainPart = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def.defName == "Brain") ?? part;

        pawn.health.AddHediff(
            HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PS_Hediff_NeuralCement"), pawn,
                brainPart));
        var pod = PS_PodFinder.FindMyPod(pawn);
        pod?.TryUnassignPawn(pawn);
    }
}