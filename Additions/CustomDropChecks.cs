using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LuaPowerDropChecks
{
	public static void AddDropCheck(string name, string funcName) {
		LuaPowerData.dropChecks.Add(S.I.itemMan.itemDictionary[name], funcName);
	}
}

[HarmonyPatch(typeof(ItemManager))]
[HarmonyPatch("GetItems")]
static class MoreLuaPower_CustomDropChecks
{
	static void Postfix(ref List<ItemObject> __result, ItemManager __instance, int rarity, int amountNeeded, ItemType itemType, bool unique, Brand brand, List<ItemObject> bannedList) {
		foreach (var i in __result) {
			if (LuaPowerData.dropChecks.ContainsKey(i)) {
				Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
				if (!mainscr.Call(mainscr.Globals[LuaPowerData.dropChecks[i]], new object[] { i }).Boolean) {
					__result.Remove(i);
					__result.Add(__instance.GetItems(rarity, 1, itemType, unique, brand, bannedList)[0]);
				}
			}
		}
	}
}