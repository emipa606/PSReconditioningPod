using System;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x02000233 RID: 563
    public class PS_ThoughtWorker_Conditioning : ThoughtWorker
    {
        // Token: 0x06000A5C RID: 2652 RVA: 0x00050834 File Offset: 0x0004EC34
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var need = (PS_Needs_Reconditioning) p.needs.AllNeeds
                .FirstOrDefault(x => x.def.defName == "PS_Needs_Reconditioning");
            if (need == null)
            {
                return ThoughtState.Inactive;
            }

            switch (need.CurCategory)
            {
                case ConditioningCategory.Fresh:
                    return ThoughtState.Inactive;
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
}