using HarmonyLib;
using Monitor.ModeSelect;

namespace SinmaiAssist.Cheat;

public class UnlockKaleidxScope
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ModeSelectMonitor), "InitialzeKaleidxScopeUnlockState")]
    public static void UnlockAll(ref ModeSelectMonitor.KaleidxScopeUnlockState __result, int index)
    {
        __result = ModeSelectMonitor.KaleidxScopeUnlockState.Unlocked;
    }
}
