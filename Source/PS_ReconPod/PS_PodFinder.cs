using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PS_ReconPod
{
    internal static class PS_PodFinder
    {
        public static PS_Buildings_ReconPod FindMyPod(Pawn pawn, bool exspectToFind = true)
        {
            //var map = pawn.Map;
            //if (map == null)
            //{
            //    Log.Error("PS_PodFinder: Tried to FindMyPod for pawn: " + pawn.LabelShort + " but map is null");
            //    return null;
            //}

            var pods = Find.Maps.SelectMany(m =>
                m.listerBuildings.allBuildingsColonist.Where(x => x.def.defName == "PS_Buildings_ReconPod").ToList());
            if (exspectToFind && !pods.Any())
            {
                //Log.Error("PS_PodFinder: Tried to FindMyPod for pawn: " + pawn.LabelShort + " but no pods found");
                return null;
            }

            var myPod = pods.Select(x => (PS_Buildings_ReconPod) x)
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
            if (!possition.InAllowedArea(pawn))
            {
                return false;
            }

            if (!possition.Walkable(pawn.Map))
            {
                return false;
            }

            return true;
        }

        public static bool HasAccessablePod(Pawn pawn)
        {
            var pod = FindMyPod(pawn, false);
            if (pod == null)
            {
                return false;
            }

            if (!pod.IsUseable(pawn))
            {
                return false;
            }

            if (!CanGetToPod(pawn, pod))
            {
                return false;
            }

            return true;
        }
    }
}