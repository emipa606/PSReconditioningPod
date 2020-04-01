using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PS_ReconPod
{
    public class PS_JobDriver_UseReconPod : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
            Toil openMissionSelect = new Toil();
            openMissionSelect.initAction = delegate ()
            {
                Pawn actor = openMissionSelect.actor;
                PS_Buildings_ReconPod pod = (PS_Buildings_ReconPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                this.StartPoding(pod, actor);
            };
            yield return openMissionSelect;
            yield break;
        }

        public void StartPoding(PS_Buildings_ReconPod pod, Pawn pawn)
        {
            if (pod.IsUseable(pawn))
            {
                var window = new PS_Panel_Reconditioning();
                window.SetPawnAndPod(pawn, pod);
                Find.WindowStack.Add(window);
            }
        }
    }
}
