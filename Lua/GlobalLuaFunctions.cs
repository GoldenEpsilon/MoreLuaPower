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
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["MakeSprite"] = (Action<string, string, string>)LuaPowerSprites.MakeSprite;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetSprite"] = (Func<string, Sprite>)LuaPowerSprites.GetSprite;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["NewEffect"] = (Action<string, string>)LuaPowerStatus.NewEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddEffect"] = (Action<Being, string, float, float>)LuaPowerStatus.AddEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetEffect"] = (Func<Being, string, bool>)LuaPowerStatus.GetEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetEffectAmount"] = (Func<Being, string, float>)LuaPowerStatus.GetEffectAmount;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["RemoveEffect"] = (Action<Being, string>)LuaPowerStatus.RemoveEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["EffectExists"] = (Func<string, bool>)LuaPowerStatus.EffectExists;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["MakeBrand"] = (Func<string, string, Brand>)LuaPowerBrands.MakeBrand;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetBrand"] = (Func<Brand, string>)LuaPowerBrands.GetBrand;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["SetBrandImage"] = (Action<string, string, string>)LuaPowerBrands.SetBrandImage;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["ParticleEffect"] = (Action<Being, Dictionary<string, string>>)LuaPowerParticles.ParticleEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["PlayCustomSound"] = (Action<Being, string>)LuaPowerSound.PlaySound;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["PlayCustomMusic"] = (Action<string>)LuaPowerSound.PlayCustomMusic;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["SetVariable"] = (Action<Being, string, string>)LuaPowerBeingVariables.SetVariable;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetVariable"] = (Func<Being, string, string>)LuaPowerBeingVariables.GetVariable;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddHook"] = (Action<FTrigger, string>)LuaPowerHooks.AddHook;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddLangTerm"] = (Action<string, string, string>)LuaPowerLang.ImportTerm;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetCustomInput"] = (Func<KeyCode, bool>)PowerMonoBehavior.GetCustomInput;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["EnableDeveloperTools"] = (Func<bool>)PowerMonoBehavior.EnableDeveloperTools;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetPlayer"] = (Func<Player>)MoreLuaPower.GetPlayer;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["PrintDev"] = (Action<string>)PowerMonoBehavior.PrintDev;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["RunDev"] = (Action<string>)PowerMonoBehavior.RunDev;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddCustomMusic"] = (Action<string, float, float>)PowerMonoBehavior.AddCustomMusic;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddMusicHook"] = (Action<string, string, string>)PowerMonoBehavior.AddMusicHook;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddZoneIcon"] = (Action<string, string>)PowerMonoBehavior.AddZoneIcon;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddZoneType"] = (Action<string, string>)CustomZoneUtil.AddZoneType;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddWorldToManualGeneration"] = (Action<string>)CustomZoneUtil.AddWorldToManualGeneration;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["TriggerZoneEvent"] = (Action<string>)CustomZoneUtil.TriggerZoneEvent;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetZoneType"] = (Func<string, ZoneType>)CustomZoneUtil.GetZoneType;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddUpgrade"] = (Action<string, string, string, string, string>)LuaPowerUpgrades.AddUpgrade;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddXMLToSpell"] = (Action<SpellObject, string>)LuaPowerUpgrades.AddXMLToSpell;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["MakeZoneGenocideLenient"] = (Action<string>)PowerMonoBehavior.MakeZoneGenocideLenient;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddWorldToCustomCampaign"] = (Action<string, string, bool>)CustomZoneUtil.AddWorldToCustomCampaign;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddCharacterToCustomCampaign"] = (Action<string, string>)CustomZoneUtil.AddCharacterToCustomCampaign;
    }
}

[HarmonyPatch(typeof(EffectActions))]
[HarmonyPatch("AddScript")]
class MoreLuaPower_LuaFunctions
{

    static List<string> loadedScripts = new List<string>();

    static void Postfix(Script ___myLuaScript, string scriptPath) {
        if (loadedScripts.Contains(scriptPath)) return;
        loadedScripts.Add(scriptPath);
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