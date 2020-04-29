using HarmonyLib;
using System;

[HarmonyPatch(typeof(Enum))]
[HarmonyPatch("InternalFormat")]
class MoreLuaPower_CustomEnumStrings
{
    static bool Prefix(Type eT, object value, ref string __result) {
        if (eT == typeof(Status) && value is int n && n > 1) {
            __result = LuaPowerData.statuses[n];
            return false;
        }
        return true;
    }
}