using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static class LuaPowerDropChecks
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
			if (LuaPowerData.dropChecks.ContainsKey(__result[i].itemID)) {
				Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
				if (!mainscr.Call(mainscr.Globals[LuaPowerData.dropChecks[__result[i].itemID]], new object[] { __result[i] }).Boolean) {
					__result.Remove(__result[i]);
					__result.Add(__instance.GetItems(rarity, 1, itemType, unique, brand, bannedList)[0]);
				}
			}
		}
	}
}

[HarmonyPatch(typeof(PostCtrl))]
[HarmonyPatch("StartOptions")]
static class MoreLuaPower_CustomDropChecks_Boss
{
	static void Prefix(PostCtrl __instance, ref List<ItemObject> finalOptions, RewardType rewardType, int siblingIndex = -1)
    {
		//Regenerate boss reward
		if(S.I.runCtrl.currentZoneDot.type == ZoneType.Boss)
        {
			for (int i = finalOptions.Count - 1; i >= 0; i--)
			{
				if (LuaPowerData.dropChecks.ContainsKey(finalOptions[i].itemID))
				{
					Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
					if (!mainscr.Call(mainscr.Globals[LuaPowerData.dropChecks[finalOptions[i].itemID]], new object[] { finalOptions[i] }).Boolean)
					{
						string removedID = finalOptions[i].itemID;
						finalOptions.Remove(finalOptions[i]);
						List<ArtifactObject> source = S.I.itemMan.bossRewards;
						if (S.I.batCtrl.currentPlayer.health.current == S.I.batCtrl.currentPlayer.health.max)
							source = source.Where<ArtifactObject>((Func<ArtifactObject, bool>)(t => !t.tags.Contains(Tag.Heal))).Where(a => a.itemID != removedID).ToList<ArtifactObject>();
						finalOptions.Add((ItemObject)source[S.I.runCtrl.NextPsuedoRand(0, source.Count)].Clone());
					}
				}
			}
		}
		//Regenerate miniboss reward
		if(S.I.runCtrl.currentZoneDot.type == ZoneType.Miniboss)
        {
			for (int i = finalOptions.Count - 1; i >= 0; i--)
			{
				if (LuaPowerData.dropChecks.ContainsKey(finalOptions[i].itemID))
				{
					Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
					if (!mainscr.Call(mainscr.Globals[LuaPowerData.dropChecks[finalOptions[i].itemID]], new object[] { finalOptions[i] }).Boolean)
					{
						string removedID = finalOptions[i].itemID;
						finalOptions.Remove(finalOptions[i]);
						List<ArtifactObject> source = S.I.itemMan.minibossRewards;
						if (S.I.batCtrl.currentPlayer.health.current == S.I.batCtrl.currentPlayer.health.max)
							source = source.Where<ArtifactObject>((Func<ArtifactObject, bool>)(t => !t.tags.Contains(Tag.Heal))).Where(a => a.itemID != removedID).ToList<ArtifactObject>();
						finalOptions.Add((ItemObject)source[S.I.runCtrl.NextPsuedoRand(0, source.Count)].Clone());
					}
				}
			}
		}
    }
}