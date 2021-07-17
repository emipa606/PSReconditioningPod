using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x020007C8 RID: 1992
    public class PS_Alert_ConditioningFailing : Alert_Critical
    {
        // Token: 0x06002C2E RID: 11310 RVA: 0x0014B3CC File Offset: 0x001497CC
        public PS_Alert_ConditioningFailing()
        {
            defaultLabel = "PS_AlertConditioningFailingLab".Translate();
            defaultExplanation = "PS_AlertConditioningFailingDes".Translate();
            defaultPriority = AlertPriority.Critical;
        }

        // Token: 0x170006DB RID: 1755
        // (get) Token: 0x06002C2F RID: 11311 RVA: 0x0014B3FC File Offset: 0x001497FC
        private IEnumerable<Pawn> ConditionedAndSlipping
        {
            get
            {
                foreach (var p in PawnsFinder
                    .AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
                {
                    if (!PS_ConditioningHelper.IsReconditioned(p))
                    {
                        continue;
                    }

                    var conditionLevel = PS_ConditioningHelper.GetCurrentNeedLevel(p);
                    if (conditionLevel <= 0.25f)
                    {
                        yield return p;
                    }
                }
            }
        }

        // Token: 0x06002C30 RID: 11312 RVA: 0x0014B418 File Offset: 0x00149818
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(ConditionedAndSlipping.ToList());
        }
    }
}