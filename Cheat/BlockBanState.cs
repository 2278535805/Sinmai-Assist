using HarmonyLib;
using Manager.UserDatas;
using Process.Entry.State;

namespace SinmaiAssist.Cheat;

/// <summary>
/// 屏蔽 banState 相关判定。
/// - 所有读取 UserDetail.BanState 的代码（包括未来新增的判定）统一返回 0；
/// - 仅在 UserDetail 导出为上传用 VO 期间（VOExtensions.Export）放行真实值，保证上传数据不变；
/// - ConfirmPlay.GetWarningState 决定警告/BAN 界面，同样返回 0。
/// </summary>
public class BlockBanState
{
    [System.ThreadStatic]
    private static int _exportDepth;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserDetail), nameof(UserDetail.BanState), MethodType.Getter)]
    public static bool BanStateGetterPrefix(ref int __result)
    {
        if (_exportDepth > 0) return true;
        __result = 0;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Net.VO.Mai2.VOExtensions), nameof(Net.VO.Mai2.VOExtensions.Export), typeof(UserDetail))]
    public static void ExportPrefix()
    {
        _exportDepth++;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(Net.VO.Mai2.VOExtensions), nameof(Net.VO.Mai2.VOExtensions.Export), typeof(UserDetail))]
    public static void ExportFinalizer()
    {
        _exportDepth--;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConfirmPlay), "GetWarningState")]
    public static void GetWarningStatePostfix(ref int __result)
    {
        __result = 0;
    }
}
