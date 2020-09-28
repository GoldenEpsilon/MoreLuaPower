using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;

//Program advance is a mechanic from MMBN where you combine multiple chips into one powerful chip.
//Spells with the ProgramAdvance param will be parsed otherwise, do a normal spell cast
// ProgramAdvanceLinkWith is the name of the spell in the other slot needed to trigger the Advance Spell - use commas to separate spells
// CostAdvancedMana is a boolean that determins if the Advance Spell drains mana
// ConsumeAfterAdvance is a boolean that consumes the chip after the Advance Spell
// AdvanceSpell is spell that ends up being casted
//EX: <Params ProgramAdvance="true" ProgramAdvanceLinkWith="Thunder" CostAdvancedMana="false" ConsumeAfterAdvance="true" AdvanceSpell="StormThunder"></Params>
//EX: <Params ProgramAdvance="true" ProgramAdvanceLinkWith="MiniThunder" CostAdvancedMana="false" ConsumeAfterAdvance="true" AdvanceSpell="StormThunder"></Params>
//EX: <Params ProgramAdvance="true" ProgramAdvanceLinkWith="Frostbolt,IceNeedle" CostAdvancedMana="true" ConsumeAfterAdvance="false" AdvanceSpell="Tundra"></Params>
//      NOTE: This example activates on EITHER Frostbolt or IceNeedle

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("CastSpell")]
class MoreLuaPower_ProgramAdvance
{
    static void Prefix(ref Player __instance, int slotNum, ref int manaOverride, ref bool consumeOverride, out Tuple<int, SpellObject, SpellObject> __state) {
        __state = new Tuple<int, SpellObject, SpellObject>(-1, null, null);
        int slot = slotNum;
        if (!__instance.duelDisk.shuffling && __instance.duelDisk.castSlots[slot].spellObj.spell != null) {
            Dictionary<string, string> pd = __instance.duelDisk.castSlots[slot].spellObj.spell.itemObj.paramDictionary;
            if (pd != null && __instance.duelDisk.castSlots[slot].cardtridgeFill != null && pd.ContainsKey("ProgramAdvance")) {
                if (!pd.ContainsKey("ProgramAdvanceLinkWith")) {
                    Debug.Log("ERROR: Spell has ProgramAdvance, but not ProgramAdvanceLinkWith");
                    return;
                }
                if (!pd.ContainsKey("AdvanceSpell")) {
                    Debug.Log("ERROR: Spell has ProgramAdvance, but not AdvanceSpell");
                    return;
                }
                List<string> str = pd["ProgramAdvanceLinkWith"].Split(',').ToList();
                int otherSlotNum = -1;
                for (int i = 0; i < __instance.duelDisk.castSlots.Count; i++) {
                    foreach (string i2 in str) {
                        if (__instance.duelDisk.castSlots[i] != null && __instance.duelDisk.castSlots[i].spellObj.spell != null && i2 == __instance.duelDisk.castSlots[i].spellObj.itemID) {
                            otherSlotNum = i;
                        }
                    }
                }
                if (otherSlotNum != -1 && slotNum < __instance.duelDisk.castSlots.Count) {
                    //ADVANCE LINK ACTIVATED

                    if (!consumeOverride && pd.ContainsKey("ConsumeAfterAdvance")) {
                        consumeOverride = pd["ConsumeAfterAdvance"] == "true";
                    }
                    if (__instance.duelDisk.castSlots[slot].spellObj.spell.itemObj.paramDictionary.ContainsKey("ConsumeAfterAdvance")) {
                        __instance.duelDisk.LaunchSlot(slot == 0 ? 1 : 0, __instance.duelDisk.castSlots[slot].spellObj.spell.itemObj.paramDictionary["ConsumeAfterAdvance"] == "true", null);
                    } else {
                        __instance.duelDisk.LaunchSlot(slot == 0 ? 1 : 0, false, null);
                    }

                    SpellObject Advance = S.I.deCtrl.CreateSpellBase(pd["AdvanceSpell"], __instance);
                    __state = new Tuple<int, SpellObject, SpellObject>(
                        __instance.duelDisk.currentCardtridges.IndexOf(__instance.duelDisk.castSlots[slotNum].cardtridgeFill),
                        __instance.duelDisk.castSlots[slotNum].cardtridgeFill.spellObj,
                        Advance);
                    __instance.duelDisk.currentCardtridges.ElementAt(__state.Item1).spellObj = Advance;
                    if (pd.ContainsKey("CostAdvancedMana") && pd["CostAdvancedMana"] == "false" && manaOverride < 0) {
                        manaOverride = 0;
                    }
                }
            }
        }
    }
    static void Postfix(ref Player __instance, Tuple<int, SpellObject, SpellObject> __state) {
        if (__state != null && __state.Item1 != -1 && __state.Item1 < __instance.duelDisk.currentCardtridges.Count
            && __instance.duelDisk.currentCardtridges.ElementAt(__state.Item1).spellObj == __state.Item3) {
            __instance.duelDisk.currentCardtridges.ElementAt(__state.Item1).spellObj = __state.Item2;
        }
    }
}
