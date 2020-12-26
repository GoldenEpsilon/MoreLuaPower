using HarmonyLib;
using System;
using UnityEngine;


class LuaPowerStatus
{
    static public void NewEffect(string effect, string sprite = "") {
        if (!LuaPowerData.customEnums[typeof(Status)].Contains(effect)) { LuaPowerData.customEnums[typeof(Status)].Add(effect); }
        if (LuaPowerData.sprites.ContainsKey(sprite)) {
            if (!S.I.batCtrl.effectSpritesDict.ContainsKey(effect)) { S.I.batCtrl.effectSpritesDict.Add(effect, LuaPowerData.sprites[sprite]); }
        }
    }
    static public bool EffectExists(string effect) {
        return LuaPowerData.customEnums[typeof(Status)].Contains(effect);
    }
    static public void AddEffect(Being being, string effect, float duration = 0, float amount = 0) {
        if (!LuaPowerData.customEnums[typeof(Status)].Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return;
        }
        if (GetEffect(being, effect)) { RemoveEffect(being, effect); }
        if (duration == 0) {
            duration = 9999f;
        }
        if (duration < 0) {
            duration = 0;
        }
        Status eff = (Status)LuaPowerData.customEnums[typeof(Status)].FindIndex(new Predicate<string>((string str) => str == effect));
        being.AddStatus(eff, amount, duration);
    }
    static public bool GetEffect(Being being, string effect) {
        if (!LuaPowerData.customEnums[typeof(Status)].Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return false;
        }
        Status eff = (Status)LuaPowerData.customEnums[typeof(Status)].FindIndex(new Predicate<string>((string str) => str == effect));
        return being.GetStatusEffect(eff);
    }
    static public float GetEffectAmount(Being being, string effect) {
        if (!LuaPowerData.customEnums[typeof(Status)].Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return 0;
        }
        Status eff = (Status)LuaPowerData.customEnums[typeof(Status)].FindIndex(new Predicate<string>((string str) => str == effect));
        if (being.GetStatusEffect(eff)) {
            return being.GetStatusEffect(eff).amount;
        }
        return 0;
    }
    static public void RemoveEffect(Being being, string effect) {
        if (!LuaPowerData.customEnums[typeof(Status)].Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return;
        }
        Status eff = (Status)LuaPowerData.customEnums[typeof(Status)].FindIndex(new Predicate<string>((string str) => str == effect));
        being.RemoveStatus(eff);
    }
}

//Makes certain checks work when they normally wouldn't
[HarmonyPatch]
static class LuaPowerStatusPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemObject), nameof(ItemObject.Trigger))]
    public static bool TriggerPrefix(ItemObject __instance, ref Being hitBeing)
    {
        if (hitBeing == null) hitBeing = __instance.being;
        return true;
    }
}