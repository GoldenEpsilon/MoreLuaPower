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
    public void Update()
    {
        foreach (object script in UpdateScripts) {
            S.I.mainCtrl.StartCoroutine(MoreLuaPower_FunctionHelper.EffectRoutine(Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().CreateCoroutine(script)));
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