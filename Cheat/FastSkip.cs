using HarmonyLib;
using MAI2.Util;
using Manager;
using Manager.UserDatas;
using MelonLoader;
using Monitor;
using Process;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SinmaiAssist.Cheat;

internal class FastSkip
{
    public enum SkipMode
    {
        Default,
        Custom,
        Ratio
    }

    private enum GameSequence
    {
        Init,
        Sync,
        Start,
        StartWait,
        Play,
        PlayEnd,
        Result,
        ResultEnd,
        FinalWait,
        Release
    }

    public static SkipMode CurrentSkipMode = SkipMode.Default;
    public static bool SkipButton = false;
    public static bool Force1Miss = false;
    public static int CustomAchivement = 0;

    public static int JudgmentCritical = 0;
    public static int JudgmentPerfect = 0;
    public static int JudgmentGreat = 0;
    public static int JudgmentGood = 0;
    public static int JudgmentMiss = 0;

    private static bool _isSkip = false;
    private static bool _Miss = false;

    private static readonly List<NoteJudge.ETiming> PerfectPool = new List<NoteJudge.ETiming>
    {
        NoteJudge.ETiming.FastPerfect,
        NoteJudge.ETiming.LatePerfect,
        NoteJudge.ETiming.FastPerfect2nd,
        NoteJudge.ETiming.LatePerfect2nd
    };

    private static readonly List<NoteJudge.ETiming> GreatPool = new List<NoteJudge.ETiming>
    {
        NoteJudge.ETiming.FastGreat,
        NoteJudge.ETiming.FastGreat2nd,
        NoteJudge.ETiming.LateGreat,
        NoteJudge.ETiming.LateGreat2nd,
        NoteJudge.ETiming.LateGreat3rd
    };

