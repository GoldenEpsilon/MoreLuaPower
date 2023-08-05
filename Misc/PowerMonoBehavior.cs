using System;
using HarmonyLib;
using MoonSharp.Interpreter;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PowerMonoBehavior : MonoBehaviour
{
    public static List<Transform> sliders = new List<Transform>();
    public static List<object> UpdateScripts = new List<object>();
    public static List<Script> UpdateBaseScripts = new List<Script>();
    public void Update() {
        for (int i = 0; i < UpdateScripts.Count; i++) {
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(UpdateBaseScripts[i].CreateCoroutine(UpdateScripts[i])));
        }
        foreach(Transform slider in sliders) {
            if (slider == null) { 
                sliders.Remove(slider);
                continue;
            }
            string temp = slider.GetChild(3).GetComponent<TextMeshProUGUI>().text;
			slider.GetChild(3).GetComponent<TextMeshProUGUI>().text = string.Format("{0}: {1}", slider.name, slider.GetComponent<Slider>().value) + "%";
            if (temp != slider.GetChild(3).GetComponent<TextMeshProUGUI>().text) {
                PlayerPrefs.SetFloat(slider.name, slider.GetComponent<Slider>().value);
            }
		}
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            EnableDeveloperTools();
        }
    }
    public static List<object> GameUpdateScripts = new List<object>();
    public static List<Script> GameUpdateBaseScripts = new List<Script>();
    public void FixedUpdate() {
        if (S.I.batCtrl.GameState == GState.MainMenu ||
            S.I.batCtrl.GameState == GState.HeroSelect ||
            S.I.batCtrl.GameState == GState.GameOver) {
            return;
        }
        for (int i = 0; i < GameUpdateScripts.Count; i++) {
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(GameUpdateBaseScripts[i].CreateCoroutine(GameUpdateScripts[i])));
        }
    }
    public static IEnumerator LoadSprite(string url, Action<Texture2D> response) {
        var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (!request.isHttpError && !request.isNetworkError) {
            response(DownloadHandlerTexture.GetContent(request));
        } else {
            Debug.LogErrorFormat("error request [{0}, {1}]", url, request.error);
            response(null);
        }
        request.Dispose();
    }
    public static IEnumerator LoadSound(FileInfo file, AudioType audioType, Action<AudioClip> response) {

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + file.FullName.ToString(), audioType)) {
            yield return www.SendWebRequest();
            if (!www.isHttpError && !www.isNetworkError) {
                response(DownloadHandlerAudioClip.GetContent(www));
            } else {
                Debug.LogErrorFormat("error request [{0}, {1}]", "file://" + file.FullName.ToString(), www.error);
                response(null);
            }
            www.Dispose();
        }
    }

    public static bool GetCustomInput(KeyCode code) {
        return Input.GetKeyDown(code);
    }

    public static void AddZoneIcon(string spriteName, string dotName) {
        S.I.runCtrl.worldBar.zoneSprites.Add(dotName, LuaPowerData.sprites[spriteName]);
    }

    public static void AddCustomMusic(string AudioName, float volume = 1, float startTime = 0, float introBoundry = 0, float endBoundry = 99999) {
        S.I.StartCoroutine(AudioDoesExist(AudioName, volume, startTime, introBoundry, endBoundry));
    }


    public static IEnumerator FadeAudioIn(AudioSource source, float duration, float maxVolume) {
        float t = 0;
        while (t < duration) {
            yield return new WaitForSeconds(.02f);
            t += .02f;
            source.volume = Math.Max(.05f, Mathf.Lerp(0, maxVolume, t / duration));
        }
    }

    public static IEnumerator FadeAudioOut(AudioSource source, float duration, float maxVolume) {
        float t = 0;
        while (t < duration) {
            yield return new WaitForSeconds(.02f);
            t += .02f;
            source.volume = Math.Max(.05f, Mathf.Lerp(maxVolume, 0, t / duration));
        }
        source.volume = 0;
    }

    public static IEnumerator CheckAudioLoops(AudioSource source, float IntroBoundry, float EndBoundry)
    {
        string original = source.clip.name;
        while(true)
        {
            
            if (source.time > EndBoundry)
            {
                source.time = IntroBoundry;
            }
            if (source.clip.name != original)
            {
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator AudioDoesExist(string AudioName, float volume, float startTime, float IntroBoundry, float EndBoundry) {
        if (LuaPowerData.customMusic.ContainsKey(AudioName)) {
            Debug.Log("Warning: " + AudioName + " is already added as music");
            yield break;
        }
        LuaPowerData.customMusic[AudioName] = null;
        Dictionary<string, AudioClip> d = Traverse.Create(S.I.itemMan).Field("allAudioClips").GetValue<Dictionary<string, AudioClip>>();
        yield return new WaitWhile(() => d.ContainsKey(AudioName) == false);
        LuaPowerData.CustomMusic myAudio = new LuaPowerData.CustomMusic(S.I.itemMan.GetAudioClip(AudioName), volume, startTime, IntroBoundry, EndBoundry);

        LuaPowerData.customMusic[AudioName] = myAudio;
    }

    public static bool AudioExists(string AudioName) {
        if (LuaPowerData.customMusic.ContainsKey(AudioName) && LuaPowerData.customMusic[AudioName] != null) {
            return true;
        }
        return false;
    }

    public static void AddMusicHook(string AudioName, string zoneBgName, string type) {
        S.I.mainCtrl.StartCoroutine(_AddMusicHook(AudioName, zoneBgName, type));
    }
    public static IEnumerator _AddMusicHook(string AudioName, string zoneBgName, string type) {
        while (!LuaPowerData.customMusic.ContainsKey(AudioName) || LuaPowerData.customMusic[AudioName] == null) {
            MPLog.Log("Warning: " + AudioName + " is not added as music yet, trying again next frame", LogLevel.Info);
            yield return new WaitForSeconds(0f);
        }
        if (type == "Battle") {
            LuaPowerData.customMusic[zoneBgName + "_Battle"] = LuaPowerData.customMusic[AudioName];
        }
        if (type == "Idle") {
            LuaPowerData.customMusic[zoneBgName + "_Idle"] = LuaPowerData.customMusic[AudioName];
        }
        if (type == "Boss") {
            LuaPowerData.customMusic[zoneBgName] = LuaPowerData.customMusic[AudioName];
        }
    }

    public static void MakeZoneGenocideLenient(string str)
    {
        LuaPowerData.GenocideLenientStages.Add(str);
    }

    public static bool EnableDeveloperTools() {

        if (!S.I.consoleView.viewContainer.activeSelf) {
            S.I.CONSOLE = true;
            S.I.DEVELOPER_TOOLS = true;
            S.I.consoleView.viewContainer.SetActive(true);
            S.I.batCtrl.AddControlBlocks(Block.Console);
            S.I.consoleView.inputField.ActivateInputField();
            S.I.consoleView.inputField.Select();
            S.I.consoleView.inputField.text = "";
            return true;
        } else {
            S.I.consoleView.viewContainer.SetActive(false);
            S.I.batCtrl.RemoveControlBlocks(Block.Console);
            S.I.consoleView.inputField.DeactivateInputField();
            return false;
        }
    }

    public static void RunDev(string str) {
        if (S.I.consoleView.GetComponent<ConsoleCtrl>() != null) {
            ConsoleCtrl conRef = S.I.consoleView.GetComponent<ConsoleCtrl>();
            conRef.runCommandString(str);
        }
    }
    public static void PrintDev(string str) {
        if (S.I.consoleView.GetComponent<ConsoleCtrl>() != null) {
            ConsoleCtrl conRef = S.I.consoleView.GetComponent<ConsoleCtrl>();
            conRef.appendLogLine(str);
        }
    }
}