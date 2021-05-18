using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

public class LuaPowerBrands
{
    public static Brand MakeBrand(string name, string description = null) {
        if (LuaPowerData.customEnums[typeof(Brand)].Contains(name)) {
            MPLog.Log("ERROR: A Brand exists with this name already.");
            return Brand.None;
        }
        Brand brand = (Brand)(LuaPowerData.customEnums[typeof(Brand)].Count);
        LuaPowerData.customEnums[typeof(Brand)].Add(name);
        Array.Resize(ref S.I.deCtrl.brandSprites, Mathf.Max(S.I.deCtrl.brandSprites.Length, (int)brand + 1));
        Traverse.Create(S.I.foCtrl).Field("brandTypes").Method("Add", new Type[] { typeof(Brand) }).GetValue(new object[] { brand });
        S.I.itemMan.brandTypes.Add(brand);
        BrandListCard brandListCard = UnityEngine.Object.Instantiate<BrandListCard>(S.I.foCtrl.brandListCardPrefab);
        brandListCard.transform.SetParent(S.I.foCtrl.focusGrid);
        brandListCard.foCtrl = S.I.foCtrl;
        brandListCard.displayButton = false;
        brandListCard.parentList = S.I.foCtrl.brandListCards;
        brandListCard.SetBrand(brand);
        brandListCard.tmpText.text = name;
        S.I.foCtrl.brandListCards.Add(brandListCard);
        LuaPowerLang.ImportTerm("BrandNames/" + name, name);
        if (description != null) {
            LuaPowerLang.ImportTerm("BrandDescriptions/" + name, description);
        }
        return brand;
    }
    public static string GetBrand(Brand brand) {
        return LuaPowerData.customEnums[typeof(Brand)][(int)brand];
    }
    public static void SetBrandImage(string name, string sprite, string BGSprite = null) {
        S.I.mainCtrl.StartCoroutine(_SetBrandImage(name, sprite, BGSprite));
    }
    public static IEnumerator _SetBrandImage(string name, string sprite, string BGSprite = null) {
        while (!LuaPowerData.customEnums[typeof(Brand)].Contains(name)) {
            yield return new WaitForSeconds(0f);
            MPLog.Log("ERROR: A Brand does not exist with the name "+name+".\nTrying again next frame (make a brand with MakeBrand)", LogLevel.Info);
        }
        while (LuaPowerSprites.GetSprite(sprite) == null) { yield return new WaitForSeconds(0f); }
        while (BGSprite != null && LuaPowerSprites.GetSprite(BGSprite) == null) { yield return new WaitForSeconds(0f); }
        Array.Resize(ref S.I.deCtrl.brandSprites, Mathf.Max(S.I.deCtrl.brandSprites.Length, LuaPowerData.customEnums[typeof(Brand)].IndexOf(name) + 1));
        S.I.deCtrl.brandSprites[LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)] = LuaPowerSprites.GetSprite(sprite);
        if (S.I.deCtrl.spellBackgroundBrands.Count <= LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)) {
            while (S.I.deCtrl.spellBackgroundBrands.Count <= LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)) {
                if (BGSprite != null) {
                    S.I.deCtrl.spellBackgroundBrands.Add(LuaPowerSprites.GetSprite(BGSprite));
                } else {
                    S.I.deCtrl.spellBackgroundBrands.Add(LuaPowerSprites.GetSprite(sprite));
                }
            }
        } else if (BGSprite != null && LuaPowerSprites.GetSprite(BGSprite) != null) {
            S.I.deCtrl.spellBackgroundBrands[LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)] = LuaPowerSprites.GetSprite(BGSprite);
        } else {
            S.I.deCtrl.spellBackgroundBrands[LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)] = LuaPowerSprites.GetSprite(sprite);
        }
        foreach (BrandListCard b in S.I.foCtrl.brandListCards) {
            if (b.brand == (Brand)LuaPowerData.customEnums[typeof(Brand)].IndexOf(name)) {
                b.image.sprite = LuaPowerSprites.GetSprite(sprite);
            }
        }
    }
}