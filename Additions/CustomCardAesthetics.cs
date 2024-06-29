using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;

public class LuaPowerCardAesthetics_Database
{
    internal static Dictionary<string, Sprite> itemBGs = new Dictionary<string, Sprite>();
    internal static Dictionary<string, Sprite> itemBorders = new Dictionary<string, Sprite>();
    internal static Dictionary<string, Color> itemTexts = new Dictionary<string, Color>();

    internal static Color flavorTextEffect = Color.white;

    internal static void AddItemBG(string id, Sprite bg)
    {
        if (!itemBGs.ContainsKey(id))
        {
            itemBGs.Add(id, bg);
        }
        itemBGs[id] = bg;
    }
    internal static void AddItemBorder(string id, Sprite border)
    {
        if (!itemBorders.ContainsKey(id))
        {
            itemBorders.Add(id, border);
        }
        itemBorders[id] = border;
    }
    internal static void AddItemText(string id, List<float> color)
    {
        if (color.Count < 3)
        {
            Debug.Log("(HDUICard) Color for text of " + id + " invalid!");
            return;
        }

        if (!itemTexts.ContainsKey(id))
        {
            itemTexts.Add(id, new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f, 1));
        }
        itemTexts[id] = new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f, 1);
    }

    internal static bool InDatabase(string id)
    {
        return itemBGs.ContainsKey(id) || itemBorders.ContainsKey(id) || itemTexts.ContainsKey(id);
    }
}

[HarmonyPatch(typeof(UICard), "SetCard", new Type[] { typeof(ItemObject) })]
public class MPL_ItemAesthetics_Render
{
    [HarmonyPostfix]
    internal static void Display(UICard __instance)
    {
        S.I.mainCtrl.StartCoroutine(_Display(__instance));
    }
    internal static IEnumerator _Display(UICard __instance)
    {
        if (LuaPowerCardAesthetics_Database.flavorTextEffect == Color.white)
        {
            LuaPowerCardAesthetics_Database.flavorTextEffect = S.I.deCtrl.cardInnerSpellPrefab.GetComponent<CardInner>().flavorText.color;
        }

        if (__instance.cardInner == null)
        {
            yield break;
        }

        ResetAesthetic(__instance.cardInner);

        if (__instance.itemObj == null)
        {
            yield break;
        }
        string id = __instance.itemObj.itemID;

        if (LuaPowerCardAesthetics_Database.InDatabase(id))
        {
            if (LuaPowerCardAesthetics_Database.itemBGs.ContainsKey(id))
            {
                __instance.cardInner.background.overrideSprite = LuaPowerCardAesthetics_Database.itemBGs[id];
            }

            if (LuaPowerCardAesthetics_Database.itemBorders.ContainsKey(id))
            {
                __instance.cardInner.border.enabled = LuaPowerCardAesthetics_Database.itemBorders[id] != null;
                if (__instance.cardInner.border.enabled)
                {
                    __instance.cardInner.border.overrideSprite = LuaPowerCardAesthetics_Database.itemBorders[id];
                }
            }

            if (LuaPowerCardAesthetics_Database.itemTexts.ContainsKey(id))
            {
                Color textColor = LuaPowerCardAesthetics_Database.itemTexts[id];
                __instance.cardInner.description.color = textColor;
                __instance.cardInner.nameText.color = textColor;
                __instance.cardInner.flavorText.color = textColor * LuaPowerCardAesthetics_Database.flavorTextEffect;
            }
        }
    }

    public static void ResetAesthetic(CardInner cardInner)
    {
        cardInner.background.overrideSprite = null;
        cardInner.border.overrideSprite = null;
        cardInner.enabled = true;

        cardInner.description.color = Color.white;
        cardInner.nameText.color = Color.white;
        cardInner.flavorText.color = LuaPowerCardAesthetics_Database.flavorTextEffect;
    }
}