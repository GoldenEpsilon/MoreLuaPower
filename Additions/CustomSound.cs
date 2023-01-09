using HarmonyLib;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[HarmonyPatch(typeof(MusicCtrl))]
[HarmonyPatch("PlayBattle")]
class MoreLuaPower_MusicBattle
{
    static bool Prefix(ref MusicCtrl __instance, int tier)
    {
        if (!__instance.battleEnvironmentsLate.ContainsKey(__instance.runCtrl.currentWorld.background) && !__instance.battleEnvironments.ContainsKey(__instance.runCtrl.currentWorld.background))
        {
            if (LuaPowerData.customMusic.ContainsKey(__instance.runCtrl.currentWorld.background + "_Battle"))
            {
                LuaPowerSound.PlayCustomMusic(__instance.runCtrl.currentWorld.background + "_Battle");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(MusicCtrl))]
[HarmonyPatch("PlayIdle")]
class MoreLuaPower_MusicIdle
{
    static bool Prefix(ref MusicCtrl __instance)
    {
        if (!__instance.idleEnvironments.ContainsKey(__instance.runCtrl.currentWorld.background))
        {
            if (LuaPowerData.customMusic.ContainsKey(__instance.runCtrl.currentWorld.background + "_Idle"))
            {
                LuaPowerSound.PlayCustomMusic(__instance.runCtrl.currentWorld.background + "_Idle");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(ModCtrl))]
[HarmonyPatch("_InstallTheseMods")]
class MoreLuaPower_SoundLoader
{
    static void Prefix(ref ModCtrl __instance, FileInfo[] fileInfo, string modsDir)
    {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        FileInfo[] fileInfoArray = fileInfo;
        if (S.I.GetComponent<PowerMonoBehavior>() == null)
        {
            S.I.gameObject.AddComponent<PowerMonoBehavior>();
        }

        for (int index = 0; index < fileInfoArray.Length; ++index)
        {
            FileInfo file = fileInfoArray[index];
            if (!file.Name.Contains(".meta"))
            {
                if (file.Name.Contains(".ogg") || file.Name.Contains(".wav"))
                {
                    AudioType audioType = file.Name.Contains(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV;

                    S.I.GetComponent<PowerMonoBehavior>().StartCoroutine(PowerMonoBehavior.LoadSound(file, audioType, (content) =>
                    {
                        if (content != null)
                        {
                            if (AllAudioClips.ContainsKey(file.Name.Split('.')[0]))
                            {
                                Debug.Log("Game already contains an Audiofile named: " + file.Name.Split('.')[0]);
                            }
                            else
                            {
                                AllAudioClips.Add(file.Name.Split('.')[0], content);
                            }
                        }
                    }));
                }
            }
        }
    }
}

public class LuaPowerSound
{
    static public void PlaySound(Being being, string sound)
    {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        if (!AllAudioClips.ContainsKey(sound))
        {
            Debug.Log(sound + " does not exist");
            return;
        }
        being.PlayOnce(AllAudioClips[sound]);
    }

    static public void PlayCustomMusic(string MusicName)
    {
        if (LuaPowerData.customMusic.ContainsKey(MusicName) && LuaPowerData.customMusic[MusicName] != null)
        {
            S.I.StartCoroutine(PowerMonoBehavior.FadeAudioOut(S.I.muCtrl.audioSource, .3f, S.I.muCtrl.audioSource.volume));
            S.I.muCtrl.StopIntroLoop();
            S.I.muCtrl.audioSource.volume = 0;
            S.I.muCtrl.audioSource.time = LuaPowerData.customMusic[MusicName].StartTime;
            S.I.StartCoroutine(PowerMonoBehavior.FadeAudioIn(S.I.muCtrl.audioSource, 1f, LuaPowerData.customMusic[MusicName].Volume));
            S.I.muCtrl.Play(LuaPowerData.customMusic[MusicName].AudioClip);
            S.I.muCtrl.audioSource.clip.name = MusicName;
            S.I.StartCoroutine(PowerMonoBehavior.CheckAudioLoops(S.I.muCtrl.audioSource, LuaPowerData.customMusic[MusicName].IntroBoundry, LuaPowerData.customMusic[MusicName].EndBoundry));
        } else {
            Debug.Log(MusicName + " does not exist");
            return;
        }
    }

static public void PlayCustomMusicIntroLoop(string MusicName, float IntroBoundry, float EndBoundry)
    {
        if (LuaPowerData.customMusic.ContainsKey(MusicName))
        {
            S.I.StartCoroutine(PowerMonoBehavior.FadeAudioOut(S.I.muCtrl.audioSource, .3f, S.I.muCtrl.audioSource.volume));
            S.I.muCtrl.StopIntroLoop();
            S.I.muCtrl.audioSource.volume = 0;
            S.I.muCtrl.audioSource.time = LuaPowerData.customMusic[MusicName].StartTime;
            S.I.StartCoroutine(PowerMonoBehavior.FadeAudioIn(S.I.muCtrl.audioSource, 1f, LuaPowerData.customMusic[MusicName].Volume));
            S.I.muCtrl.Play(LuaPowerData.customMusic[MusicName].AudioClip);
            S.I.muCtrl.audioSource.clip.name = MusicName;
            S.I.StartCoroutine(PowerMonoBehavior.CheckAudioLoops(S.I.muCtrl.audioSource, IntroBoundry, EndBoundry));
        }
    }
}