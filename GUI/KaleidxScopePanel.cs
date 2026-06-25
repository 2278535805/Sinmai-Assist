using System;
using System.Collections.Generic;
using MAI2.Util;
using Manager;
using Manager.MaiStudio;
using Manager.UserDatas;
using MelonLoader;
using SinmaiAssist.Utils;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class KaleidxScopePanel
{
    private static Vector2 _scrollPos = Vector2.zero;

    public static void OnGUI()
    {
        try
        {
            var userData1 = Singleton<UserDataManager>.Instance.GetUserData(0L);
            var userData2 = Singleton<UserDataManager>.Instance.GetUserData(1L);
            var gates = Singleton<DataManager>.Instance.GetKaleidxScopeGates();

            GUILayout.Label("KaleidxScope Manager", MainGUI.Style.Title);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"1P: {(userData1 != null ? userData1.Detail.UserName : "---")}", MainGUI.Style.Text);
            GUILayout.Label($"2P: {(userData2 != null ? userData2.Detail.UserName : "---")}", MainGUI.Style.Text);
            GUILayout.EndHorizontal();

            GUILayout.Label($"Total Gates: {gates.Count}", MainGUI.Style.Text);

            GUILayout.Label("Batch Operations", MainGUI.Style.Title);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All Found", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 160 }))
            {
                UnlockAllGates(0, true, false, false, false);
                UnlockAllGates(1, true, false, false, false);
                GameMessageManager.SendMessage(0, "All Gates Found");
            }
            if (GUILayout.Button("All Key", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 160 }))
            {
                UnlockAllKeys(0, false, false);
                UnlockAllKeys(1, false, false);
                GameMessageManager.SendMessage(0, "All Keys Found");
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All Clear", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 160 }))
            {
                UnlockAllGates(0, true, true, true, true);
                UnlockAllGates(1, true, true, true, true);
                GameMessageManager.SendMessage(0, "All Cleared");
            }
            if (GUILayout.Button("Reset All", new GUIStyle(MainGUI.Style.Button){ fixedWidth = 160 }))
            {
                ResetAll(0);
                ResetAll(1);
                GameMessageManager.SendMessage(0, "All Reset");
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Gates (F=Found K=Key C=Clear W=Info)", MainGUI.Style.Title);
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID/Nm", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 90 });
            GUILayout.Label("P1", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 44 });
            GUILayout.Label("P2", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 44 });
            GUILayout.Label("Op", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 96 });
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(180f));

            foreach (var kvp in gates)
            {
                var gate = kvp.Value;
                if (gate == null) continue;
                if (!Singleton<EventManager>.Instance.IsOpenEvent(gate.eventName.id)) continue;

                int gateId = gate.name.id;
                var p1Scope = FindScope(userData1, gateId);
                var p2Scope = FindScope(userData2, gateId);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{gateId}:{gate.name.str}", new GUIStyle(MainGUI.Style.Text) { fixedWidth = 90 });
                GUILayout.Label(ScopeStatusString(p1Scope), new GUIStyle(MainGUI.Style.Text) { fixedWidth = 44 });
                GUILayout.Label(ScopeStatusString(p2Scope), new GUIStyle(MainGUI.Style.Text) { fixedWidth = 44 });

                if (GUILayout.Button("F", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 22 }))
                {
                    userData1?.AddKaleidxScopeGate(gateId);
                    userData2?.AddKaleidxScopeGate(gateId);
                }
                if (GUILayout.Button("K", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 22 }))
                {
                    UnlockKeysForGate(userData1, gateId);
                    UnlockKeysForGate(userData2, gateId);
                }
                if (GUILayout.Button("C", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 22 }))
                {
                    SetScopeClear(userData1, gateId);
                    SetScopeClear(userData2, gateId);
                }
                if (GUILayout.Button("X", new GUIStyle(MainGUI.Style.Button) { fixedWidth = 22 }))
                {
                    ResetScope(userData1, gateId);
                    ResetScope(userData2, gateId);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
        catch (Exception e)
        {
            GUILayout.Label($"Error: {e.Message}", MainGUI.Style.ErrorMessage);
        }
    }

    private static UserKaleidxScope FindScope(UserData userData, int gateId)
    {
        if (userData == null) return null;
        foreach (var scope in userData.userKaleidxScopeList)
        {
            if (scope.gateId == gateId) return scope;
        }
        return null;
    }

    private static string ScopeStatusString(UserKaleidxScope scope)
    {
        if (scope == null) return "----";
        string f = scope.isGateFound ? "F" : "-";
        string k = scope.isKeyFound ? "K" : "-";
        string c = scope.isClear ? "C" : "-";
        string w = scope.isInfoWatched ? "W" : "-";
        return $"{f}{k}{c}{w}";
    }

    private static void UnlockAllGates(long index, bool found, bool key, bool clear, bool info)
    {
        var userData = User.GetUserData(index);
        if (userData == null || userData.IsGuest()) return;

        var gates = Singleton<DataManager>.Instance.GetKaleidxScopeGates();
        foreach (var kvp in gates)
        {
            var gate = kvp.Value;
            if (gate == null) continue;
            if (!Singleton<EventManager>.Instance.IsOpenEvent(gate.eventName.id)) continue;

            int gateId = gate.name.id;
            if (found) userData.AddKaleidxScopeGate(gateId);

            if (key)
            {
                var courses = Singleton<DataManager>.Instance.GetKaleidxScopeCourses();
                foreach (var c in courses)
                {
                    if (c.Value.gateName.id == gateId)
                    {
                        userData.AddKaleidxScopeKey(c.Value.keyName.id);
                    }
                }
            }

            if (clear || info)
            {
                var scope = FindScope(userData, gateId);
                if (scope != null)
                {
                    if (clear)
                    {
                        scope.isClear = true;
                        scope.clearDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (info) scope.isInfoWatched = true;
                }
            }
        }
    }

    private static void UnlockAllKeys(long index, bool clear, bool info)
    {
        var userData = User.GetUserData(index);
        if (userData == null || userData.IsGuest()) return;

        var courses = Singleton<DataManager>.Instance.GetKaleidxScopeCourses();
        foreach (var kvp in courses)
        {
            var course = kvp.Value;
            if (course == null) continue;

            var gate = Singleton<DataManager>.Instance.GetKaleidxScopeGate(course.gateName.id);
            if (gate == null) continue;
            if (!Singleton<EventManager>.Instance.IsOpenEvent(gate.eventName.id)) continue;

            userData.AddKaleidxScopeKey(course.keyName.id);

            if (clear || info)
            {
                var scope = FindScope(userData, course.gateName.id);
                if (scope != null)
                {
                    if (clear)
                    {
                        scope.isClear = true;
                        scope.clearDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (info) scope.isInfoWatched = true;
                }
            }
        }
    }

    private static void UnlockKeysForGate(UserData userData, int gateId)
    {
        if (userData == null || userData.IsGuest()) return;

        var courses = Singleton<DataManager>.Instance.GetKaleidxScopeCourses();
        foreach (var kvp in courses)
        {
            var course = kvp.Value;
            if (course == null) continue;
            if (course.gateName.id == gateId)
            {
                userData.AddKaleidxScopeKey(course.keyName.id);
            }
        }
    }

    private static void SetScopeClear(UserData userData, int gateId)
    {
        if (userData == null || userData.IsGuest()) return;

        userData.AddKaleidxScopeGate(gateId);
        UnlockKeysForGate(userData, gateId);

        var scope = FindScope(userData, gateId);
        if (scope != null)
        {
            scope.isClear = true;
            scope.clearDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    private static void ResetScope(UserData userData, int gateId)
    {
        if (userData == null || userData.IsGuest()) return;

        userData.userKaleidxScopeList.RemoveAll(s => s.gateId == gateId);
    }

    private static void ResetAll(long index)
    {
        var userData = User.GetUserData(index);
        if (userData == null || userData.IsGuest()) return;

        userData.userKaleidxScopeList.Clear();
        GameMessageManager.SendMessage((int)index, "KaleidxScope data cleared");
    }
}
