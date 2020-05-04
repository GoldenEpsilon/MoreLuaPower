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
    static void Prefix(ref Player __instance, int slotNum, ref int manaOverride, bool consumeOverride)
    {
        if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("DontDiscard") && 
            __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["DontDiscard"] == "true")
        {
            if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining")) {
                if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ManaCost")) {
                    switch (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ManaCost"]) {
                        case "Start":
                            if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("MaxShots") &&
                            __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] !=
                            __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["MaxShots"]) { 
                                manaOverride = 0; 
                            }
                            break;
                        case "End":
                            if (Int32.Parse(__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) > 1) {
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
        if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("DontDiscard") &&
            __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["DontDiscard"] == "true" &&
            forceConsume == false) {
            if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ShotsRemaining")){
                __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = (Int32.Parse(__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) - 1).ToString();
                if (Int32.Parse(__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"]) < 1) {
                    if (__instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("MaxShots")) {
                        __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ShotsRemaining"] = __instance.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["MaxShots"];
                    }
                    return true;
                }
                return false;
            }
            return false;
        }
        return true;
    }
}