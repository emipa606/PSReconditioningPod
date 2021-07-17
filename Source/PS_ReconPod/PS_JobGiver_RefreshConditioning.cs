using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PS_ReconPod
{
    // Token: 0x02000007 RID: 7
    public class PS_JobGiver_RefreshConditioning : ThinkNode_JobGiver
    {
        // Token: 0x06000024 RID: 36 RVA: 0x00002840 File Offset: 0x00000A40
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            return (PS_JobGiver_RefreshConditioning) base.DeepCopy(resolve);
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00002860 File Offset: 0x00000A60
        public override float GetPriority(Pawn pawn)
        {
            var currentLevel = PS_ConditioningHelper.GetCurrentNeedLevel(pawn);
            if (currentLevel < 0)
            {
                return 0f;
            }

            if (currentLevel < 0.5f)
            {
                return 11.5f;
            }

            return 0f;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000028AC File Offset: 0x00000AAC
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed)
            {
                return null;
            }

            var currentLevel = PS_ConditioningHelper.GetCurrentNeedLevel(pawn);
            if (currentLevel < 0f || currentLevel > 0.5f)
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
                var job = DrugAIUtility.IngestAndTakeToInventoryJob(closest, pawn,
                    1); // new Job(JobDefOf.Ingest, new LocalTargetInfo(closest));
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
            if (!pawn.CanReach(localTarget, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return false;
            }

            return true;
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
}
//            Thing thing3 = EnergyNeedUtility.ClosestPowerSource(pawn);
//            bool flag3 = thing3 != null;
//                    if (flag3)
//                    {
//                        Building building = thing3 as Building;
//                        bool flag4 = thing3 != null && building != null && building.PowerComp != null && building.PowerComp.PowerNet.CurrentStoredEnergy() > 50f;
//                        if (flag4)
//                        {
//                            IntVec3 position = thing3.Position;
//                            bool flag5 = position.Walkable(pawn.Map) && position.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(position), 1, -1, null, false) && pawn.CanReach(position, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
//                            if (flag5)
//                            {
//                                return new Job(JobDefOf.ChJAndroidRecharge, thing3);
//                            }
//                            IEnumerable<IntVec3> source = GenAdj.CellsAdjacentCardinal(building);
//                            Func < IntVec3, float> <> 9__0;
//                            Func<IntVec3, float> keySelector;
//                            if ((keySelector = <> 9__0) == null)
//                            {
//                                keySelector = (<> 9__0 = ((IntVec3 selector) => selector.DistanceTo(pawn.Position)));
//                            }
//                            foreach (IntVec3 intVec in source.OrderByDescending(keySelector))
//                            {
//                                bool flag6 = intVec.Walkable(pawn.Map) && intVec.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(intVec), 1, -1, null, false) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
//                                if (flag6)
//                                {
//                                    return new Job(JobDefOf.ChJAndroidRecharge, thing3, intVec);
//                                }
//                            }
//                        }
//                    }
//                    Thing thing2 = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Thing thing) => thing.TryGetComp<EnergySourceComp>() != null && !thing.IsForbidden(pawn) && pawn.CanReserve(new LocalTargetInfo(thing), 1, -1, null, false) && thing.Position.InAllowedArea(pawn) && pawn.CanReach(new LocalTargetInfo(thing), PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn), null, 0, -1, false, RegionType.Set_Passable, false);
//                    bool flag7 = thing2 != null;
//                    if (flag7)
//                    {
//                        EnergySourceComp energySourceComp = thing2.TryGetComp<EnergySourceComp>();
//                        bool flag8 = energySourceComp != null;
//                        if (flag8)
//                        {
//                            int num = (int)Math.Ceiling((double)((need_Energy.MaxLevel - need_Energy.CurLevel) / energySourceComp.EnergyProps.energyWhenConsumed));
//                            bool flag9 = num > 0;
//                            if (flag9)
//                            {
//                                return new Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(thing2))
//                                {
//                                    count = num
//                                };
//                            }
//                        }
//                    }
//                    result = null;
//                }
//            }
//            }
//            return result;
//        }


//    }
//}