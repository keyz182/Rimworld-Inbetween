using System;
using System.Collections.Generic;
using Verse;

namespace Inbetween.Conditions;

public class DoorOpenConditionDef : Def
{
    public Type workerClass;

    public bool AllHostilesDead = false;
    public List<ThingDef> AllThingsDestroyed;
    public int TicksElapsed = -1;

    [Unsaved(false)] public DoorOpenConditionWorker workerInt;

    public DoorOpenConditionWorker Worker
    {
        get
        {
            if (workerInt == null)
            {
                workerInt = (DoorOpenConditionWorker) Activator.CreateInstance(workerClass);
                workerInt.def = this;
            }

            return workerInt;
        }
    }
}
