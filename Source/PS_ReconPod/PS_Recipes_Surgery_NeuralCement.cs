using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod
{
    public class PS_Recipes_Surgery_NeuralCement : Recipe_Surgery
    {
        protected virtual List<int> AllowedDegrees()
        {
            return new List<int> {0};
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
                yield return pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().First().Part;
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
            var brainPart = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def.defName == "Brain");
            if (brainPart == null)
            {
                brainPart = part;
            }

            pawn.health.AddHediff(
                HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PS_Hediff_NeuralCement"), pawn,
                    brainPart));
            var pod = PS_PodFinder.FindMyPod(pawn);
            if (pod != null)
            {
                pod.TryUnassignPawn(pawn);
            }
        }
    }
}