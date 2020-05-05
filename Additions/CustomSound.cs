using HarmonyLib;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[HarmonyPatch(typeof(ModCtrl))]
[HarmonyPatch("_InstallTheseMods")]
class MoreLuaPower_SoundLoader
{
    static void Prefix(ref ModCtrl __instance, FileInfo[] fileInfo, string modsDir) {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        FileInfo[] fileInfoArray = fileInfo;
        if (S.I.GetComponent<PowerMonoBehavior>() == null) {
            S.I.gameObject.AddComponent<PowerMonoBehavior>();
        }

        for (int index = 0; index < fileInfoArray.Length; ++index) {
            FileInfo file = fileInfoArray[index];
            if (!file.Name.Contains(".meta")) {
                if (file.Name.Contains(".ogg") || file.Name.Contains(".wav")) {
                    AudioType audioType = file.Name.Contains(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV;

                    S.I.GetComponent<PowerMonoBehavior>().StartCoroutine(PowerMonoBehavior.LoadSound(file, audioType, (content) => {
                        if (content != null) {
                            if (AllAudioClips.ContainsKey(file.Name.Split('.')[0])) {
                                Debug.Log("Game already contains an Audiofile named: " + file.Name.Split('.')[0]);
                            } else {
                                AllAudioClips.Add(file.Name.Split('.')[0], content);
                            }
                        }
                    }));
                }
            }
        }
    }
}

class LuaPowerSound
{
    static public void PlaySound(Being being, string sound) {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        if (!AllAudioClips.ContainsKey(sound)) {
            Debug.Log(sound + " does not exist");
            return;
        }
        being.PlayOnce(AllAudioClips[sound]);
    }

    static public void PlayBattleMusic(string music)
    {
        var AllAudioClips = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<String, AudioClip>>();
        if (!AllAudioClips.ContainsKey(music))
        {
            Debug.Log(music + " does not exist");
            return;
        }
        S.I.muCtrl.Stop();
        S.I.muCtrl.Play(AllAudioClips[music], true);
    }
}