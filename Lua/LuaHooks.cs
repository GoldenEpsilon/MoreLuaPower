using HarmonyLib;
using UnityEngine;

class LuaPowerHooks
{
	public static ItemObject triggerItem;
	public static void AddHook(FTrigger trigger, string func) {
		bool duplicate = false;
		foreach (var hook in LuaPowerData.luaHooks) {
			if (hook._trigger == trigger && hook._func == func) {
				duplicate = true;
			}
		}
		if (!duplicate) {
			LuaPowerData.luaHooks.Add(new LuaPowerTrigger(trigger, func));
		}
	}
}

[HarmonyPatch(typeof(Being))]
[HarmonyPatch("TriggerArtifacts")]
class MoreLuaPower_SpellCastHook
{
	static void Postfix(Being __instance, FTrigger fTrigger) {
		if (__instance == S.I.batCtrl.currentPlayer) {
			if ((LuaPowerHooks.triggerItem == null || LuaPowerHooks.triggerItem.item == null) && S.I.batCtrl.currentPlayer != null) {
				LuaPowerHooks.triggerItem = new ItemObject();
				LuaPowerHooks.triggerItem.item = S.I.batCtrl.currentPlayer.gameObject.AddComponent<Artifact>();
				LuaPowerHooks.triggerItem.item.being = S.I.batCtrl.currentPlayer;
			}
			foreach (LuaPowerTrigger hook in LuaPowerData.luaHooks.FindAll((LuaPowerTrigger hook) => { return hook._trigger == fTrigger; })) {
				if (LuaPowerHooks.triggerItem != null) {
					EffectActions.CallFunctionWithItem(hook._func, LuaPowerHooks.triggerItem);
				} else {
					Debug.Log("ERROR: Hooks are not loaded, but it is trying to trigger. Is there a player?");
				}
			}
		}
	}
}
