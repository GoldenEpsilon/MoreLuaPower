using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(EffectActions), MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(string) })]
class MoreLuaPower_GlobalLuaFunctions
{
    static void Postfix() {
        Table GLOBAL_LUA = Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals;

	// Visual & Media Functions
	GLOBAL_LUA["MakeSprite"] = (Action<string, string, string>)LuaPowerSprites.MakeSprite;
	GLOBAL_LUA["GetSprite"] = (Func<string, Sprite>)LuaPowerSprites.GetSprite;
	GLOBAL_LUA["AddCustomMusic"] = (Action<string, float, float, float, float>)PowerMonoBehavior.AddCustomMusic;
	GLOBAL_LUA["AddMusicHook"] = (Action<string, string, string>)PowerMonoBehavior.AddMusicHook;
	GLOBAL_LUA["PlayCustomSound"] = (Action<Being, string>)LuaPowerSound.PlaySound;
	GLOBAL_LUA["PlayCustomMusic"] = (Action<string>)LuaPowerSound.PlayCustomMusic;
	GLOBAL_LUA["PlayCustomMusicIntroLoop"] = (Action<string, float, float>)LuaPowerSound.PlayCustomMusicIntroLoop;
	GLOBAL_LUA["AudioExists"] = (Func<string, bool>)PowerMonoBehavior.AudioExists;
	GLOBAL_LUA["LoadCutscene"] = (Action<string, string, string>)LuaPowerCutscenes.LoadCutscene;
	GLOBAL_LUA["PlayCutscene"] = (Action<string, bool, bool, bool, int>)LuaPowerCutscenes.PlayCutscene;
	GLOBAL_LUA["PlayCutsceneURL"] = (Action<string, bool, bool, bool, int>)LuaPowerCutscenes.PlayCutsceneURL;
	GLOBAL_LUA["ParticleEffect"] = (Action<Being, Dictionary<string, string>>)LuaPowerParticles.ParticleEffect;

	// Effect Functions
	GLOBAL_LUA["NewEffect"] = (Action<string, string>)LuaPowerStatus.NewEffect;
	GLOBAL_LUA["GetStatus"] = (Func<string, Status>)LuaPowerMisc.GetStatus;
	GLOBAL_LUA["GetFTrigger"] = (Func<string, FTrigger>)LuaPowerMisc.GetFTrigger;
	GLOBAL_LUA["AddEffect"] = (Action<Being, string, float, float>)LuaPowerStatus.AddEffect;
	GLOBAL_LUA["GetEffect"] = (Func<Being, string, bool>)LuaPowerStatus.GetEffect;
	GLOBAL_LUA["GetEffectAmount"] = (Func<Being, string, float>)LuaPowerStatus.GetEffectAmount;
	GLOBAL_LUA["RemoveEffect"] = (Action<Being, string>)LuaPowerStatus.RemoveEffect;
	GLOBAL_LUA["EffectExists"] = (Func<string, bool>)LuaPowerStatus.EffectExists;
	GLOBAL_LUA["AddTriggerTooltip"] = (Action<string, string, string>)LuaPowerCustomTooltips.AddTriggerTooltip;
	GLOBAL_LUA["AddEffectTooltip"] = (Action<string, string, string>)LuaPowerCustomTooltips.AddEffectTooltip;

	// Brand Functions
	GLOBAL_LUA["MakeBrand"] = (Func<string, string, Brand>)LuaPowerBrands.MakeBrand;
	GLOBAL_LUA["SetBrandImage"] = (Action<string, string, string>)LuaPowerBrands.SetBrandImage;
	GLOBAL_LUA["GetBrand"] = (Func<string, Brand>)LuaPowerMisc.GetBrand;

	// ItemObject Functions
	GLOBAL_LUA["AddXMLToSpell"] = (Action<SpellObject, string>)LuaPowerUpgrades.AddXMLToSpell;
	GLOBAL_LUA["AddUpgrade"] = (Action<string, string, string, string, string>)LuaPowerUpgrades.AddUpgrade;
	GLOBAL_LUA["ApplyRandomUpgrade"] = (Action<SpellObject, List<Enhancement>>)LuaPowerUpgrades.ApplyRandomUpgrade;
	GLOBAL_LUA["GetUpgrade"] = (Func<string, Enhancement>)LuaPowerMisc.GetUpgrade;
	GLOBAL_LUA["CreateDrop"] = (Action<List<ItemObject>>)LuaPowerCustomDrops.CreateDrop;
	GLOBAL_LUA["AddDropCheck"] = (Action<string, string>)LuaPowerDropChecks.AddDropCheck;
	GLOBAL_LUA["ChangeCardBackground"] = (Action<string, Sprite>)LuaPowerCardAesthetics_Database.AddItemBG;
	GLOBAL_LUA["ChangeCardBorder"] = (Action<string, Sprite>)LuaPowerCardAesthetics_Database.AddItemBorder;
	GLOBAL_LUA["ChangeCardTextColor"] = (Action<string, List<float>>)LuaPowerCardAesthetics_Database.AddItemText;
	    
	// Zone Functions
	GLOBAL_LUA["AddZoneIcon"] = (Action<string, string>)PowerMonoBehavior.AddZoneIcon;
	GLOBAL_LUA["MakeZoneGenocideLenient"] = (Action<string>)PowerMonoBehavior.MakeZoneGenocideLenient;
	GLOBAL_LUA["GetBossTier"] = (Func<Being, int>)DogeBossData.GetBossTier;
	GLOBAL_LUA["AddWorldToCustomCampaign"] = (Action<string, string, bool>)CustomZoneUtil.AddWorldToCustomCampaign;
	GLOBAL_LUA["AddCharacterToCustomCampaign"] = (Action<string, string>)CustomZoneUtil.AddCharacterToCustomCampaign;
	GLOBAL_LUA["AddZoneType"] = (Action<string, string>)CustomZoneUtil.AddZoneType;
	GLOBAL_LUA["GetZoneType"] = (Func<string, ZoneType>)CustomZoneUtil.GetZoneType;
	GLOBAL_LUA["AddWorldToManualGeneration"] = (Action<string>)CustomZoneUtil.AddWorldToManualGeneration;
	GLOBAL_LUA["TriggerZoneEvent"] = (Action<string>)CustomZoneUtil.TriggerZoneEvent;
	GLOBAL_LUA["AddEdenGossip"] = (Action<string, string, string, string>)EdenGossip_AdditiveLines.Gossip_Data.AddEdenGossip;
	GLOBAL_LUA["RemoveEdenGossip"] = (Action<string, string, string, string>)EdenGossip_AdditiveLines.Gossip_Data.RemoveEdenGossip;
	GLOBAL_LUA["GetEdenGossip"] = (Func<string, string, string, List<string>>)EdenGossip_AdditiveLines.Gossip_Data.GetEdenGossip;

	// Player Functions
	GLOBAL_LUA["GetPlayer"] = (Func<Player>)MoreLuaPower.GetPlayer;
	GLOBAL_LUA["CharSetupTaunt"] = (Action<string>)TauntReviverList.AddCharacter;
    	GLOBAL_LUA["CharSetupPet"] = (Action<string>)PetReviverList.AddCharacter;
	GLOBAL_LUA["GetCustomInput"] = (Func<KeyCode, bool>)PowerMonoBehavior.GetCustomInput;
	GLOBAL_LUA["OverrideAnimator"] = (Action<Being, string>)UtilityFunctions.OverrideAnimator;

	// Custom Bar functions
	GLOBAL_LUA["AddBar"] = (Action<string, string, Being, float, float, int, float, float, float, Sprite, bool, bool>)LuaPowerBars.AddBar;
	GLOBAL_LUA["UpdateBar"] = (Action<string, float, float>)LuaPowerBars.UpdateBar;
	GLOBAL_LUA["ChangeBarAttributes"] = (Action<string, bool, bool, int>)LuaPowerBars.ChangeBarAttributes;
	GLOBAL_LUA["ChangeBarColor"] = (Action<string, float, float, float>)LuaPowerBars.ChangeBarColor;
	GLOBAL_LUA["ChangeBarSprite"] = (Action<string, Sprite>)LuaPowerBars.ChangeBarSprite;
	GLOBAL_LUA["ShowBar"] = (Action<string>)LuaPowerBars.ShowBar;
	GLOBAL_LUA["HideBar"] = (Action<string>)LuaPowerBars.HideBar;
	GLOBAL_LUA["RemoveBar"] = (Action<string>)LuaPowerBars.RemoveBar;
	GLOBAL_LUA["GetBar"] = (Func<string, FillBar>)LuaPowerBars.GetBar;

	// Utility
	GLOBAL_LUA["AddHook"] = (Action<FTrigger, string, Being>)LuaPowerHooks.AddHook;
	GLOBAL_LUA["SetCheatcode"] = (Action<string, string, string>)CustomCheatcodesList.SetCheatcode;
	GLOBAL_LUA["AddLangTerm"] = (Action<string, string, string>)LuaPowerLang.ImportTerm;
	GLOBAL_LUA["SetVariable"] = (Action<Being, string, string>)LuaPowerBeingVariables.SetVariable;
	GLOBAL_LUA["GetVariable"] = (Func<Being, string, string>)LuaPowerBeingVariables.GetVariable;

	// Custom Setting Functions
	GLOBAL_LUA["GetSettingToggle"] = (Func<string, bool>)MPLCustomSettings.GetSettingToggle;
	GLOBAL_LUA["GetSettingRotation"] = (Func<string, string>)MPLCustomSettings.GetSettingRotation;
	GLOBAL_LUA["GetSettingSlider"] = (Func<string, float>)MPLCustomSettings.GetSettingSlider;
	GLOBAL_LUA["AddSettingToggle"] = (Action<string, bool>)MPLCustomSettings.AddSettingToggle;
	GLOBAL_LUA["AddSettingRotation"] = (Action<string, List<string>, int>)MPLCustomSettings.AddSettingRotation;
	GLOBAL_LUA["AddSettingSlider"] = (Action<string, float>)MPLCustomSettings.AddSettingSlider;
	GLOBAL_LUA["AddSettingFolder"] = (Action<string>)MPLCustomSettings.AddSettingFolder;
	GLOBAL_LUA["GetSettingFolder"] = (Func<string, List<string>>)MPLCustomSettings.GetSettingFolder;
	GLOBAL_LUA["AddSettingTextBox"] = (Action<string, string>)MPLCustomSettings.AddSettingTextBox;
	GLOBAL_LUA["GetSettingTextBox"] = (Func<string, string>)MPLCustomSettings.GetSettingTextBox;
	GLOBAL_LUA["AddSettingButton"] = (Action<string, List<string>>)MPLCustomSettings.AddSettingButton;
	GLOBAL_LUA["EditSettingButton"] = (Action<string, List<string>>)MPLCustomSettings.EditSettingButton;

	// Achievements
	GLOBAL_LUA["AddCustomAchievement"] = (Action<string, string, string, bool>)LuaPowerAchievements.APIV.AddCustomAchievement;
	GLOBAL_LUA["UnlockCustomAchievement"] = (Action<string, int>)LuaPowerAchievements.APIV.UnlockCustomAchievement;

	// Dev Functions
	GLOBAL_LUA["Log"] = (Action<string, LogLevel>)MPLog.Log;
	GLOBAL_LUA["ChangeFileLogLevel"] = (Action<LogLevel>)MPLog.ChangeFileLogLevel;
	GLOBAL_LUA["ChangeConsoleLogLevel"] = (Action<LogLevel>)MPLog.ChangeConsoleLogLevel;
	GLOBAL_LUA["EnableDeveloperTools"] = (Func<bool>)PowerMonoBehavior.EnableDeveloperTools;
	GLOBAL_LUA["PrintDev"] = (Action<string>)PowerMonoBehavior.PrintDev;
	GLOBAL_LUA["RunDev"] = (Action<string>)PowerMonoBehavior.RunDev;
    }
}

