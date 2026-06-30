using ChimeLib.NET;
using HarmonyLib;
using Manager;
using System.Collections;
using System.Reflection;
using SinmaiAssist.GUI;
using UnityEngine;

namespace SinmaiAssist.Common;

public class DummyChimeLogin
{
    // CameraManager Patch
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CameraManager), "get_IsAvailableCamera")]
    public static bool IsAvailableCamera(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CameraManager), "get_IsAvailableChimeCamera")]
    public static bool IsAvailableChimeCamera(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CameraManager), "get_IsAvailableCameras")]
    public static bool IsAvailableCameras(ref bool[] __result)
    {
        __result = new bool[2] { true, true };
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChimeDevice), "HasError")]
    public static bool HasError(ref bool __result)
    {
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChimeDevice), "GetDecodeStrings")]
    public static bool GetDecodeStrings(ref string[] __result)
    {
        if (DummyLoginPanel.CodeLoginFlag)
        {
            DummyLoginPanel.CodeLoginFlag = false;
            if (DummyLoginPanel.DummyLoginCode == null)
            {
                __result = null;
                return false;
            }
            else
            {
                __result = new string[1] { DummyLoginPanel.DummyLoginCode };
                return false;
            }
        }
        return true;
    }

    // ChimeReaderManager Patch
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChimeReaderManager), "Execute")]
    public static bool Execute(ChimeReaderManager __instance)
    {
        var result = AccessTools.Field(typeof(ChimeReaderManager), "_result");
        var aimeId = AccessTools.Field(typeof(ChimeReaderManager), "_aimeId");
        var currentState = AccessTools.Field(typeof(ChimeReaderManager), "currentState");
        if (DummyLoginPanel.UserIdLoginFlag)
        {
            ChimeId _aimeId;
            System.Type chimeIdType = System.Type.GetType("ChimeLib.NET.ChimeId, ChimeLib.NET");
            MethodInfo makeMethod = chimeIdType.GetMethod("Make", BindingFlags.NonPublic | BindingFlags.Static);
            _aimeId = (ChimeId)makeMethod.Invoke(null, new object[] { uint.Parse(DummyLoginPanel.DummyUserId) });
            result.SetValue(__instance, ChimeReaderManager.Result.Done);
            aimeId.SetValue(__instance, _aimeId);
            currentState.SetValue(__instance, 9);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChimeReaderManager), "AdvCheck")]
    public static bool AdvCheck(ref bool __result)
    {
        if (DummyLoginPanel.UserIdLoginFlag)
        {
            __result = true;
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Process.Entry.TryAime), "Execute")]
    public static void ClearFlag()
    {
        DummyLoginPanel.UserIdLoginFlag = false;
    }
}