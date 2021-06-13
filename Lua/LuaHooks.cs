using HarmonyLib;
using UnityEngine;

public class LuaPowerHooks
{
	public static ItemObject triggerItem;
	public static void AddHook(FTrigger trigger, string func, Being being = null) {
		bool duplicate = false;
		foreach (var hook in LuaPowerData.luaHooks) {
			if (hook._trigger == trigger && hook._func == func && 
				((hook._being == MoreLuaPower.GetPlayer() && being == null) || hook._being == being)) {
				duplicate = true;
			}
		}
		if (!duplicate) {
			LuaPowerData.luaHooks.Add(new LuaPowerTrigger(trigger, func, being));
		}
	}
}

[HarmonyPatch(typeof(Being))]
[HarmonyPatch("TriggerArtifacts")]
class MoreLuaPower_SpellCastHook
{
	static void Postfix(Being __instance, FTrigger fTrigger) {
		foreach (LuaPowerTrigger hook in LuaPowerData.luaHooks.FindAll(
			(LuaPowerTrigger hook) => { 
				return hook._trigger == fTrigger && 
					(hook._being == __instance || 
						(hook._being == null && 
						__instance == MoreLuaPower.GetPlayer())
					); 
			}
		)) {
			if ((LuaPowerHooks.triggerItem == null || LuaPowerHooks.triggerItem.item == null)) {
				LuaPowerHooks.triggerItem = new ItemObject();
				LuaPowerHooks.triggerItem.being = __instance;
				LuaPowerHooks.triggerItem.item = __instance.gameObject.AddComponent<MPLHook>();
				LuaPowerHooks.triggerItem.item.being = __instance;
				LuaPowerHooks.triggerItem.item.itemObj = LuaPowerHooks.triggerItem;
			}
			EffectActions.CallFunctionWithItem(hook._func, LuaPowerHooks.triggerItem);
		}
	}
}
