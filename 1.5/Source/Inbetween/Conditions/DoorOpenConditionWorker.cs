using System.Linq;
using Inbetween.Buildings;
using Inbetween.Mapping;
using RimWorld;
using Verse;

namespace Inbetween.Conditions;

public class DoorOpenConditionWorker
{
    public DoorOpenConditionDef def;

    public virtual bool CanDoorOpen(out string reason, Building_IBPortal door)
    {
        Map map = door.Map;
        InbetweenZoneMapComponent mapComponent = map.GetComponent<InbetweenZoneMapComponent>();

        if (def.AllHostilesDead && map.mapPawns.AllPawnsSpawned.Any(p => p.HostileTo(Faction.OfPlayer)))
        {
            reason = "IB_HostilesOnMap".Translate();
            return false;
        }

        if (def.AllThingsDestroyed is { Count: > 0 } && map.listerThings.AllThings.Any(t => def.AllThingsDestroyed.Contains(t.def)))
        {
            reason = "IB_ThingsOnMap".Translate();
            return false;
        }

        int openTick = mapComponent.SpawnedTick + def.TicksElapsed;

        if (def.TicksElapsed >= 0 && openTick > Find.TickManager.TicksAbs)
        {
            reason = "IB_TimerNotElapsed".Translate((openTick - Find.TickManager.TicksAbs) / 60);
            return false;
        }

        reason = "";
        return true;
    }
}
