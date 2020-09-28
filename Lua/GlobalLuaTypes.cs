using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using UnityEngine;



[HarmonyPatch(typeof(EffectActions), MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(string) })]
class MoreLuaPower_GlobalLuaTypes
{
    static void Postfix() {
        UserData.RegisterType<Rewired.Player>(InteropAccessMode.Default, null);
        UserData.RegisterType<S>(InteropAccessMode.Default, null);
        UserData.RegisterType<Run>(InteropAccessMode.Default, null);
        UserData.RegisterType<SpellListCard>(InteropAccessMode.Default, null);
        UserData.RegisterType<ListCard>(InteropAccessMode.Default, null);
        UserData.RegisterType<StatusEffect>(InteropAccessMode.Default, null);
        UserData.RegisterType<HeroSelectCtrl>(InteropAccessMode.Default, null);
        UserData.RegisterType<AnimatorOverrideController>(InteropAccessMode.Default, null);
        UserData.RegisterType<Sprite>(InteropAccessMode.Default, null);
        UserData.RegisterType<Texture2D>(InteropAccessMode.Default, null);
        UserData.RegisterType<AnimationOverrider>(InteropAccessMode.Default, null);
        UserData.RegisterType<Check>(InteropAccessMode.Default, null);
        UserData.RegisterType<Shape>(InteropAccessMode.Default, null);
        UserData.RegisterType<Location>(InteropAccessMode.Default, null);
        UserData.RegisterType<ArcType>(InteropAccessMode.Default, null);
        UserData.RegisterType<KeyCode>(InteropAccessMode.Default, null);
    }
}