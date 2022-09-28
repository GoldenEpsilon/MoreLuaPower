/*
 *  More Lua Power, started by Golden Epsilon
 *  Tons of stuff added by Sunreal, Shenanigans, Lea, and DecoyDoge
 *  Other Contributors: stephanreiken, tongo
 *  Workshop URL: https://steamcommunity.com/sharedfiles/filedetails/?id=2066319533
 *  GitHub Page: https://github.com/GoldenEpsilon/MoreLuaPower
 *
 *  Please do not include the DLL in your mods directly:
 *      Ask people to download the workshop version instead.
 *      
 *  That said, if there's something you want to modify from the code to make your own harmony mod, feel free!
 *  I am also open to help; If you have something you want to add in here, just let me know/add it in yourself! You will be credited.
*/
using HarmonyLib;
using System;
using UnityEngine;

[HarmonyPatch(typeof(S))]
[HarmonyPatch("Awake")]
class MoreLuaPower
{
	static void Prepare() {
		Debug.Log("MoreLuaPower Version 2.3.15");
		LuaPowerData.Setup();
		LuaPowerCustomEnumsSetup.Setup();
		LuaPowerCutscenes.Setup();
		//CustomZoneUtil.Setup();
		//CustomBosses.DataHandler.Setup();
		if (S.I.GetComponent<PowerMonoBehavior>() == null) {
			S.I.gameObject.AddComponent<PowerMonoBehavior>();
		}
		if (!LuaPowerData.customEnums[typeof(Effect)].Contains("Lua")) { LuaPowerData.customEnums[typeof(Effect)].Add("Lua"); }
		S.I.deCtrl.effectTooltips.Add((Effect)Enum.Parse(typeof(Effect), "Lua"));
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnSave")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnSave"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnLoad")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnLoad"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnChooseArtifact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnChooseArtifact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemoveArtifact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemoveArtifact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnChoosePact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnChoosePact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemovePact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemovePact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnUpgrade")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnUpgrade"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemove")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemove"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("PreMove")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("PreMove"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("PreHit")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("PreHit"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnOwnedPetDeath")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnOwnedPetDeath"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnPetDeath")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnPetDeath"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnLoop")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnLoop"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnTaunt")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnTaunt"); }
	}
	static public Player GetPlayer() {
		return S.I.batCtrl.currentPlayer;
	}
}

[HarmonyPatch(typeof(HarmonyMain))]
[HarmonyPatch("LoadMod")]
class MoreLuaPowerOnlyOne
{
	static bool Prefix(string modName) {
		if (Harmony.HasAnyPatches("com."+modName+".patch")) {
			MPLog.Log(modName + " is not being reinstalled", LogLevel.Info);
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(ModCtrl))]
[HarmonyPatch("InstallMods")]
class MoreLuaPowerReset
{
	static void Prefix() {
		LuaPowerData.GenocideLenientStages.Clear();
		LuaPowerData.customEnums.Clear();
		LuaPowerData.enumAdditions.Clear();
		LuaPowerData.customMusic.Clear(); ;
		LuaPowerData.sprites.Clear();
		LuaPowerData.materials.Clear();
		LuaPowerData.scripts.Clear();
		LuaPowerData.luaHooks.Clear();
		LuaPowerData.customUpgrades.Clear();
		LuaPowerData.baseGameEnumAmount.Clear();
		LuaPowerData.dropChecks.Clear();
		LuaPowerData.luaFunctionLoaded.Clear();
		LuaPowerData.videos.Clear();
		LuaPowerData.Setup();
		LuaPowerCustomEnumsSetup.Setup();
		LuaPowerCutscenes.Setup();
		if (S.I.GetComponent<PowerMonoBehavior>() == null) {
			S.I.gameObject.AddComponent<PowerMonoBehavior>();
		}
		if (!LuaPowerData.customEnums[typeof(Effect)].Contains("Lua")) { LuaPowerData.customEnums[typeof(Effect)].Add("Lua"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnSave")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnSave"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnLoad")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnLoad"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnChooseArtifact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnChooseArtifact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemoveArtifact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemoveArtifact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnChoosePact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnChoosePact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemovePact")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemovePact"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnUpgrade")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnUpgrade"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnRemove")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnRemove"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("PreMove")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("PreMove"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("PreHit")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("PreHit"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnOwnedPetDeath")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnOwnedPetDeath"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnPetDeath")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnPetDeath"); }
		if (!LuaPowerData.customEnums[typeof(FTrigger)].Contains("OnLoop")) { LuaPowerData.customEnums[typeof(FTrigger)].Add("OnLoop"); }
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
