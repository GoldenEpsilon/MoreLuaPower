using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class LuaPowerCustomTooltips
{
	public static void AddTriggerTooltip(string trigger, string name, string tooltip) {
		S.I.deCtrl.triggerTooltips.Add((FTrigger)Enum.Parse(typeof(FTrigger), trigger));
		LuaPowerLang.ImportTerm("MechKeys/" + trigger.ToString(), name);
		LuaPowerLang.ImportTerm("MechTooltips/" + trigger.ToString(), tooltip);
	}

	public static void AddEffectTooltip(string effect, string name, string tooltip) {
		S.I.deCtrl.effectTooltips.Add((Effect)Enum.Parse(typeof(Effect), effect));
		LuaPowerLang.ImportTerm("MechKeys/" + effect.ToString(), name);
		LuaPowerLang.ImportTerm("MechTooltips/" + effect.ToString(), tooltip);
	}
}