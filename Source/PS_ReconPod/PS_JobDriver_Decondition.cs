using System.Collections.Generic;
using Verse.AI;

namespace PS_ReconPod;

public class PS_JobDriver_Decondition : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var pawn1 = pawn;
        var targetA = job.targetA;
        var job1 = job;
        return pawn1.Reserve(targetA, job1, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
        var enterPodToil = new Toil();
        enterPodToil.initAction = delegate
        {
            var actor = enterPodToil.actor;
            var unused = (PS_Buildings_ReconPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            //pod.StartDeconditioning(actor);
        };
        yield return enterPodToil;
    }
}