[HarmonyPatch(typeof(EffectActions))]
[HarmonyPatch("AddScript")]
class MoreLuaPower_LuaFunctions
{

    static List<string> loadedScripts = new List<string>();

    static void Postfix(Script ___myLuaScript, string scriptPath) {
        object obj;
        obj = ___myLuaScript.Globals["Init"];
        if (obj != null && scriptPath != null) {
            if (!LuaPowerData.luaFunctionLoaded.Contains(scriptPath)) {
                S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(___myLuaScript.CreateCoroutine(obj)));
                LuaPowerData.luaFunctionLoaded.Add(scriptPath);
            }
            ___myLuaScript.Globals.Remove("Init");
        }
        obj = ___myLuaScript.Globals["Awake"];
        if (obj != null) {
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(___myLuaScript.CreateCoroutine(obj)));
            ___myLuaScript.Globals.Remove("Awake");
        }
        if (loadedScripts.Contains(scriptPath)) return;
        loadedScripts.Add(scriptPath);
        obj = ___myLuaScript.Globals["Update"];
        if (obj != null) {
            bool unique = true;
            foreach (object o in PowerMonoBehavior.UpdateScripts) {
                if (DynValue.FromObject(___myLuaScript, o).Function.EntryPointByteCodeLocation == DynValue.FromObject(___myLuaScript, obj).Function.EntryPointByteCodeLocation) {
                    unique = false;
                }
            }
            if (unique) {
                PowerMonoBehavior.UpdateScripts.Add(obj);
                PowerMonoBehavior.UpdateBaseScripts.Add(___myLuaScript);
            }
            ___myLuaScript.Globals.Remove("Update");
        }
        obj = ___myLuaScript.Globals["GameUpdate"];
        if (obj != null) {
            bool unique = true;
            foreach (object o in PowerMonoBehavior.GameUpdateScripts) {
                if (DynValue.FromObject(___myLuaScript, o).Function.EntryPointByteCodeLocation == DynValue.FromObject(___myLuaScript, obj).Function.EntryPointByteCodeLocation) {
                    unique = false;
                }
            }
            if (unique) {
                PowerMonoBehavior.GameUpdateScripts.Add(obj);
                PowerMonoBehavior.GameUpdateBaseScripts.Add(___myLuaScript);
            }
            ___myLuaScript.Globals.Remove("GameUpdate");
        }
        obj = ___myLuaScript.Globals["WorldInit"];
        if (obj != null)
        {
            bool unique = true;
            foreach (object o in PowerMonoBehavior.GameUpdateScripts)
            {
                if (DynValue.FromObject(___myLuaScript, o).Function.EntryPointByteCodeLocation == DynValue.FromObject(___myLuaScript, obj).Function.EntryPointByteCodeLocation)
                {
                    unique = false;
                }
            }
            if (unique)
            {
                CustomWorldGenerator.WorldInitScripts.Add(obj);
                CustomWorldGenerator.WorldInitBaseScripts.Add(___myLuaScript);
            }
            ___myLuaScript.Globals.Remove("WorldInit");
        }
        obj = ___myLuaScript.Globals["WorldPost"];
        if (obj != null)
        {
            bool unique = true;
            foreach (object o in PowerMonoBehavior.GameUpdateScripts)
            {
                if (DynValue.FromObject(___myLuaScript, o).Function.EntryPointByteCodeLocation == DynValue.FromObject(___myLuaScript, obj).Function.EntryPointByteCodeLocation)
                {
                    unique = false;
                }
            }
            if (unique)
            {
                CustomWorldGenerator.WorldPostScripts.Add(obj);
                CustomWorldGenerator.WorldPostBaseScripts.Add(___myLuaScript);
            }
            ___myLuaScript.Globals.Remove("WorldPost");
        }
        obj = ___myLuaScript.Globals["ZoneEvent"];
        if (obj != null)
        {
            bool unique = true;
            foreach (object o in PowerMonoBehavior.GameUpdateScripts)
            {
                if (DynValue.FromObject(___myLuaScript, o).Function.EntryPointByteCodeLocation == DynValue.FromObject(___myLuaScript, obj).Function.EntryPointByteCodeLocation)
                {
                    unique = false;
                }
            }
            if (unique)
            {
                CustomZoneUtil.ZoneEventScripts.Add(obj);
                CustomZoneUtil.ZoneEventBaseScripts.Add(___myLuaScript);
            }
            ___myLuaScript.Globals.Remove("ZoneEvent");
        }
        ___myLuaScript.Globals.Remove("PATH");
    }
}

class MoreLuaPower_FunctionHelper
{
    public static IEnumerator EffectRoutine(DynValue result) {
        foreach (DynValue thr in result.Coroutine.AsTypedEnumerable()) {
            yield return null;
        }
        yield break;
    }
}
