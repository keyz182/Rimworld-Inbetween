using HarmonyLib;
using Inbetween.Inbetween;
using RimWorld;
using Verse;

namespace Inbetween.HarmonyPatches;

[HarmonyPatch(typeof(GameComponent_Anomaly))]
public static class GameComponent_Anomaly_Patch
{

    [HarmonyPatch(nameof(GameComponent_Anomaly.StartedNewGame))]
    [HarmonyPrefix]
    public static bool StartedNewGame_Patch()
    {
        return !Current.Game.GetComponent<InbetweenGameComponent>().InbetweenQuickplayMode;
    }
}
