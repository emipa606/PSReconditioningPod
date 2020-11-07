using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PS_ReconPod
{
    public class PS_JobDriver_RefreshConditioning : JobDriver
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
            var enterPodToil = new Toil();
            enterPodToil.initAction = delegate ()
            {
                Pawn actor = enterPodToil.actor;
                var pod = (PS_Buildings_ReconPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                pod.StartRefreshing(actor, actor.jobs.curJob.GetTarget(TargetIndex.A));
            };
            yield return enterPodToil;
            yield break;
        }
        
    }
}
