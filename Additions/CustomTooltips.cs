using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

static class LuaPowerCustomTooltips
{
	public static List<string> customTooltips = new List<string>();

	public static void AddTriggerTooltip(string trigger, string name, string tooltip) {
		S.I.deCtrl.triggerTooltips.Add((FTrigger)Enum.Parse(typeof(FTrigger), trigger));
		LuaPowerLang.ImportTerm("MechKeys/" + trigger.ToString(), name);
		LuaPowerLang.ImportTerm("MechTooltips/" + trigger.ToString(), tooltip);
	}

	public static void AddEffectTooltip(string effect, string name, string tooltip) {
		Effect effectEnum;
		if (!string.IsNullOrEmpty(effect) && Enum.TryParse(effect, out effectEnum))
		{
			S.I.deCtrl.effectTooltips.Add((Effect)Enum.Parse(typeof(Effect), effect));
		} else
        {
			customTooltips.Add(effect);
        }
		LuaPowerLang.ImportTerm("MechKeys/" + effect, name);
		LuaPowerLang.ImportTerm("MechTooltips/" + effect, tooltip);
	}


}

[HarmonyPatch]
static class LuaPowerCustomTooltipsPatch
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardInner), "MechTooltipInstance")]
	static bool MechTooltipInstance(CardInner __instance, EffectApp efApp, string dictKey)
    {
		if (dictKey == "Lua")
		{
			//Need to go ahead and add MechDetails for all Lua effApps on this card since otherwise they'll be treated as duplicate "Lua" effects
			for (int index = __instance.itemObj.efApps.Count - 1; index >= 0; --index)
			{
				var thisApp = __instance.itemObj.efApps[index];
				if (thisApp.effect == (Effect)Enum.Parse(typeof(Effect),"Lua"))
				{
					var effectKey = thisApp.value;
					if (LuaPowerCustomTooltips.customTooltips.Contains(effectKey))
					{
						MechDetails mechDetails = UnityEngine.Object.Instantiate<MechDetails>(__instance.deCtrl.mechTooltip, __instance.mechTooltipGrid);
						mechDetails.cardInner = __instance;
						mechDetails.FillTra(thisApp, __instance.deCtrl.ctrl.effectSpritesDict[effectKey], effectKey);
					}
				}
			}
			return false;
		}

		return true;
    }

}

