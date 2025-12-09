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
    static void OnEffectActionFunctionCall(EffectActions __instance, string fn, ItemObject itemObj)
    {
        if (fn == "Move")
        {
            MPLog.Log("OnMoveEffect hook triggering", LogLevel.Info);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnMoveEffect"));
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
        if (fTrigger == (FTrigger)Enum.Parse(typeof(FTrigger), "OnAddStatus") && __instance.HasTrigger(fTrigger))
        {
            // Adapted from decompiled ItemObject.Trigger method (build ID 10865936)
            foreach (EffectApp effectApp in __instance.efApps)
            {
                if (fTrigger == effectApp.fTrigger)
                {
                    if (effectApp.checks != null)
                    {
                        foreach (Check check in effectApp.checks)
                        {
                            if (check.ToString() == "AddedStatus")
                            {
                                // enum int value of applied Status is passed forward in forwardedHitDamage param for OnAddStatus triggers
                                // The AddedStatus check passes iff the checkValue string corresponds to the name of a Status whose int value matches the passed int value 
                                if (Enum.IsDefined(typeof(Status), effectApp.checkValue))
                                {
                                    Status checkStatus = (Status)Enum.Parse(typeof(Status), effectApp.checkValue);
                                    if ((int)checkStatus != forwardedHitDamage)
                                    {
                                        return false;
                                    }
                                }
                                
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.SetType), new Type[] { typeof(TileType) })]
    static void OnTileCrack(Tile __instance, TileType newType)
    {
        if (!(newType == TileType.Cracked || (newType == TileType.Broken && __instance.occupation > 0)))
        {
            return;
        }

        if (__instance.type == TileType.Cracked || __instance.type == TileType.Broken)
        {
            return;
        }

        MPLog.Log("OnTileCrack hook triggering", LogLevel.Info);
        Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["lastTileCrackedGlobal"] = __instance;
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnTileCrack"), __instance.battleGrid);
    }
}
