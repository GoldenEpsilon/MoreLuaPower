using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;

class LuaPowerStatus
{
    static public void Setup() {
        LuaPowerData.statuses = new List<string>();
        foreach (string i in Status.GetNames(typeof(Status))) {
            LuaPowerData.statuses.Add(i);
        }
    }
    static public void NewEffect(string effect, string sprite) {
        if (LuaPowerData.sprites.ContainsKey(sprite)) {
            if (!LuaPowerData.statuses.Contains(effect)) { LuaPowerData.statuses.Add(effect); }
            if (!S.I.batCtrl.effectSpritesDict.ContainsKey(effect)) { S.I.batCtrl.effectSpritesDict.Add(effect, LuaPowerData.sprites[sprite]); }
        }
    }
    static public bool EffectExists(string effect) {
        return LuaPowerData.statuses.Contains(effect);
    }
    static public void AddEffect(Being being, string effect, float duration = 0, float amount = 0) {
        if (!LuaPowerData.statuses.Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return;
        }
        Status eff = (Status)LuaPowerData.statuses.FindIndex(new Predicate<string>((string str) => str == effect));
        being.AddStatus(eff, amount, duration);
    }
    static public bool GetEffect(Being being, string effect) {
        if (!LuaPowerData.statuses.Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return false;
        }
        Status eff = (Status)LuaPowerData.statuses.FindIndex(new Predicate<string>((string str) => str == effect));
        return being.GetStatusEffect(eff);
    }
    static public float GetEffectAmount(Being being, string effect) {
        if (!LuaPowerData.statuses.Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return 0;
        }
        Status eff = (Status)LuaPowerData.statuses.FindIndex(new Predicate<string>((string str) => str == effect));
        if (being.GetStatusEffect(eff)) {
            return being.GetStatusEffect(eff).amount;
        }
        return 0;
    }
    static public void RemoveEffect(Being being, string effect) {
        if (!LuaPowerData.statuses.Contains(effect)) {
            Debug.Log("NewEffect was not called for effect " + effect);
            return;
        }
        Status eff = (Status)LuaPowerData.statuses.FindIndex(new Predicate<string>((string str) => str == effect));
        being.RemoveStatus(eff);
    }
}