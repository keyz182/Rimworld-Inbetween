using HarmonyLib;
using UnityEngine;
using Verse;

namespace Inbetween;

public class InbetweenMod : Mod
{
    public static Settings settings;

    public InbetweenMod(ModContentPack content) : base(content)
    {
        ModLog.Log("Hello world from Inbetween");

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("keyz182.rimworld.Inbetween.main");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Inbetween_SettingsCategory".Translate();
    }
}
