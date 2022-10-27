using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PS_ReconPod;

internal static class PS_PodFinder
{
    public static PS_Buildings_ReconPod FindMyPod(Pawn pawn, bool exspectToFind = true)
    {
        var pods = Find.Maps.SelectMany(m =>
            m.listerBuildings.allBuildingsColonist.Where(x => x.def.defName == "PS_Buildings_ReconPod").ToList());
        if (exspectToFind && !pods.Any())
        {
            return null;
        }

        var myPod = pods.Select(x => (PS_Buildings_ReconPod)x)
            .FirstOrDefault(x => x.HasOwner && x.PodOwner == pawn);
        return myPod;
    }


    public static bool CanGetToPod(Pawn pawn, PS_Buildings_ReconPod pod)
    {
        var localTarget = new LocalTargetInfo(pod);
        if (!pawn.CanReach(localTarget, PathEndMode.Touch, Danger.Deadly))
        {
            return false;
        }

        var possition = pod.Position;
        return possition.InAllowedArea(pawn) && possition.Walkable(pawn.Map);
    }

    public static bool HasAccessablePod(Pawn pawn)
    {
        var pod = FindMyPod(pawn, false);
        if (pod == null)
        {
            return false;
        }

        return pod.IsUseable(pawn) && CanGetToPod(pawn, pod);
    }
}