using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using UnityEngine;

class LuaPowerCustomEnumsSetup
{
    static public void Setup() {
        LuaPowerData.statuses = new List<string>();
        foreach (string i in Enum.GetNames(typeof(Status))) {
            LuaPowerData.statuses.Add(i);
        }
        LuaPowerData.brands = new List<string>();
        foreach (string i in Enum.GetNames(typeof(Brand))) {
            LuaPowerData.brands.Add(i);
        }
        LuaPowerData.effects = new List<string>();
        foreach (string i in Enum.GetNames(typeof(Effect))) {
            LuaPowerData.effects.Add(i);
        }
        //LuaPowerData.effects.Add("CallLua"); not implemented yet
    }
}

[HarmonyPatch(typeof(Enum))]
[HarmonyPatch("InternalFormat")]
class MoreLuaPower_CustomEnums
{
    static bool Prefix(Type eT, object value, ref string __result) {
        if (value is int n && n > 1) {
            if (eT == typeof(Status) && LuaPowerData.statuses.Count > 0) {
                __result = LuaPowerData.statuses[n];
                return false;
            }
            if (eT == typeof(Brand) && LuaPowerData.brands.Count > 0) {
                __result = LuaPowerData.brands[n];
                return false;
            }
            if (eT == typeof(Effect) && LuaPowerData.effects.Count > 0) {
                __result = LuaPowerData.effects[n];
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(Enum))]
[HarmonyPatch("GetCachedValuesAndNames")]
class MoreLuaPower_CustomEnumsParse
{
    static void Postfix(ref object __result, object enumType, bool getNames) {
        FieldInfo n = __result.GetType().GetField("Names", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        FieldInfo v = __result.GetType().GetField("Values", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        if (enumType == typeof(Status) && LuaPowerData.statuses.Count > 0) {
            if (getNames) {
                n.SetValue(__result, LuaPowerData.statuses.ToArray());
            }
            List<ulong> values = new List<ulong>();
            for (int i = 0; i < LuaPowerData.statuses.Count; i++) {
                values.Add((ulong)i);
            }
            v.SetValue(__result, values.ToArray());
        }
        if (enumType == typeof(Brand) && LuaPowerData.brands.Count > 0) {
            if (getNames) {
                n.SetValue(__result, LuaPowerData.brands.ToArray());
            }
            List<ulong> values = new List<ulong>();
            for (int i = 0; i < LuaPowerData.brands.Count; i++) {
                values.Add((ulong)i);
            }
            v.SetValue(__result, values.ToArray());
        }
        if (enumType == typeof(Effect) && LuaPowerData.effects.Count > 0) {
            if (getNames) {
                n.SetValue(__result, LuaPowerData.effects.ToArray());
            }
            List<ulong> values = new List<ulong>();
            for (int i = 0; i < LuaPowerData.effects.Count; i++) {
                values.Add((ulong)i);
            }
            v.SetValue(__result, values.ToArray());
        }
        enumType.GetType().GetField("GenericCache", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).SetValue(enumType, __result);
    }
}