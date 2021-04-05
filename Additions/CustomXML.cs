using System;
using System.Collections.Generic;
using System.Xml;
using HarmonyLib;

/*class LuaPowerXMLAttributes
{
	static public string GetXMLAttribute(EffectApp efApp, string attribute) {
		return LuaPowerData.xmlAttributes[efApp][attribute];
	}
}

[HarmonyPatch(typeof(EffectApp))]
[HarmonyPatch("AddTo")]
class MoreLuaPower_XmlAttributes
{
	static void Postfix(XmlReader reader, List<EffectApp> appList) {
		reader.MoveToFirstAttribute();
		var d = new Dictionary<string, string>();
		while (reader.MoveToNextAttribute()) {
			d.Add(reader.Name, reader.Value);
		}
		if (appList.Count > 0) {
			LuaPowerData.xmlAttributes.Add(appList[appList.Count - 1], d);
		}
	}
}*/