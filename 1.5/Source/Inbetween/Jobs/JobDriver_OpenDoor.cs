using System;
using System.Collections.Generic;
using Inbetween.Buildings;
using Verse;
using Verse.AI;

namespace Inbetween.Jobs;

public class JobDriver_OpenDoor : JobDriver
{
    public Building_InbetweenDoor Door => TargetThingA as Building_InbetweenDoor;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false);

        Toil toilFaceAndWait = Toils_General.Wait(90, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, true, -0.5f);

        toilFaceAndWait.tickAction = (Action) Delegate.Combine(toilFaceAndWait.tickAction, new Action(delegate
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
        }));
        toilFaceAndWait.handlingFacing = true;
        yield return toilFaceAndWait;

        Toil toilTeleport = ToilMaker.MakeToil();

        toilTeleport.initAction = () =>
        {
            ModLog.Log($"Ensuring Map from Job {this}");
            Door.EnsureMap(() =>
            {
                ModLog.Log($"Map Ensured from Job {this}");
            });
        };

        toilTeleport.AddEndCondition(() => Door.GetOtherMap() == null && !Door.IsOpen() ? JobCondition.Ongoing : JobCondition.Succeeded);

        yield return toilTeleport;
    }
}
