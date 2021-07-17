using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x020007C8 RID: 1992
    public class PS_Alert_ConditionedHasNoPod : Alert
    {
        // Token: 0x06002C2E RID: 11310 RVA: 0x0014B3CC File Offset: 0x001497CC
        public PS_Alert_ConditionedHasNoPod()
        {
            defaultLabel = "PS_AlertConditionedHasNoPodLab".Translate();
            defaultExplanation = "PS_AlertConditionedHasNoPodDes".Translate();
            defaultPriority = AlertPriority.High;
        }

        // Token: 0x170006DB RID: 1755
        // (get) Token: 0x06002C2F RID: 11311 RVA: 0x0014B3FC File Offset: 0x001497FC
        private IEnumerable<Pawn> ConditionedWithoutPod
        {
            get
            {
                foreach (var p in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (!p.Map.IsPlayerHome || !PS_ConditioningHelper.IsReconditioned(p))
                    {
                        continue;
                    }

                    if (!PS_PodFinder.HasAccessablePod(p))
                    {
                        yield return p;
                    }
                }
            }
        }

        // Token: 0x06002C30 RID: 11312 RVA: 0x0014B418 File Offset: 0x00149818
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(ConditionedWithoutPod.ToList());
        }
    }
}