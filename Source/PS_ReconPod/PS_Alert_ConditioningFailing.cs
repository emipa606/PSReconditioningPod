using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_Alert_ConditioningFailing : Alert_Critical
{
    public PS_Alert_ConditioningFailing()
    {
        defaultLabel = "PS_AlertConditioningFailingLab".Translate();
        defaultExplanation = "PS_AlertConditioningFailingDes".Translate();
        defaultPriority = AlertPriority.Critical;
    }

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

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritsAre(ConditionedAndSlipping.ToList());
    }
}