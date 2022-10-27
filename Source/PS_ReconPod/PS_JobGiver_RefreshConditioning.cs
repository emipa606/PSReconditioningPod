using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PS_ReconPod;

public class PS_JobGiver_RefreshConditioning : ThinkNode_JobGiver
{
    public override ThinkNode DeepCopy(bool resolve = true)
    {
        return (PS_JobGiver_RefreshConditioning)base.DeepCopy(resolve);
    }

    public override float GetPriority(Pawn pawn)
    {
        var currentLevel = PS_ConditioningHelper.GetCurrentNeedLevel(pawn);
        switch (currentLevel)
        {
            case < 0:
                return 0f;
            case < 0.5f:
                return 11.5f;
            default:
                return 0f;
        }
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.Downed)
        {
            return null;
        }

        var currentLevel = PS_ConditioningHelper.GetCurrentNeedLevel(pawn);
        if (currentLevel is < 0f or > 0.5f)
        {
            return null;
        }

        var myPod = PS_PodFinder.FindMyPod(pawn);
        if (myPod != null && PS_PodFinder.CanGetToPod(pawn, myPod) && pawn.CanReserve(new LocalTargetInfo(myPod)) &&
            myPod.IsUseable(pawn))
        {
            return new Job(PS_ReconPodDefsOf.PS_RefreshConditioning, new LocalTargetInfo(myPod));
        }

        if (pawn.Map == null)
        {
            return null;
        }

        var condionallList =
            pawn.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("PS_Drugs_Conditionall"));
        if (!(condionallList?.Any() ?? false))
        {
            return null;
        }

        var avalible = condionallList.Where(x => IsConditionallAvalible(x, pawn)).ToList();
        if (avalible.Any())
        {
            return null;
        }

        var closest = GetClostest(pawn, avalible);
        if (closest == null)
        {
            return null;
        }

        try
        {
            var job = DrugAIUtility.IngestAndTakeToInventoryJob(closest, pawn, 1);
            return job;
        }
        catch (ArgumentException)
        {
            Log.Error("PS_BadDrugPolicyError".Translate());
            throw;
        }
    }

    private bool IsConditionallAvalible(Thing conditionall, Pawn pawn)
    {
        if (conditionall.IsForbidden(pawn))
        {
            return false;
        }

        if (!conditionall.Position.InAllowedArea(pawn))
        {
            return false;
        }

        var localTarget = new LocalTargetInfo(conditionall);
        return pawn.CanReach(localTarget, PathEndMode.ClosestTouch, Danger.Deadly);
    }

    private Thing GetClostest(Pawn pawn, List<Thing> things)
    {
        var index = 0;
        var minDist = float.MaxValue;
        for (var n = 0; n < things.Count; n++)
        {
            var tempDist = pawn.Position.DistanceTo(things[n].Position);
            if (!(tempDist < minDist))
            {
                continue;
            }

            minDist = tempDist;
            index = n;
        }

        return things[index];
    }
}