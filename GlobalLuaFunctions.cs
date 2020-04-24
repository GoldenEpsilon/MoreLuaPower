using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

[HarmonyPatch(typeof(EffectActions), MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(string) })]
class MoreLuaPower_GlobalLuaFunctions
{
    static void Postfix() {
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["MakeSprite"] = (Action<string, string, string>)LuaPowerSprites.MakeSprite;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["NewEffect"] = (Action<string, string>)LuaPowerStatus.NewEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddEffect"] = (Action<Being, string, float, float>)LuaPowerStatus.AddEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetEffect"] = (Func<Being, string, bool>)LuaPowerStatus.GetEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetEffectAmount"] = (Func<Being, string, float>)LuaPowerStatus.GetEffectAmount;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["RemoveEffect"] = (Action<Being, string>)LuaPowerStatus.RemoveEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["EffectExists"] = (Func<string, bool>)LuaPowerStatus.EffectExists;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["ParticleEffect"] = (Action<Being, Dictionary<string, string>>)LuaPowerParticles.ParticleEffect;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["PlaySound"] = (Action<Being, string>)LuaPowerSound.PlaySound;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["SetVariable"] = (Action<Being, string, string>)LuaPowerBeingVariables.SetVariable;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetVariable"] = (Func<Being, string, string>)LuaPowerBeingVariables.GetVariable;
    }
}