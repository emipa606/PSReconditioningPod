using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x020007C8 RID: 1992
    public class PS_Alert_ConditionedHasNoPod: Alert
    {
        // Token: 0x06002C2E RID: 11310 RVA: 0x0014B3CC File Offset: 0x001497CC
        public PS_Alert_ConditionedHasNoPod()
        {
            this.defaultLabel = "PS_AlertConditionedHasNoPodLab".Translate();
            this.defaultExplanation = "PS_AlertConditionedHasNoPodDes".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        // Token: 0x170006DB RID: 1755
        // (get) Token: 0x06002C2F RID: 11311 RVA: 0x0014B3FC File Offset: 0x001497FC
        private IEnumerable<Pawn> ConditionedWithoutPod
        {
            get
            {
                foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (p.Map.IsPlayerHome && PS_ConditioningHelper.IsReconditioned(p))
                    {
                        if(!PS_PodFinder.HasAccessablePod(p))
                            yield return p;
                    }
                }
                yield break;
            }
        }

        // Token: 0x06002C30 RID: 11312 RVA: 0x0014B418 File Offset: 0x00149818
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(this.ConditionedWithoutPod.ToList());
        }
    }
}
