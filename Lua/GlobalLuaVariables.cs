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

[HarmonyPatch(typeof(EffectActions), MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(string) })]
class MoreLuaPower_GlobalLuaVariables
{
    static void Postfix() {
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["TimeScale"] = Time.timeScale;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["DeltaTime"] = Time.deltaTime;
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["Check"] = UserData.CreateStatic<Check>();
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["Shape"] = UserData.CreateStatic<Shape>();
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["Location"] = UserData.CreateStatic<Location>();
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["ArcType"] = UserData.CreateStatic<ArcType>();
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GunPointSetting"] = UserData.CreateStatic<GunPointSetting>();
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["Time"] = UserData.CreateStatic<Time>(); //I NEED TO CHECK TO SEE IF THIS WORKS
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["S"] = UserData.CreateStatic<S>();//I NEED TO TEST THIS
    }
}

[HarmonyPatch(typeof(EffectActions))]
[HarmonyPatch("AddScript")]
class MoreLuaPower_PATHVariable
{
    static void Prefix(string scriptPath, Script ___myLuaScript) {
        ___myLuaScript.Globals["PATH"] = Path.GetDirectoryName(scriptPath);
        LuaPowerData.scripts.Add(___myLuaScript);
    }
}