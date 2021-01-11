using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class LuaPowerCustomTooltips
{
	public static void AddTriggerTooltip(FTrigger trigger, string name, string tooltip) {
		S.I.deCtrl.triggerTooltips.Add(trigger);
		LuaPowerLang.ImportTerm("MechKeys/" + trigger.ToString(), name);
		LuaPowerLang.ImportTerm("MechTooltips/" + trigger.ToString(), tooltip);
	}

	public static void AddEffectTooltip(Effect effect, string name, string tooltip) {
		S.I.deCtrl.effectTooltips.Add(effect);
		LuaPowerLang.ImportTerm("MechKeys/" + effect.ToString(), name);
		LuaPowerLang.ImportTerm("MechTooltips/" + effect.ToString(), tooltip);
	}
}