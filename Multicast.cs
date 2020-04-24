using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using I2.Loc;
using UnityEngine;

//Multicast is a mechanic that allows you to make spells that dont discard after the first use!
//Spells with the DontDiscard param will be parsed otherwise, do a normal spell cast
//ShotsRemaining is a number that keeps track of how many casts you have left
//MaxShots is self explanitory
//Ex: <Params DontDiscard="true" ShotsRemaining="5" MaxShots="5"></Params>

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("CastSpell")]
class MoreLuaPower_Multicast
{
    static bool Prefix(ref Player __instance, int slotNum, ref int manaOverride, bool consumeOverride)
    {
        if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("DontDiscard") && __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["DontDiscard"] == "true")
        {
            if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining") && Int32.Parse(__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) <= 1)
            {
                __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["MaxShots"];
                manaOverride = 0;
                return true;
            }
            if (slotNum >= __instance.duelDisk.castSlots.Count)
                return false;
            if ((UnityEngine.Object)__instance.lastSpellText != (UnityEngine.Object)null)
                SimplePool.Despawn(__instance.lastSpellText.gameObject, 0.0f);
            if (__instance.duelDisk.shuffling)
            {
                object[] newEmptyArray = { };
                __instance.lastSpellText = __instance.CreateFloatText(__instance.ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Deck_is_shuffling, newEmptyArray), -20, 65, 0.5f, (Sprite)null);
                __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = (Int32.Parse(__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) - 1).ToString();
            }
            else
            {
                slotNum = Traverse.Create(__instance).Method("GetSlotNum", slotNum).GetValue<int>();
                if ((UnityEngine.Object)__instance.duelDisk.castSlots[slotNum].cardtridgeFill == (UnityEngine.Object)null)
                {
                    __instance.lastSpellText = __instance.CreateFloatText(__instance.ctrl.statusTextPrefab, ScriptLocalization.UI.NoMoreSpells, -20, 65, 0.5f, (Sprite)null);
                }
                else
                {
                    SpellObject spellObj = __instance.duelDisk.castSlots[slotNum].cardtridgeFill.spellObj;
                    int num1 = 0;
                    if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] == __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["MaxShots"])
                    {
                        num1 = Traverse.Create(__instance).Method("CalculateManaCost", spellObj).GetValue<int>();
                    }
                    if (manaOverride >= 0)
                        num1 = manaOverride;
                    if ((double)__instance.duelDisk.currentMana >= (double)num1)
                    {
                        spellObj.being = (Being)__instance;
                        spellObj.spell.being = (Being)__instance;
                        __instance.anim.SetTrigger("spellCast");
                        __instance.anim.ResetTrigger("toIdle");
                        spellObj.castSlotNum = slotNum;
                        __instance.audioSource.PlayOneShot(__instance.castSound);
                        __instance.manaBeforeSpellCast = __instance.duelDisk.currentMana;
                        __instance.duelDisk.currentMana -= (float)num1;
                        __instance.TriggerArtifacts(FTrigger.OnManaBelow, (Being)null, 0);
                        __instance.lastSpellText = __instance.CreateSpellText(spellObj, 0.5f);
                        __instance.TriggerArtifacts(FTrigger.OnSpellCast, (Being)null, 0);
                        __instance.TriggerAllArtifacts(FTrigger.OnPlayerSpellCast, (Being)null, 0);
                        if (spellObj.itemID == "Jam")
                            __instance.TriggerArtifacts(FTrigger.OnJamCast, (Being)null, 0);
                        __instance.theSpellCast = spellObj;
                        spellObj.PlayerCast();
                        __instance.spellsCastThisBattle.Add(spellObj);
                        if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining"))
                        {
                            __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = (Int32.Parse(__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) - 1).ToString();
                        }
                        if (spellObj.channel)
                            __instance.anim.SetTrigger("channel");
                        foreach (Cpu currentPet in __instance.currentPets)
                            currentPet.StartAction();
                        if (AchievementsCtrl.IsUnlocked("Disguised_Toast") || __instance.player.IsReference())
                            return false;
                        float amountVar = 0.0f;
                        if ((double)__instance.ctrl.GetAmount(new AmountApp(ref amountVar, "JamsCastThisBattle"), 0.0f, spellObj, (ArtifactObject)null, (PactObject)null, false) < 10.0)
                            return false;
                        AchievementsCtrl.UnlockAchievement("Disguised_Toast");
                    }
                    else
                    {
                        if ((double)__instance.duelDisk.currentMana >= (double)num1)
                            return false;
                        float max = (float)num1 - __instance.duelDisk.currentMana;
                        double num2 = (double)Mathf.Clamp(max, 0.1f, max);
                        if (num1 > __instance.maxMana)
                        {
                            __instance.lastSpellText = __instance.CreateFloatText(__instance.ctrl.statusTextPrefab, ScriptLocalization.UI.Max_Mana_too_low, -20, 65, 0.5f, (Sprite)null);
                            --__instance.duelDisk.currentMana;
                        }
                        else
                            __instance.lastSpellText = __instance.CreateFloatText(__instance.ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.NeedMoreMana + "({0})", (object)Mathf.Clamp(max, 0.1f, max).ToString("f1")), -20, 65, 0.5f, (Sprite)null);
                    }
                }
            }
            return false;
        }
        return true;
    }
}
