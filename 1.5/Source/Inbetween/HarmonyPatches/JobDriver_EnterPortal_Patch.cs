using System;
using System.Collections.Generic;
using HarmonyLib;
using Inbetween.Mapping;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Inbetween.HarmonyPatches;

[HarmonyPatch(typeof(JobDriver_EnterPortal))]
public static class JobDriver_EnterPortal_Patch
{
    public static IEnumerable<Toil> MakeNewToils_Actual(JobDriver_EnterPortal _this, Building_InbetweenDoor door)
    {
        _this.FailOnDespawnedOrNull(TargetIndex.A);
        _this.FailOn(delegate
        {
            string text;
            return !door.IsEnterable(out text);
        });
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false);

        Toil toilFaceAndWait = Toils_General.Wait(90, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, true, -0.5f);

        toilFaceAndWait.tickAction = (Action) Delegate.Combine(toilFaceAndWait.tickAction, new Action(delegate
        {
            _this.pawn.rotationTracker.FaceTarget(_this.job.targetA);
        }));
        toilFaceAndWait.handlingFacing = true;
        yield return toilFaceAndWait;

        Toil toilTeleport = ToilMaker.MakeToil("MakeNewToils");

        toilTeleport.initAction =
            delegate
            {
                door.EnsureMap(delegate
                {
                    Map otherMap = door.GetOtherMap();

                    IntVec3 intVec = door.GetDestinationLocation();
                    if (!intVec.Standable(otherMap))
                    {
                        intVec = CellFinder.StandableCellNear(intVec, otherMap, 10f, null);
                    }

                    if (intVec == IntVec3.Invalid)
                    {
                        Messages.Message("UnableToEnterPortal".Translate(door.Label), door, MessageTypeDefOf.NegativeEvent, true);
                        return;
                    }

                    bool drafted = _this.pawn.Drafted;
                    _this.pawn.DeSpawnOrDeselect(DestroyMode.Vanish);
                    GenSpawn.Spawn(_this.pawn, intVec, otherMap, Rot4.Random, WipeMode.Vanish, false, false);
                    door.OnEntered(_this.pawn);
                    if (!otherMap.IsPocketMap)
                    {
                        _this.pawn.inventory.UnloadEverything = true;
                    }

                    if (drafted || door.AutoDraftOnEnter)
                    {
                        _this.pawn.drafter.Drafted = true;
                    }

                    if (_this.pawn.carryTracker.CarriedThing != null && !_this.pawn.Drafted)
                    {
                        Thing thing;
                        _this.pawn.carryTracker.TryDropCarriedThing(_this.pawn.Position, ThingPlaceMode.Direct, out thing, null);
                    }

                    Lord lord = _this.pawn.GetLord();
                    if (lord != null)
                    {
                        lord.Notify_PawnLost(_this.pawn, PawnLostCondition.ExitedMap, null);
                    }
                });
            };
        yield return toilTeleport;
    }

    [HarmonyPatch("MakeNewToils")]
    [HarmonyPrefix]
    public static bool MakeNewToils_Patch(JobDriver_EnterPortal __instance, ref IEnumerable<Toil> __result)
    {
        object target = AccessTools.Property(typeof(JobDriver_EnterPortal), "TargetThingA").GetValue(__instance);
        Building_InbetweenDoor door = target as Building_InbetweenDoor;

        if (door == null)
        {
            return true;
        }

        __result = MakeNewToils_Actual(__instance, door);

        return false;
    }
}
