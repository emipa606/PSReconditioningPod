using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_Alert_ConditionedHasNoPod : Alert
{
    public PS_Alert_ConditionedHasNoPod()
    {
        defaultLabel = "PS_AlertConditionedHasNoPodLab".Translate();
        defaultExplanation = "PS_AlertConditionedHasNoPodDes".Translate();
        defaultPriority = AlertPriority.High;
    }

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

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritsAre(ConditionedWithoutPod.ToList());
    }
}