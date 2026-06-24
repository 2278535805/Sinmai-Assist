using SinmaiAssist.Cheat;
using UnityEngine;

namespace SinmaiAssist.GUI;

public class FastSkipPanel
{
    private static string _scoreInput = "0";
    private static string _jCriticalInput = "0";
    private static string _jPerfectInput = "0";
    private static string _jGreatInput = "0";
    private static string _jGoodInput = "0";
    private static string _jMissInput = "0";

    public static void OnGUI()
    {
        FastSkip.SkipButton = false;

        string modeName = FastSkip.CurrentSkipMode switch
        {
            FastSkip.SkipMode.Ratio => "Ratio",
            FastSkip.SkipMode.Custom => "Custom",
            _ => "Default"
        };
        GUILayout.Label($"Skip Mode: {modeName}", MainGUI.Style.Text);

        if (GUILayout.Button("Skip", new GUIStyle(MainGUI.Style.Button) { fontSize = 20 }, GUILayout.Height(45f)))
            FastSkip.SkipButton = true;

        GUILayout.Label("Mode Setting", MainGUI.Style.Title);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Default", MainGUI.Style.Button))
            FastSkip.CurrentSkipMode = FastSkip.SkipMode.Default;
        if (GUILayout.Button("Custom", MainGUI.Style.Button))
            FastSkip.CurrentSkipMode = FastSkip.SkipMode.Custom;
        if (GUILayout.Button("Ratio", MainGUI.Style.Button))
            FastSkip.CurrentSkipMode = FastSkip.SkipMode.Ratio;
        GUILayout.EndHorizontal();

        if (FastSkip.CurrentSkipMode == FastSkip.SkipMode.Custom)
        {
            GUILayout.Label("Custom Setting", MainGUI.Style.Title);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Achievement (0-101): ", MainGUI.Style.Text);
            _scoreInput = GUILayout.TextField(_scoreInput);
            GUILayout.EndHorizontal();
            if (int.TryParse(_scoreInput, out int scoreValue))
            {
                if (scoreValue >= 0f && scoreValue <= 101f)
                {
                    FastSkip.CustomAchivement = scoreValue;
                    GUILayout.Label($"  Value: {scoreValue}%", MainGUI.Style.Text);
                }
                else
                {
                    GUILayout.Label("  Error: 0 - 101", MainGUI.Style.ErrorMessage);
                }
            }
            else
            {
                GUILayout.Label("  Error: Invalid int", MainGUI.Style.ErrorMessage);
            }
            FastSkip.Force1Miss = GUILayout.Toggle(FastSkip.Force1Miss, "Force 1 Miss");
        }

        if (FastSkip.CurrentSkipMode == FastSkip.SkipMode.Ratio)
        {
            GUILayout.Label("Judgment Counts Ratio", MainGUI.Style.Title);

            DrawCountInput(ref _jCriticalInput, "Critical", ref FastSkip.JudgmentCritical);
            DrawCountInput(ref _jPerfectInput, "Perfect", ref FastSkip.JudgmentPerfect);
            DrawCountInput(ref _jGreatInput, "Great", ref FastSkip.JudgmentGreat);
            DrawCountInput(ref _jGoodInput, "Good", ref FastSkip.JudgmentGood);
            DrawCountInput(ref _jMissInput, "Miss", ref FastSkip.JudgmentMiss);

            int total = FastSkip.JudgmentCritical + FastSkip.JudgmentPerfect + FastSkip.JudgmentGreat
                        + FastSkip.JudgmentGood + FastSkip.JudgmentMiss;
            GUILayout.Label($"Total Notes: {total}", MainGUI.Style.Text);
        }
    }

    private static void DrawCountInput(ref string input, string label, ref int target)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, new GUIStyle(MainGUI.Style.Text) { fixedWidth = 80 });
        input = GUILayout.TextField(input);
        GUILayout.EndHorizontal();
        if (int.TryParse(input, out int value) && value >= 0)
        {
            target = value;
        }
        else if (!string.IsNullOrEmpty(input))
        {
            GUILayout.Label("  Error: Invalid count", MainGUI.Style.ErrorMessage);
        }
    }
}
