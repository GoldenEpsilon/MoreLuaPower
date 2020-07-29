using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

static class LuaPowerUpgrades
{
	static public void AddUpgrade(string name, string abbreviation, string description, string check, string effect){
		if (LuaPowerData.customUpgrades.ContainsKey(name)) {
			Debug.LogError("'" + name + "' is already an upgrade.");
			return;
		}
		object obj = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals[check];
		if (obj == null) {
			Debug.LogError("'" + check + "' is not a LUA Function.");
			return;
		}
		object obj2 = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals[effect];
		if (obj2 == null) {
			Debug.LogError("'" + check + "' is not a LUA Function.");
			return;
		}
		LuaPowerData.customUpgrades.Add(name, new Tuple<string, string, string>(abbreviation, check, effect));
		if (!LuaPowerData.customEnums[typeof(Enhancement)].Contains(name)) { LuaPowerData.customEnums[typeof(Enhancement)].Add(name); }
		LuaPowerLang.ImportTerm("Enhancements/"+name, description);
		//EffectActions.CallFunctionWithItem(check, );
	}
}

[HarmonyPatch(typeof(PostCtrl))]
[HarmonyPatch("EnhanceSpell")]
static class MoreLuaPower_CustomUpgrades 
{
	static void Prefix(SpellObject spellObj, Enhancement enhancement) {
		Debug.Log(Enum.GetValues(typeof(Enhancement)).Cast<Enhancement>().ToList<Enhancement>().Count);
		if ((int)enhancement >= LuaPowerData.baseGameEnumAmount[typeof(Enhancement)] && LuaPowerData.customUpgrades.ContainsKey(enhancement.ToString())) {
			Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
			if (mainscr.Call(mainscr.Globals[LuaPowerData.customUpgrades[enhancement.ToString()].Item2], new object[] { spellObj }).Boolean) {
				mainscr.Call(mainscr.Globals[LuaPowerData.customUpgrades[enhancement.ToString()].Item3], new object[] { spellObj });
				spellObj.nameString += U.I.Colorify(" " + LuaPowerData.customUpgrades[enhancement.ToString()].Item1, UIColor.Enhancement);
				spellObj.enhancements.Add(enhancement);
			}
		}
	}
}