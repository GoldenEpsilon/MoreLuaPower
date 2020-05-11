/*
 *  More Lua Power, made by Golden Epsilon
 *  Audio loading, ProgramAdvance, Kickers, and Multicast added by Sunreal
 *  PetBuff for MoreLuaPower by stephanreiken
 *  Workshop URL: https://steamcommunity.com/sharedfiles/filedetails/?id=2066319533
 *  GitHub Page: https://github.com/GoldenEpsilon/MoreLuaPower
 *
 *  Please do not include the DLL in your mods directly:
 *      Ask people to download the workshop version instead.
 *      
 *  That said, if there's something you want to modify from the code to make your own harmony mod, feel free!
 *  I am also open to help; If you have something you want to add in here, just let me know/add it in yourself! You will be credited.
*/

//There are some libraries I don't need that are being called here, this is so that I can copy/paste when creating a new file no matter what it needs.
using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Reflection.Emit;
using System.Collections.ObjectModel;
using Rewired.Utils.Classes.Data;
using Rewired;

[HarmonyPatch(typeof(S))]
[HarmonyPatch("Awake")]
class MoreLuaPower
{
    static void Prepare() {
        Debug.Log("MoreLuaPower Version 1.4");
		LuaPowerData.Setup();
		LuaPowerCustomEnumsSetup.Setup();
		//MoreLuaPowerTesting.Test();
	}
}
/*
class MoreLuaPowerTesting
{
	static public void NewBrand(string brand, string brandSprite, string brandBackSprite) {
		if (LuaPowerData.sprites.ContainsKey(brandSprite) && LuaPowerData.sprites.ContainsKey(brandBackSprite)) {
			if (!LuaPowerData.brands.Contains(brand)) { LuaPowerData.brands.Add(brand); }
			if (LuaPowerData.brands.IndexOf(brand) > S.I.deCtrl.brandSprites.Length) {
				Array.Resize(ref S.I.deCtrl.brandSprites, LuaPowerData.brands.IndexOf(brand));
				S.I.deCtrl.brandSprites[LuaPowerData.brands.IndexOf(brand)] = LuaPowerData.sprites[brandSprite];
				S.I.deCtrl.spellBackgroundBrands[LuaPowerData.brands.IndexOf(brand)] = LuaPowerData.sprites[brandBackSprite];
			}
		}
	}
	public static void Test() {
		
		Dictionary<InputAction, List<string>> temp = (Dictionary<InputAction, List<string>>)Traverse.Create(S.I.conCtrl).Field("RewiredActions").GetValue();
		temp.Add((InputAction)17, new List<string> { "Console" });
		Traverse.Create(S.I.conCtrl).Field("RewiredActions").SetValue(temp);
	}
}
//ReInput.mapping.Actions

[HarmonyPatch(typeof(StatusEffect))]
[HarmonyPatch("Set")]
class MoreLuaPower_CustomStatusTranspiler
{
	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
		var foundCut = false;
		int toFind = 4;
		int startIndex = -1, endIndex = -1;

		var codes = new List<CodeInstruction>(instructions);
		for (int i = 0; i < codes.Count; i++) {
			if (foundCut && codes[i].opcode == OpCodes.Callvirt) {
				toFind--;
				if (toFind <= 0) {
					//Debug.Log("END " + (i));
					endIndex = i;
					break;
				}
			} else if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Callvirt) {
				//Debug.Log("START " + (i));
				startIndex = i;
				foundCut = true;
			}
		}
		if (startIndex > -1 && endIndex > -1) {
			codes.RemoveRange(startIndex, endIndex - startIndex - 1);
		} else {
			Debug.Log("ERROR: Could not find start/end");
			Debug.Log("START " + startIndex);
			Debug.Log("END " + endIndex);
		}

		return codes.AsEnumerable();
	}
}

[HarmonyPatch(typeof(ControlsCtrl))]
[HarmonyPatch("CreateBindingButtons")]
class MoreLuaPower_CustomInput
{
	static void Postfix(ControlsCtrl __instance, string controllerID, int playerNum) {
		var t1 = Status.AtkDmg;
		var s1 = t1.ToString();
		Console.WriteLine((int)t1);
		Console.WriteLine(s1);

		var t2 = (Status)(LuaPowerData.statuses.Count - 1);
		var s2 = t2.ToString();
		Console.WriteLine((int)t2);
		Console.WriteLine(s2);
		Debug.Log(1);
		global::InputAction inputAction = (global::InputAction)17;
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
		if (inputAction != global::InputAction.SignIn) {
			if (rewiredPlayer != null) {
				bool flag = false;
				foreach (string name in ControlsCtrl.RewiredActions[inputAction]) {
					InputMapCategory mapCategoryForAction = (InputMapCategory)Traverse.Create(__instance).Method("GetMapCategoryForAction", new object[] { ReInput.mapping.GetAction(name) }).GetValue();
					if (ControlsCtrl.ControllerIDIsKeyboard((string)Traverse.Create(__instance).Field("currentBindingControllerID").GetValue())) {
						using (IEnumerator<ControllerMap> enumerator2 = rewiredPlayer.controllers.maps.GetAllMaps(ControllerType.Keyboard).GetEnumerator()) {
							while (enumerator2.MoveNext()) {
								if (enumerator2.Current.categoryId == mapCategoryForAction.id) {
									flag = true;
									break;
								}
							}
							continue;
						}
					}
					using (IEnumerator<ControllerMap> enumerator2 = rewiredPlayer.controllers.maps.GetAllMaps(ControllerType.Joystick).GetEnumerator()) {
						while (enumerator2.MoveNext()) {
							if (enumerator2.Current.categoryId == mapCategoryForAction.id) {
								flag = true;
								break;
							}
						}
					}
				}
				if (!flag) {
					return;
				}
			}
			BindButton bindButton = UnityEngine.Object.Instantiate<BindButton>(__instance.bindButtonPrefab);
			bindButton.transform.SetParent(__instance.bindButtonGrid);
			bindButton.navList = (List<UIButton>)Traverse.Create(__instance).Field("bindButtonList").GetValue();
			bindButton.playerNum = playerNum;
			bindButton.back = __instance.playerControlsButtons[playerNum];
			Traverse.Create(__instance).Field("bindButtonList").Method("Add", new object[] { bindButton });
			bindButton.inputAction = inputAction;
			bindButton.rewiredControllerID = controllerID;
		}
	}
}*/