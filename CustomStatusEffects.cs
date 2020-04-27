using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;

/*  
 *  Type:   Transpiler
 *  What:   I am removing 
 *		        this.icon.sprite = being.ctrl.effectSpritesDict[text];
 *		        this.iconBackground.sprite = this.icon.sprite;
 *		    from StatusEffect.Set()
 *          (lines 31 and 32 of StatusEffect in DNSpy)
 *  Why:    Custom Status Effects do not work unless I remove this chunk of code, so that it does not overwrite what I set up in Prefix for Set()
 *  How:    I am checking the IL opcodes for ldloc_0 followed by callvirt, then removing all instructions until I have removed 4 callvirt s.
*/
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

[HarmonyPatch(typeof(StatusEffect))]
[HarmonyPatch("Set")]
class MoreLuaPower_CustomStatusEffects
{
    static void Postfix(Being being, Status statusType, StatusEffect __instance, Image ___icon, Image ___iconBackground) {
        string text = "";
        int parse = 0;
        if (int.TryParse(statusType.ToString(), out parse)) {
            text = LuaPowerData.statuses[parse];
        } else {
            text = statusType.ToString();
        }
        __instance.transform.name = "StatIcon - " + text;
        ___icon.sprite = being.ctrl.effectSpritesDict[text];
        ___iconBackground.sprite = ___icon.sprite;
    }
}

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