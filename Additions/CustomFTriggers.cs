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
        MPLog.Log("OnSave hook triggering", LogLevel.Minor);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnSave"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateRunFromSave))]
    static void OnLoad() {
        MPLog.Log("OnLoad hook triggering", LogLevel.Minor);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnLoad"));
        //DOES NOT WORK CORRECTLY (because it triggers before the artifact loads, I think - adding custom hooks should work)
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnChoose(ChoiceCard __instance) {
        if (__instance.itemObj.type == ItemType.Art) {
            MPLog.Log("OnChooseArtifact hook triggering", LogLevel.Minor);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChooseArtifact"));
        }
        if (__instance.itemObj.type == ItemType.Pact) {
            MPLog.Log("OnChoosePact hook triggering", LogLevel.Minor);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChoosePact"));
        }
        MPLog.Log("OnChooseThis hook triggering", LogLevel.Minor);
        __instance.itemObj.Trigger((FTrigger)Enum.Parse(typeof(FTrigger), "OnChooseThis"));
        MPLog.Log("OnChoose hook triggering", LogLevel.Minor);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChoose"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DeckCtrl), nameof(DeckCtrl.RemoveArtifactCard))]
    static void OnRemoveArtifact(ListCard cardToRemove) {
        if (cardToRemove.itemObj.type == ItemType.Art) {
            MPLog.Log("OnRemoveArtifact hook triggering", LogLevel.Minor);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemoveArtifact"));
        }
        if (cardToRemove.itemObj.type == ItemType.Pact) {
            MPLog.Log("OnRemovePact hook triggering", LogLevel.Minor);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemovePact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnUpgrade(ChoiceCard __instance) {
        if (__instance.rewardType == RewardType.Upgrade) {
            MPLog.Log("OnUpgrade hook triggering", LogLevel.Minor);
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnUpgrade"));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ListCard), nameof(ListCard.RemoveThisCard))]
    static void OnRemove(ListCard __instance, bool useRemover) {
        MPLog.Log("OnRemoveThis hook triggering", LogLevel.Minor);
        __instance.itemObj.Trigger((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemoveThis"));
        MPLog.Log("OnRemove hook triggering", LogLevel.Minor);
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemove"));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Animator), nameof(Animator.SetTrigger), new Type[] { typeof(string) })]
    static void OnAnimationTrigger(Animator __instance, string name) {
        if (name == "taunt") {
            foreach (var p in S.I.batCtrl.currentPlayers) {
                if (p.anim == __instance) {
                    MPLog.Log("OnTaunt hook triggering", LogLevel.Minor);
                    S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnTaunt"));
                    break;
                }
            }
        }
    }
}
