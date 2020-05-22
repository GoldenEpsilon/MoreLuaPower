using System;
using HarmonyLib;
using MoonSharp.Interpreter;
using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;

public class PowerMonoBehavior : MonoBehaviour
{
    public static List<object> UpdateScripts = new List<object>();
    public static List<Script> UpdateBaseScripts = new List<Script>();
    public void Update()
    {
        for (int i = 0; i < UpdateScripts.Count; i++) {
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(UpdateBaseScripts[i].CreateCoroutine(UpdateScripts[i])));
        }
    }
    public static List<object> GameUpdateScripts = new List<object>();
    public static List<Script> GameUpdateBaseScripts = new List<Script>();
    public void FixedUpdate()
    {
        if (S.I.batCtrl.GameState == GState.MainMenu || 
            S.I.batCtrl.GameState == GState.HeroSelect || 
            S.I.batCtrl.GameState == GState.GameOver) {
            return;
        }
        for (int i = 0; i < GameUpdateScripts.Count; i++)
        {
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
}