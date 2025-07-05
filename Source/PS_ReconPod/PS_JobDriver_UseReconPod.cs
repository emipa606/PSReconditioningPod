using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PS_ReconPod;

public class PS_JobDriver_UseReconPod : JobDriver
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
        var openMissionSelect = new Toil();
        openMissionSelect.initAction = delegate
        {
            var actor = openMissionSelect.actor;
            var pod = (PS_Buildings_ReconPod)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
            StartPoding(pod, actor);
        };
        yield return openMissionSelect;
    }

    private void StartPoding(PS_Buildings_ReconPod pod, Pawn podPawn)
    {
        if (!pod.IsUseable(podPawn))
        {
            return;
        }

        var window = new PS_Panel_Reconditioning();
        window.SetPawnAndPod(podPawn, pod);
        Find.WindowStack.Add(window);
    }
}