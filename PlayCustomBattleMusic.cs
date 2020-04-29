using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(BC))]
[HarmonyPatch("StartBattle")]
class MoreLuaPower_PlayCustomBattleMusic
{
    static void Postfix(ref SpawnCtrl __instance)
    {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        S.I.muCtrl.Play(AllAudioClips["YoumuBossTheme"], true);
    }
}