    private static readonly List<NoteJudge.ETiming> GoodPool = new List<NoteJudge.ETiming>
    {
        NoteJudge.ETiming.FastGood,
        NoteJudge.ETiming.LateGood
    };

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameProcess), "OnUpdate")]
    public static void Skip(GameProcess __instance)
    {
        try
        {
            System.Type processBaseType = typeof(GameProcess).BaseType;
            GameSequence _sequence = (GameSequence)typeof(GameProcess).GetField("_sequence", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var UpdateSubbMonitorDataMethod = typeof(GameProcess).GetMethod("UpdateSubbMonitorData", BindingFlags.NonPublic | BindingFlags.Instance);
            var SetReleaseMethod = typeof(GameProcess).GetMethod("SetRelease", BindingFlags.NonPublic | BindingFlags.Instance);
            var IsPartyPlayMethod = typeof(GameProcess).GetMethod("IsPartyPlay", BindingFlags.NonPublic | BindingFlags.Instance);
            var containerField = processBaseType.GetField("container", BindingFlags.NonPublic | BindingFlags.Instance);
            ProcessDataContainer container = (ProcessDataContainer)containerField.GetValue(__instance);
            if (_sequence >= GameSequence.Play && _sequence < GameSequence.Release && !GameManager.IsNoteCheckMode)
            {
                _isSkip = false;
                if (DebugInput.GetKeyDown(KeyCode.Space) || SkipButton)
                {
                    _isSkip = true;
                    GameMonitor[] monitors = (GameMonitor[])typeof(GameProcess).GetField("_monitors", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    if (CurrentSkipMode == SkipMode.Custom || CurrentSkipMode == SkipMode.Ratio)
                    {
                        for (int i = 0; i < monitors.Length; i++)
                        {
                            monitors[i].Seek(0);
                        }
                        NotesManager.StartPlay(0);
                        NotesManager.Pause(true);
                        bool IsPartyPlay = (bool)IsPartyPlayMethod.Invoke(__instance, null);
                        Singleton<GamePlayManager>.Instance.Initialize(IsPartyPlay);
                        uint maxCombo = 0u;
                        for (int i = 0; i < monitors.Length; i++)
                        {
                            if (Singleton<UserDataManager>.Instance.GetUserData(i).IsEntry)
                            {
                                monitors[i].ForceAchivement(CurrentSkipMode == SkipMode.Ratio ? 0 : CustomAchivement, 0);
                                maxCombo += Singleton<GamePlayManager>.Instance.GetGameScore(i).MaxCombo;
                            }
                        }
                        GameScoreList gameScore = Singleton<GamePlayManager>.Instance.GetGameScore(2);
                        if (gameScore.IsEnable && !gameScore.IsHuman())
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                if (Singleton<UserDataManager>.Instance.GetUserData(i).IsEntry && GameManager.SelectGhostID[i] != GhostManager.GhostTarget.End)
                                {
                                    UserGhost ghostToEnum = Singleton<GhostManager>.Instance.GetGhostToEnum(GameManager.SelectGhostID[i]);
                                    gameScore.SetForceAchivement_Battle((float)GameManager.ConvAchiveIntToDecimal(ghostToEnum.Achievement));
                                    break;
                                }
                            }
                        }
                        for (int i = 0; i < monitors.Length; i++)
                        {
                            if (Singleton<UserDataManager>.Instance.GetUserData(i).IsEntry)
                            {
                                Singleton<GamePlayManager>.Instance.GetGameScore(i).SetChain(maxCombo);
                            }
                        }
                    }
                    if (_isSkip)
                    {
                        for (int i = 0; i < monitors.Length; i++)
                        {
                            if (Singleton<UserDataManager>.Instance.GetUserData(i).IsEntry)
                            {
                                UpdateSubbMonitorDataMethod.Invoke(__instance, new object[] { i });
                                Message[] message = (Message[])typeof(GameProcess).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                                container.processManager.SendMessage(message[i]);
                                Singleton<GamePlayManager>.Instance.SetSyncResult(i);
                            }
                        }
                        SetReleaseMethod.Invoke(__instance, null);
                        SkipButton = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameScoreList), "SetForceAchivement")]
    public static bool SetForceAchivement(int achivement, int dxscore, GameScoreList __instance)
    {
        if (CurrentSkipMode == SkipMode.Ratio)
        {
            return SetForceAchivementByJudgment(__instance);
        }

        decimal num1 = achivement;
        if (num1 >= 100.0m && num1 <= 100.4m) num1 = 100.3m;
        long num2;
        long num3;
        if (num1 > 100.0m)
        {
            num2 = (long)((decimal)__instance.ScoreTotal._allPerfectScore * (num1 - 1.0m) * 0.01m);
            num3 = __instance.ScoreTotal._breakBonusScore;
        }
        else
        {
            num2 = (long)((decimal)__instance.ScoreTotal._allPerfectScore * (num1 * 0.99m * 0.01m));
            num3 = (long)((decimal)__instance.ScoreTotal._breakBonusScore * num1 * 0.01m);
        }
        NoteJudge.ETiming[] NoteArray = new NoteJudge.ETiming[13]
        {
            NoteJudge.ETiming.Critical,
            NoteJudge.ETiming.FastPerfect,
            NoteJudge.ETiming.LatePerfect,
            NoteJudge.ETiming.FastPerfect2nd,
            NoteJudge.ETiming.LatePerfect2nd,
            NoteJudge.ETiming.FastGreat,
            NoteJudge.ETiming.FastGreat2nd,
            NoteJudge.ETiming.LateGreat,
            NoteJudge.ETiming.FastGreat2nd,
            NoteJudge.ETiming.LateGreat2nd,
            NoteJudge.ETiming.LateGreat3rd,
            NoteJudge.ETiming.FastGood,
            NoteJudge.ETiming.LateGood
        };
        int monitorIndex = (int)typeof(GameScoreList).GetField("_monitorIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        NoteDataList noteList = NotesManager.Instance(monitorIndex).getReader().GetNoteList();

        System.Random rng = new System.Random();
        int fastCount = 0;
        int lateCount = 0;

        foreach (NoteData breakNoteData in noteList)
        {
            if (!breakNoteData.type.isBreakScore()) continue;

            bool flag = false;
            NoteScore.EScoreType eScoreType = GamePlayManager.NoteType2ScoreType(breakNoteData.type);
            List<NoteJudge.ETiming> validTimings = new List<NoteJudge.ETiming>();
            foreach (NoteJudge.ETiming eTiming in NoteArray)
            {
                if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming, NoteScore.EScoreType.Break)) && 0m <= (decimal)(num3 - NoteScore.GetJudgeScore(eTiming, NoteScore.EScoreType.BreakBonus)))
                {
                    validTimings.Add(eTiming);
                }
            }
            if (validTimings.Count > 0)
            {
                NoteJudge.ETiming chosen = PickWeightedTiming(validTimings, rng, ref fastCount, ref lateCount);
                num2 -= NoteScore.GetJudgeScore(chosen, eScoreType);
                num3 -= NoteScore.GetJudgeScore(chosen, NoteScore.EScoreType.BreakBonus);
                __instance.SetResult(breakNoteData.indexNote, eScoreType, chosen);
                flag = true;
            }
            if (!flag)
            {
                __instance.SetResult(breakNoteData.indexNote, NoteScore.EScoreType.Break, NoteJudge.ETiming.TooFast);
                _Miss = true;
            }
        }

        List<KeyValuePair<NoteData, NoteScore.EScoreType>> nonBreakNotes = new List<KeyValuePair<NoteData, NoteScore.EScoreType>>();
        foreach (NoteData noteData in noteList)
        {
            if (!noteData.type.isBreakScore())
            {
                nonBreakNotes.Add(new KeyValuePair<NoteData, NoteScore.EScoreType>(noteData, GamePlayManager.NoteType2ScoreType(noteData.type)));
            }
        }
        Shuffle(nonBreakNotes, rng);

        foreach (var pair in nonBreakNotes)
        {
            NoteData noteData = pair.Key;
            NoteScore.EScoreType eScoreType = pair.Value;

            if (Force1Miss && !_Miss && noteData.type.isTapScore())
            {
                __instance.SetResult(noteData.indexNote, eScoreType, NoteJudge.ETiming.TooFast);
                _Miss = true;
                continue;
            }

            List<NoteJudge.ETiming> validTimings = new List<NoteJudge.ETiming>();
            foreach (NoteJudge.ETiming eTiming in NoteArray)
            {
                if (0m <= (decimal)(num2 - NoteScore.GetJudgeScore(eTiming, eScoreType)))
                {
                    validTimings.Add(eTiming);
                }
            }
            if (validTimings.Count > 0)
            {
                NoteJudge.ETiming chosen = PickWeightedTiming(validTimings, rng, ref fastCount, ref lateCount);
                num2 -= NoteScore.GetJudgeScore(chosen, eScoreType);
                __instance.SetResult(noteData.indexNote, eScoreType, chosen);
            }
            else
            {
                __instance.SetResult(noteData.indexNote, eScoreType, NoteJudge.ETiming.TooFast);
                _Miss = true;
            }
        }
        return false;
    }

    private static NoteJudge.ETiming PickWeightedTiming(List<NoteJudge.ETiming> validTimings, System.Random rng, ref int fastCount, ref int lateCount)
    {
        List<KeyValuePair<NoteJudge.ETiming, double>> weighted = new List<KeyValuePair<NoteJudge.ETiming, double>>();
        foreach (var t in validTimings)
        {
            double w = GetTimingWeight(t);
            if (IsFastTiming(t) && lateCount > fastCount)
                w *= 2.0;
            else if (IsLateTiming(t) && fastCount > lateCount)
                w *= 2.0;
            weighted.Add(new KeyValuePair<NoteJudge.ETiming, double>(t, w));
        }

        double total = 0.0;
        for (int i = 0; i < weighted.Count; i++)
            total += weighted[i].Value;

        double roll = rng.NextDouble() * total;
        double cum = 0.0;
        for (int i = 0; i < weighted.Count; i++)
        {
            cum += weighted[i].Value;
            if (roll <= cum)
            {
                var chosen = weighted[i].Key;
                if (IsFastTiming(chosen)) fastCount++;
                if (IsLateTiming(chosen)) lateCount++;
                return chosen;
            }
        }
        NoteJudge.ETiming last = validTimings[validTimings.Count - 1];
        if (IsFastTiming(last)) fastCount++;
        if (IsLateTiming(last)) lateCount++;
        return last;
    }

    private static double GetTimingWeight(NoteJudge.ETiming timing)
    {
        switch (timing)
        {
            case NoteJudge.ETiming.Critical: return 12.0;
            case NoteJudge.ETiming.FastPerfect:
            case NoteJudge.ETiming.LatePerfect:
            case NoteJudge.ETiming.FastPerfect2nd:
            case NoteJudge.ETiming.LatePerfect2nd: return 3.0;
            case NoteJudge.ETiming.FastGreat:
            case NoteJudge.ETiming.FastGreat2nd:
            case NoteJudge.ETiming.LateGreat:
            case NoteJudge.ETiming.LateGreat2nd:
            case NoteJudge.ETiming.LateGreat3rd: return 2.0;
            case NoteJudge.ETiming.FastGood:
            case NoteJudge.ETiming.LateGood: return 1.0;
            default: return 0.1;
        }
    }

    private static bool IsFastTiming(NoteJudge.ETiming timing)
    {
        return timing.ToString().Contains("Fast");
    }

    private static bool IsLateTiming(NoteJudge.ETiming timing)
    {
        return timing.ToString().Contains("Late");
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private static bool SetForceAchivementByJudgment(GameScoreList __instance)
    {
        try
        {
            int monitorIndex = (int)typeof(GameScoreList).GetField("_monitorIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            NoteDataList noteList = NotesManager.Instance(monitorIndex).getReader().GetNoteList();

            var rng = new System.Random();

            List<NoteJudge.ETiming> timingPool = new List<NoteJudge.ETiming>();
            for (int i = 0; i < JudgmentCritical; i++)
                timingPool.Add(NoteJudge.ETiming.Critical);
            for (int i = 0; i < JudgmentPerfect; i++)
                timingPool.Add(PerfectPool[rng.Next(PerfectPool.Count)]);
            for (int i = 0; i < JudgmentGreat; i++)
                timingPool.Add(GreatPool[rng.Next(GreatPool.Count)]);
            for (int i = 0; i < JudgmentGood; i++)
                timingPool.Add(GoodPool[rng.Next(GoodPool.Count)]);
            for (int i = 0; i < JudgmentMiss; i++)
                timingPool.Add(NoteJudge.ETiming.TooFast);

            List<NoteData> breakNotes = new List<NoteData>();
            List<KeyValuePair<NoteData, NoteScore.EScoreType>> nonBreakNotes = new List<KeyValuePair<NoteData, NoteScore.EScoreType>>();
            foreach (NoteData noteData in noteList)
            {
                if (noteData.type.isBreakScore())
                    breakNotes.Add(noteData);
                else
                    nonBreakNotes.Add(new KeyValuePair<NoteData, NoteScore.EScoreType>(noteData, GamePlayManager.NoteType2ScoreType(noteData.type)));
            }

            int totalNotes = breakNotes.Count + nonBreakNotes.Count;
            while (timingPool.Count < totalNotes)
                timingPool.Add(NoteJudge.ETiming.TooFast);

            Shuffle(timingPool, rng);

            int idx = 0;
            foreach (NoteData breakNote in breakNotes)
            {
                if (idx >= timingPool.Count) break;
                NoteJudge.ETiming timing = timingPool[idx++];
                NoteScore.EScoreType eScoreType = GamePlayManager.NoteType2ScoreType(breakNote.type);
                __instance.SetResult(breakNote.indexNote, eScoreType, timing);
                if (timing == NoteJudge.ETiming.TooFast)
                    _Miss = true;
            }

            Shuffle(nonBreakNotes, rng);
            foreach (var pair in nonBreakNotes)
            {
                if (idx >= timingPool.Count) break;
                NoteData noteData = pair.Key;
                NoteScore.EScoreType eScoreType = pair.Value;
                NoteJudge.ETiming timing = timingPool[idx++];

                if (Force1Miss && !_Miss && noteData.type.isTapScore())
                {
                    __instance.SetResult(noteData.indexNote, eScoreType, NoteJudge.ETiming.TooFast);
                    _Miss = true;
                }
                else
                {
                    __instance.SetResult(noteData.indexNote, eScoreType, timing);
                    if (timing == NoteJudge.ETiming.TooFast)
                        _Miss = true;
                }
            }
        }
        catch (Exception e)
        {
            MelonLogger.Error($"CustomScore Fabrication Error: {e.Message}");
        }
        return false;
    }
}