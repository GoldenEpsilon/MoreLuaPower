using Discord;
using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;


[HarmonyPatch]
static class LuaPowerFTriggerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.SaveRun))]
    static void OnSave() {
        MPLog.Log("OnSave hook triggering", LogLevel.Info);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnSave"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateRunFromSave))]
    static void OnLoad() {
        MPLog.Log("OnLoad hook triggering", LogLevel.Info);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnLoad"));
        //DOES NOT WORK CORRECTLY (because it triggers before the artifact loads, I think - adding custom hooks should work)
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnChoose(ChoiceCard __instance) {
        if (__instance.itemObj.type == ItemType.Art) {
            MPLog.Log("OnChooseArtifact hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChooseArtifact"));
        }
        if (__instance.itemObj.type == ItemType.Pact) {
            MPLog.Log("OnChoosePact hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChoosePact"));
        }
        MPLog.Log("OnChooseThis hook triggering", LogLevel.Info);
        __instance.itemObj.Trigger((FTrigger)Enum.Parse(typeof(FTrigger), "OnChooseThis"));
        MPLog.Log("OnChoose hook triggering", LogLevel.Info);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChoose"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DeckCtrl), nameof(DeckCtrl.RemoveArtifactCard))]
    static void OnRemoveArtifact(ListCard cardToRemove) {
        if (cardToRemove.itemObj.type == ItemType.Art) {
            MPLog.Log("OnRemoveArtifact hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemoveArtifact"));
        }
        if (cardToRemove.itemObj.type == ItemType.Pact) {
            MPLog.Log("OnRemovePact hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemovePact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnUpgrade(ChoiceCard __instance) {
        if (__instance.rewardType == RewardType.Upgrade) {
            MPLog.Log("OnUpgrade hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnUpgrade"));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ListCard), nameof(ListCard.RemoveThisCard))]
    static void OnRemove(ListCard __instance, bool useRemover) {
        MPLog.Log("OnRemoveThis hook triggering", LogLevel.Info);
        __instance.itemObj.Trigger((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemoveThis"));
        MPLog.Log("OnRemove hook triggering", LogLevel.Info);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemove"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator), nameof(Animator.SetTrigger), new Type[] { typeof(string) })]
    static void OnAnimationTrigger(Animator __instance, string name) {
        if (name == "taunt") {
            foreach (var p in S.I.batCtrl.currentPlayers) {
                if (p.anim == __instance) {
                    MPLog.Log("OnTaunt hook triggering", LogLevel.Info);
                    S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnTaunt"));
                    break;
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EffectActions), nameof(EffectActions.CallFunctionWithItem), new Type[] { typeof(string), typeof(ItemObject) } )]
    static void OnEffectActionFunctionCall(EffectActions __instance, string fn, ItemObject itemObject)
    {
        if (fn == "Move")
        {
            MPLog.Log("OnForceMove hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnForceMove"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Being), nameof(Being.AddStatus), new Type[] { typeof(Status), typeof(float), typeof(float), typeof(ItemObject) })]
    static void OnAddStatus(Being __instance, Status statusType, float amount, float duration, ItemObject source)
    {
        MPLog.Log("OnAddStatus hook triggering", LogLevel.Info);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnAddStatus"), forwardedTargetHit: __instance, forwardedHitAmount: (int) statusType);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemObject), nameof(ItemObject.Trigger), new Type[] { typeof(FTrigger), typeof(bool), typeof(Being), typeof(int) })]
    static bool ItemObjectTriggerOnAddStatusHandler(ItemObject __instance, FTrigger fTrigger, bool doublecast, Being hitBeing, int forwardedHitDamage)
    {
        if (fTrigger != (FTrigger)Enum.Parse(typeof(FTrigger), "OnAddStatus"))
        {
            return true;
        }
        else if (!__instance.HasTrigger(fTrigger))
        {
            return true;
        }
        else
        {
            // Adapted from decompiled ItemObject.Trigger method (build ID 10865936)
            foreach (EffectApp effectApp in __instance.efApps)
            {
                if (fTrigger == effectApp.fTrigger)
                {
                    if (__instance.currentApp != null && __instance.currentApp.frameTriggered == Time.frameCount && __instance.currentApp.fTrigger == fTrigger)
                    {
                        break;
                    }

                    bool checkFailed = false;
                    if (effectApp.checks != null)
                    {
                        foreach (Check check in effectApp.checks)
                        {
                            if (check != Check.None)
                            {
                                switch (check)
                                {
                                    case Check.AmountOver:
                                        if (__instance.ctrl.GetAmount(effectApp.amountApp, effectApp.amount, __instance.spellObj, __instance.artObj, null, false) <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Fragile:
                                        if (!__instance.being.GetStatusEffect(Status.Fragile))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.NotFragile:
                                        if (__instance.being.GetStatusEffect(Status.Fragile))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.HasStatusFromThis:
                                        {
                                            Status status = (Status)Enum.Parse(typeof(Status), effectApp.checkValue);
                                            if (!__instance.being.GetStatusEffect(status))
                                            {
                                                checkFailed = true;
                                            }
                                            else
                                            {
                                                bool flag = false;
                                                using (List<StatusStack>.Enumerator enumerator3 = __instance.being.GetStatusEffect(status).statusStacks.GetEnumerator())
                                                {
                                                    while (enumerator3.MoveNext())
                                                    {
                                                        if (enumerator3.Current.source == __instance)
                                                        {
                                                            flag = true;
                                                        }
                                                    }
                                                }
                                                checkFailed = !flag;
                                            }
                                            break;
                                        }
                                    case Check.HitHPOver:
                                        if (hitBeing && (float)hitBeing.health.current <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.HitHPUnder:
                                        if (hitBeing && (float)hitBeing.health.current >= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Hostage:
                                        if (hitBeing && !hitBeing.beingObj.tags.Contains(Tag.Hostage))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.HPUnder:
                                        if (__instance.being && (float)__instance.being.health.current >= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.HPOver:
                                        if (__instance.being && (float)__instance.being.health.current <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.DamageOver:
                                        if (hitBeing && (float)forwardedHitDamage <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Flow:
                                        if (!__instance.ctrl.currentPlayer.GetStatusEffect(Status.Flow))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.NotFlow:
                                        if (__instance.ctrl.currentPlayer.GetStatusEffect(Status.Flow))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Enemy:
                                        if (hitBeing && hitBeing.alignNum != -1)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.EnemyOrHostage:
                                        if (hitBeing && hitBeing.alignNum != -1 && !hitBeing.beingObj.tags.Contains(Tag.Hostage))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.EnemyOrStructure:
                                        if (hitBeing && hitBeing.alignNum != -1 && hitBeing.alignNum != 0)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.ManaCostOver:
                                        if (__instance.being && __instance.being.player && __instance.being.player.spellToCast != null && __instance.being.player.spellToCast.mana <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.ManaCostUnder:
                                        if (__instance.being && __instance.being.player && __instance.being.player.spellToCast != null && __instance.being.player.spellToCast.mana >= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Neutral:
                                        if (hitBeing && hitBeing.alignNum != 0 && !hitBeing.beingObj.tags.Contains(Tag.Structure))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.Never:
                                        checkFailed = true;
                                        break;
                                    case Check.NoMinion:
                                        if (hitBeing && hitBeing.minion)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.NotDrone:
                                        if (hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Drone))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.NotWeaponOrDrone:
                                        if ((hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Weapon)) || (hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Drone)))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.PlayerWasHit:
                                        if (__instance.being.battleGrid.lastTargetHit.player == null)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.PlayerWasNotHit:
                                        if (__instance.being.battleGrid.lastTargetHit.player != null)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.ShieldUnder:
                                        if (__instance.being && (float)__instance.being.health.shield >= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.SpellID:
                                        if (hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.itemID != effectApp.checkValue)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.SpellToCastID:
                                        if (__instance.being && __instance.being.player && __instance.being.player.spellToCast != null && __instance.being.player.spellToCast.itemID != effectApp.checkValue)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.SpellToCastIsNotWeapon:
                                        if (__instance.being && __instance.being.player && __instance.being.player.spellToCast != null && __instance.being.player.spellToCast.type == ItemType.Wep)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.StatusOver:
                                        if (Enum.IsDefined(typeof(Status), effectApp.checkValue))
                                        {
                                            Status status2 = (Status)Enum.Parse(typeof(Status), effectApp.checkValue);
                                            if ((hitBeing && !hitBeing.GetStatusEffect(status2)) || (hitBeing && hitBeing.GetStatusEffect(status2) && hitBeing.GetStatusEffect(status2).amount <= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false)))
                                            {
                                                checkFailed = true;
                                            }
                                        }
                                        break;
                                    case Check.StatusUnder:
                                        if (Enum.IsDefined(typeof(Status), effectApp.checkValue))
                                        {
                                            Status status3 = (Status)Enum.Parse(typeof(Status), effectApp.checkValue);
                                            if (hitBeing && hitBeing.GetStatusEffect(status3) && hitBeing.GetStatusEffect(status3).amount >= __instance.ctrl.GetAmount(effectApp.checkAmountApp, effectApp.checkAmount, __instance.spellObj, __instance.artObj, null, false))
                                            {
                                                checkFailed = true;
                                            }
                                        }
                                        break;
                                    case Check.TouchedTileNotBroken:
                                        if (__instance.spellObj != null && __instance.spellObj.touchedTile.type == TileType.Broken)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.TrinityCast:
                                        if (__instance.spellObj != null && !__instance.spellObj.trinityCasted)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.NotTrinityCast:
                                        if (__instance.spellObj != null && __instance.spellObj.trinityCasted)
                                        {
                                            checkFailed = true;
                                        }
                                        break;
                                    case Check.LastSpell:
                                        {
                                            if (__instance.spellObj.being.player.duelDisk.queuedCardtridges.Count > 0)
                                            {
                                                checkFailed = true;
                                            }
                                            int num = 0;
                                            using (List<CastSlot>.Enumerator enumerator4 = __instance.spellObj.being.player.duelDisk.castSlots.GetEnumerator())
                                            {
                                                while (enumerator4.MoveNext())
                                                {
                                                    if (enumerator4.Current.cardtridgeFill)
                                                    {
                                                        num++;
                                                    }
                                                }
                                            }
                                            if (num > 1)
                                            {
                                                checkFailed = true;
                                            }
                                            break;
                                        }
                                    default:
                                        if (check == (Check)Enum.Parse(typeof(Check), "AddedStatus"))
                                        {
                                            // enum int value of applied Status is passed forward in forwardedHitDamage param for OnAddStatus triggers
                                            // The AddedStatus check passes iff the checkValue string corresponds to the name of a Status whose int value matches the passed int value 
                                            if (Enum.IsDefined(typeof(Status), effectApp.checkValue))
                                            {
                                                Status checkStatus = (Status)Enum.Parse(typeof(Status), effectApp.checkValue);
                                                if ((int) checkStatus == forwardedHitDamage)
                                                {
                                                    checkFailed = true;
                                                    break;
                                                }
                                            }
                                            checkFailed = false;
                                        }
                                        break;
                                }
                            }
                        }
                        if (checkFailed)
                        {
                            continue;
                        }
                    }
                    if (doublecast && effectApp.effect == Effect.DoubleCast)
                    {
                        break;
                    }
                    if (UnityEngine.Random.Range(0f, 1f) <= effectApp.chance)
                    {
                        __instance.currentApp = effectApp;
                        __instance.currentApp.frameTriggered = Time.frameCount;
                        EffectActions.CallFunctionWithItem(effectApp.effect.ToString(), __instance);
                        if (__instance.artObj != null && __instance.artObj.art.listCard != null && (effectApp.triggerCooldown == 0f || effectApp.triggerCooldown > 1f))
                        {
                            __instance.artObj.art.listCard.anim.SetTrigger("cast");
                        }
                        __instance.currentApp = null;
                    }
                }
            }
            return false;
        }
    }
}
