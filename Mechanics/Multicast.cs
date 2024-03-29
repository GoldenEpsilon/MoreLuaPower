﻿using System;

using HarmonyLib;
using UnityEngine;

//Multicast is a mechanic that allows you to make spells that dont discard after the first use!
//Spells with the DontDiscard param will be parsed otherwise, do a normal spell cast
//ShotsRemaining is a number that keeps track of how many casts you have left
//MaxShots is self-explanatory
//ManaCost is either "Start", "End", or "All", Defaulting to "All".
//  If it is set to "Start" the mana cost will only be used the first cast,
//  if it is set to "End" the mana cost will only be used the last cast,
//  and if it is set to "All" the mana cost will be used for all casts.
//Note: ManaCost does not affect anything if you don't have ShotsRemaining
//Ex: <Params DontDiscard="true" ShotsRemaining="5" MaxShots="5" ManaCost="Start"></Params>

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("CastSpell")]
class MoreLuaPower_Multicast
{
    [HarmonyPriority(Priority.LowerThanNormal)]
    static void Prefix(ref Player __instance, int slotNum, ref int manaOverride, bool consumeOverride) {
        if (slotNum >= __instance.duelDisk.castSlots.Count) {
            return;
        }
        slotNum = Traverse.Create(__instance).Method("GetSlotNum", slotNum).GetValue<int>();
        if (__instance.duelDisk.castSlots[slotNum].cardtridgeFill == null) {
            return;
        }
        SpellObject spellToCast = __instance.duelDisk.castSlots[slotNum].cardtridgeFill.spellObj;
        if (spellToCast.spell.itemObj.paramDictionary.ContainsKey("DontDiscard") &&
        spellToCast.spell.itemObj.paramDictionary["DontDiscard"] == "true")
        {
            if (spellToCast.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining"))
            {
                if (spellToCast.spell.itemObj.paramDictionary.ContainsKey("ManaCost"))
                {
                    switch (spellToCast.spell.itemObj.paramDictionary["ManaCost"])
                    {
                        case "Start":
                            if (spellToCast.spell.itemObj.paramDictionary.ContainsKey("MaxShots") &&
                            spellToCast.spell.itemObj.paramDictionary["ShotsRemaining"] !=
                            spellToCast.spell.itemObj.paramDictionary["MaxShots"])
                            {
                                manaOverride = 0;
                            }
                            break;
                        case "End":
                            if (Int32.Parse(spellToCast.spell.itemObj.paramDictionary["ShotsRemaining"]) > 1)
                            {
                                manaOverride = 0;
                            }
                            break;
                        case "All":
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(DuelDisk))]
[HarmonyPatch("LaunchSlot")]
class MoreLuaPower_Multicast2
{
    static bool Prefix(DuelDisk __instance, int slotNum, bool forceConsume) {
        if (__instance.castSlots[slotNum] != null && __instance.castSlots[slotNum].spellObj != null && __instance.castSlots[slotNum].spellObj.spell != null && __instance.castSlots[slotNum].spellObj.spell.itemObj != null) {
            if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("DontDiscard") &&
            __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["DontDiscard"] == "true" &&
            forceConsume == false) {
                if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining")) {
                    __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = (Int32.Parse(__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) - 1).ToString();
                    if (Int32.Parse(__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) < 1) {
                        //if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("MaxShots")) {
                        //    __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["MaxShots"];
                        //}
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(CastSlot))]
[HarmonyPatch("Load")]
class MoreLuaPower_Multicast3 
{
    static bool Prefix(Cardtridge cardtridge) {
        if(cardtridge.spellObj.spell.itemObj.paramDictionary.ContainsKey("MaxShots")) {
            cardtridge.spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = cardtridge.spellObj.spell.itemObj.paramDictionary["MaxShots"];
        }
        return true;
    }
}