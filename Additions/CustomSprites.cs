using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Reflection.Emit;

static class LuaPowerSprites
{
    static public void MakeSprite(string image, string PATH, string name) {
        string str = Path.Combine(PATH, image);
        if (S.I.GetComponent<PowerMonoBehavior>() == null) {
            S.I.gameObject.AddComponent<PowerMonoBehavior>();
        }

        if (LuaPowerData.sprites.ContainsKey(name)) {
            Debug.Log("ERROR: A Sprite exists with this name already.");
        }

        S.I.GetComponent<PowerMonoBehavior>().StartCoroutine(
         PowerMonoBehavior.LoadSprite(str, (Texture2D content) => {
             if (content != null) {
                 content.filterMode = FilterMode.Point;
                 LuaPowerData.sprites[name] = Sprite.Create(content, new Rect(0f, 0f, (float)content.width, (float)content.height), new Vector2(0.5f, 0.5f), 1f);
             } else {
                 Debug.Log("ERROR: MakeSprite didn't find anything in content");
             }
         }));
    }

    static public Sprite GetSprite(string image) {
        if (LuaPowerData.sprites.ContainsKey(image)) {
            return LuaPowerData.sprites[image];
        }
        return null;
    }
}