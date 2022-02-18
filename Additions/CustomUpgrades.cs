using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
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
	}
	static public void AddXMLToSpell(SpellObject spellObj, string XML) {
		XmlReader reader = XmlReader.Create(new StringReader("<Spell itemID=\""+ spellObj.itemID +"\">" + XML + "</Spell>"));
		if (reader.ReadToDescendant("Spell")) {
			spellObj.ReadXmlPrototype(reader);
		}
	}

	static public void ApplyRandomUpgrade(SpellObject spellObj) {
		int count = spellObj.enhancements.Count;
		List<int> intList = Utils.RandomList(Enum.GetNames(typeof(Enhancement)).Length, true);
		for (int i = 0; i < intList.Count; ++i)
		{
			S.I.poCtrl.EnhanceSpell(spellObj, (Enhancement)intList[i]);
			if (spellObj.enhancements.Count > count)
			{
				count = spellObj.enhancements.Count;
				break;
			}
		}
	}
}

[HarmonyPatch(typeof(PostCtrl))]
[HarmonyPatch("EnhanceSpell")]
static class MoreLuaPower_CustomUpgrades 
{
	static void Prefix(SpellObject spellObj, Enhancement enhancement) {
		if ((int)enhancement >= LuaPowerData.baseGameEnumAmount[typeof(Enhancement)] && LuaPowerData.customUpgrades.ContainsKey(enhancement.ToString())) {
			Script mainscr = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>();
			MPLog.Log(" Creating Enhancement: " + enhancement.ToString() + 
			" Check function: " + LuaPowerData.customUpgrades[enhancement.ToString()].Item2
			, LogLevel.Info);
			if (mainscr.Call(mainscr.Globals[LuaPowerData.customUpgrades[enhancement.ToString()].Item2], new object[] { spellObj }).Boolean) {
				MPLog.Log(" Creating Enhancement: " + enhancement.ToString() +
				" Apply function: " + LuaPowerData.customUpgrades[enhancement.ToString()].Item3
				, LogLevel.Info);
				mainscr.Call(mainscr.Globals[LuaPowerData.customUpgrades[enhancement.ToString()].Item3], new object[] { spellObj });
				spellObj.nameString += U.I.Colorify(" " + LuaPowerData.customUpgrades[enhancement.ToString()].Item1, UIColor.Enhancement);
				spellObj.enhancements.Add(enhancement);
			}
		}
	}
}