using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace SinmaiAssist.Common;

// Removes the intentional one-frame delay applied to in-game input.
//
// Manager.InputManager.UpdateAmInput writes the current raw input into index [1] of
// GameButtonPush/GameButtonDown/GamePanelPush/GamePanelDown and shifts the previous [1]
// into [0]. The gameplay note/net code only reads index [0] (InGameButtonDown,
// InGameTouchPanelAreaDown, ...), so every hit is one frame late. Copying the freshly
// written [1] back into [0] exposes it in the same frame; the Down slot of [1] is then
// cleared so the next frame's shift does not re-trigger the same press (IsUsedThisFrame
// still guarantees a single consumption per frame).
public class ReduceInputLatency
{
    private static readonly FieldInfo GameButtonPushField = AccessTools.Field(typeof(Manager.InputManager), "GameButtonPush");
    private static readonly FieldInfo GameButtonDownField = AccessTools.Field(typeof(Manager.InputManager), "GameButtonDown");
    private static readonly FieldInfo GamePanelPushField = AccessTools.Field(typeof(Manager.InputManager), "GamePanelPush");
    private static readonly FieldInfo GamePanelDownField = AccessTools.Field(typeof(Manager.InputManager), "GamePanelDown");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Manager.InputManager), "UpdateAmInput")]
    public static void SyncInGameInput()
    {
        if (!SinmaiAssist.MainConfig.Common.ReduceInputLatency.RemoveInputBuffer) return;
        if (GameButtonDownField == null || GamePanelDownField == null) return;

        bool[,,] gameButtonPush = (bool[,,])GameButtonPushField.GetValue(null);
        bool[,,] gameButtonDown = (bool[,,])GameButtonDownField.GetValue(null);
        bool[,,] gamePanelPush = (bool[,,])GamePanelPushField.GetValue(null);
        bool[,,] gamePanelDown = (bool[,,])GamePanelDownField.GetValue(null);

        for (int player = 0; player < 2; player++)
        {
            for (int button = 0; button < 9; button++)
            {
                gameButtonPush[0, player, button] = gameButtonPush[1, player, button];
                gameButtonDown[0, player, button] = gameButtonDown[1, player, button];
                gameButtonDown[1, player, button] = false;
            }
            for (int area = 0; area < 35; area++)
            {
                gamePanelPush[0, player, area] = gamePanelPush[1, player, area];
                gamePanelDown[0, player, area] = gamePanelDown[1, player, area];
                gamePanelDown[1, player, area] = false;
            }
        }
    }
}

// Bypasses the ON-OFF-ON debounce filter in IO.Jvs+JvsSwitch.Execute, which drops a
// press that follows a single frame of release (IO/Jvs.cs:64-67). The original
// Execute already advanced the history and wrote its (possibly suppressed) result, so
// the postfix simply recomputes the raw state without the suppression and overwrites
// the resulting fields.
[HarmonyPatch]
public class JvsBounceBypass
{
    private static FieldInfo _switchInputField;
    private static FieldInfo _subKeyField;
    private static FieldInfo _invertField;
    private static FieldInfo _isStateOnField;
    private static FieldInfo _isStateOnOldField;
    private static FieldInfo _isTriggerOnField;
    private static FieldInfo _isTriggerOffField;

    static MethodBase TargetMethod()
    {
        Type jvsSwitchType = AccessTools.TypeByName("IO.Jvs+JvsSwitch");
        if (jvsSwitchType == null) return null;

        _switchInputField = AccessTools.Field(jvsSwitchType, "_switchInput");
        _subKeyField = AccessTools.Field(jvsSwitchType, "_subKey");
        _invertField = AccessTools.Field(jvsSwitchType, "_invert");
        _isStateOnField = AccessTools.Field(jvsSwitchType, "_isStateOn");
        _isStateOnOldField = AccessTools.Field(jvsSwitchType, "_isStateOnOld");
        _isTriggerOnField = AccessTools.Field(jvsSwitchType, "_isTriggerOn");
        _isTriggerOffField = AccessTools.Field(jvsSwitchType, "_isTriggerOff");

        return AccessTools.Method(jvsSwitchType, "Execute");
    }

    [HarmonyPostfix]
    public static void Postfix(object __instance)
    {
        if (!SinmaiAssist.MainConfig.Common.ReduceInputLatency.DisableBounceFilter) return;
        if (_switchInputField == null) return;

        AMDaemon.SwitchInput switchInput = (AMDaemon.SwitchInput)_switchInputField.GetValue(__instance);
        KeyCode subKey = (KeyCode)_subKeyField.GetValue(__instance);
        bool invert = (bool)_invertField.GetValue(__instance);

        bool flag = DebugInput.GetKey(subKey);
        flag |= invert
            ? (!switchInput.IsOn || switchInput.HasOffNow)
            : (switchInput.IsOn || switchInput.HasOnNow);

        bool stateOnOld = (bool)_isStateOnOldField.GetValue(__instance);
        _isStateOnField.SetValue(__instance, flag);
        _isTriggerOnField.SetValue(__instance, flag && (flag ^ stateOnOld));
        _isTriggerOffField.SetValue(__instance, !flag && (flag ^ stateOnOld));
    }
}

// Bypasses the ON-OFF-ON debounce filter in IO.TouchPanel.GameInputLog.KillOnOffOn,
// which clears a panel bit when the pattern on-off-on appears within three frames
// (IO/TouchPanel/GameInputLog.cs:73-80). Skipping it lets fast repeated taps through.
[HarmonyPatch]
public class GameInputLogBounceBypass
{
    static MethodBase TargetMethod()
    {
        Type gameInputLogType = AccessTools.TypeByName("IO.TouchPanel.GameInputLog");
        return gameInputLogType == null ? null : AccessTools.Method(gameInputLogType, "KillOnOffOn");
    }

    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !SinmaiAssist.MainConfig.Common.ReduceInputLatency.DisableBounceFilter;
    }
}
