using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaPowerDropChecks
{
	public static void AddDropCheck(string name, string funcName) {
		S.I.mainCtrl.StartCoroutine(_AddDropCheck(name, funcName));
	}
	public static IEnumerator _AddDropCheck(string name, string funcName) {
		yield return new WaitForSeconds(0f);
		LuaPowerData.dropChecks.Add(name, funcName);
	}
}

[HarmonyPatch(typeof(ItemManager))]
[HarmonyPatch("GetItems")]
static class MoreLuaPower_CustomDropChecks
{
	static void Postfix(ref List<ItemObject> __result, ItemManager __instance, int rarity, int amountNeeded, ItemType itemType, bool unique, Brand brand, List<ItemObject> bannedList) {
		for (int i = __result.Count - 1; i >= 0; i--) {
			if (LuaPowerData.dropChecks.ContainsKey(__result[i].nameString)) {
				Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
				if (!mainscr.Call(mainscr.Globals[LuaPowerData.dropChecks[__result[i].nameString]], new object[] { __result[i] }).Boolean) {
					__result.Remove(__result[i]);
					__result.Add(__instance.GetItems(rarity, 1, itemType, unique, brand, bannedList)[0]);
				}
			}
		}
	}
}