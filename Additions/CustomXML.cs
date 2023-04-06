using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HarmonyLib;

[HarmonyPatch(typeof(ItemManager))]
[HarmonyPatch("ReadSpellFile")]
class MoreLuaPower_XmlSpells
{
	static void Postfix(ItemManager __instance) {
		string s = S.I.xmlReader.GetDataFile("Spells.xml");
		for (int i = -1; i < S.I.modCtrl.spellMods.Count; i++) {
			if (i != -1) {
				s = S.I.modCtrl.spellMods[i];
			}
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(s));
			int num = 0;
			if (xmlTextReader.ReadToDescendant("Spells") && xmlTextReader.ReadToDescendant("Spell")) {
				do {
					num++;
					var id = xmlTextReader.GetAttribute("itemID");
					if (__instance.spellDictionary.ContainsKey(id)) {
						SpellObject spellObject = __instance.spellDictionary[id];
						if (!spellObject.paramDictionary.ContainsKey("MPL_XML")) {
							spellObject.paramDictionary["MPL_XML"] = "Checked";

							XmlReader xmlReader = xmlTextReader.ReadSubtree();
							while (xmlReader.Read()) {
								if (!xmlReader.IsEmptyElement) {
									string name = xmlReader.Name;
									switch (name) {
										case "App":
											EffectApp.AddTo(xmlReader, spellObject, spellObject.efApps, xmlReader.GetAttribute("trigger"), xmlReader.GetAttribute("effect"));
											break;
									}
								}
							}

									__instance.spellDictionary[spellObject.itemID] = spellObject;
							__instance.itemDictionary[spellObject.itemID] = spellObject;
						}
					}
				}
				while (xmlTextReader.ReadToNextSibling("Spell"));
			}
		}
	}
}