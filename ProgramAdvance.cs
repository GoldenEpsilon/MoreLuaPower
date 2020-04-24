using HarmonyLib;
using I2.Loc;
using UnityEngine;

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("CastSpell")]
class MoreLuaPower_ProgramAdvance
{
    static bool Prefix(ref Player __instance, int slotNum, ref int manaOverride, bool consumeOverride)
    {
        if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary.ContainsKey("ProgramAdvance"))
        {
            string str = __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ProgramAdvanceLinkWith"];
            int otherSlotNum = slotNum == 0 ? 1 : 0;
            if (str != __instance.duelDisk.castSlots[otherSlotNum].spellObj.itemID)
            {
                return true;
            }
            else
            {
                //ADVANCE LINK
                bool consumeAfterAdvance1 = __instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["ConsumeAfterAdvance"] == "true";
                bool consumeAfterAdvance2 = __instance.duelDisk.castSlots[otherSlotNum].spellObj.spell.itemObj.paramDictionary["ConsumeAfterAdvance"] == "true";
                if (slotNum >= __instance.duelDisk.castSlots.Count)
                    return false;
                if ((UnityEngine.Object)__instance.lastSpellText != (UnityEngine.Object)null)
                    SimplePool.Despawn(__instance.lastSpellText.gameObject, 0.0f);
                if (__instance.duelDisk.shuffling)
                {
                    object[] newEmptyArray = { };
                    __instance.lastSpellText = __instance.CreateFloatText(__instance.ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Deck_is_shuffling, (object[])newEmptyArray), -20, 65, 0.5f, (Sprite)null);
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
                        SpellObject spellObj = S.I.deCtrl.CreateSpellBase(__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["AdvanceSpell"], __instance);
                        int num1 = Traverse.Create(__instance).Method("CalculateManaCost", spellObj).GetValue<int>();
                        if (__instance.duelDisk.castSlots[slotNum].spellObj.spell.itemObj.paramDictionary["CostAdvancedMana"] == "false")
                        {
                            num1 = 0;
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
                            if (consumeAfterAdvance1)
                            {
                                __instance.duelDisk.LaunchSlot(slotNum, true, (FloatingText)null);
                            }
                            else
                            {
                                __instance.duelDisk.LaunchSlot(slotNum, false, (FloatingText)null);
                            }
                            if (consumeAfterAdvance2)
                            {
                                __instance.duelDisk.LaunchSlot(otherSlotNum, true, (FloatingText)null);
                            }
                            else
                            {
                                __instance.duelDisk.LaunchSlot(otherSlotNum, false, (FloatingText)null);
                            }

                            if (spellObj.itemID == "Jam")
                                __instance.TriggerArtifacts(FTrigger.OnJamCast, (Being)null, 0);
                            __instance.theSpellCast = spellObj;
                            spellObj.PlayerCast();
                            __instance.spellsCastThisBattle.Add(spellObj);

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
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        return true;
    }
}
