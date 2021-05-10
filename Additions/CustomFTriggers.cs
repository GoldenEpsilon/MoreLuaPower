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
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnSave"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateRunFromSave))]
    static void OnLoad() {
        S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnLoad"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnChooseArtifact(ChoiceCard __instance) {
        if (__instance.itemObj.type == ItemType.Art) {
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChooseArtifact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DeckCtrl), nameof(DeckCtrl.RemoveArtifactCard))]
    static void OnRemoveArtifact(ListCard cardToRemove) {
        if (cardToRemove.itemObj.type == ItemType.Art) {
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemoveArtifact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnChoosePact(ChoiceCard __instance) {
        if (__instance.itemObj.type == ItemType.Pact) {
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnChoosePact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DeckCtrl), nameof(DeckCtrl.RemoveArtifactCard))]
    static void OnRemovePact(ListCard cardToRemove) {
        if (cardToRemove.itemObj.type == ItemType.Pact) {
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemovePact"));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChoiceCard), nameof(ChoiceCard.GetThisCard))]
    static void OnUpgrade(ChoiceCard __instance) {
        if (__instance.rewardType == RewardType.Upgrade) {
            S.I.deCtrl.TriggerAllArtifacts((FTrigger)Enum.Parse(typeof(FTrigger), "OnUpgrade"));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ListCard), nameof(ListCard.RemoveThisCard))]
    static void OnRemove(ListCard __instance, bool useRemover) {
        __instance.itemObj.Trigger((FTrigger)Enum.Parse(typeof(FTrigger), "OnRemove"));
    }
}
