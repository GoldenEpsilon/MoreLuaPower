using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

public class LuaPowerCustomEnumsSetup
{
    static public void Setup() {
        foreach (Type i in LuaPowerData.customEnums.Keys) {
            LuaPowerData.customEnums[i].Clear();
            LuaPowerData.baseGameEnumAmount.Remove(i);
            foreach (string i2 in Enum.GetNames(i)) {
                LuaPowerData.customEnums[i].Add(i2);
            }
            LuaPowerData.baseGameEnumAmount.Add(i, LuaPowerData.customEnums[i].Count);
        }
    }
}

[HarmonyPatch(typeof(Type))]
[HarmonyPatch("GetEnumData")]
class MoreLuaPower_CustomEnums
{
    static void Postfix(Enum __instance, ref string[] enumNames) {
        foreach (Type i in LuaPowerData.customEnums.Keys) {
            if (__instance.GetType() == i && LuaPowerData.customEnums[i].Count > 0) {
                enumNames = LuaPowerData.customEnums[i].ToArray();
            }
        }
    }
}

[HarmonyPatch(typeof(Enum))]
[HarmonyPatch("GetCachedValuesAndNames")]
class MoreLuaPower_CustomEnumsParse
{
    static void Postfix(ref object __result, object enumType, bool getNames) {
        FieldInfo n = __result.GetType().GetField("Names", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        FieldInfo v = __result.GetType().GetField("Values", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (Type i in LuaPowerData.customEnums.Keys) {
            string[] names = (string[])n.GetValue(__result);
            if ((Type)enumType == i && LuaPowerData.customEnums[i].Count > names.Length) {
                if (getNames) {
                    n.SetValue(__result, LuaPowerData.customEnums[i].ToArray());
                }
                List<ulong> values = new List<ulong>();
                for (int i2 = 0; i2 < LuaPowerData.customEnums[i].Count; i2++) {
                    values.Add((ulong)i2);
                }
                v.SetValue(__result, values.ToArray());
            }
        }
        enumType.GetType().GetField("GenericCache", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).SetValue(enumType, __result);
    }
}