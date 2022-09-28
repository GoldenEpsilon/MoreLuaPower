using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace LuaPowerAchievements

{
    [HarmonyPatch]
    [Serializable]
    public class AchievementUpender
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AchievementsCtrl), nameof(AchievementsCtrl.GetAchievementData))]
        static List<AchievementData> patchAchievement(List<AchievementData> __return, AchievementsCtrl __instance) {
            AchievementData[] array = __return.ToArray();
            foreach (AchievementData achievement in APIV.ourAchievementData.ToArray()) {
                __return.Add(achievement);
            }
            return new List<AchievementData>((IEnumerable<AchievementData>)__return);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementsCtrl), nameof(AchievementsCtrl.IsUnlocked))]
        static bool patchIsUnlocked(ref bool __result, string achievementID) {
            // If the achievement is one of ours, we return whether it's unlocked, and skip the vanilla check.

            int found = APIV.ourAchievementData.FindIndex(a => a.AchievementName == achievementID);
            if (found != -1) {
                int PercentUnlocked = PlayerPrefs.GetInt("CustomAchievement_" + achievementID);
                __result = PercentUnlocked >= 100;
                return false;

            }
            return true;
        }
    }

    public static class APIV
    {


        public static List<AchievementData> ourAchievementData = new List<AchievementData>();
        public static void UnlockCustomAchievement(string name, int percentUnlocked = 100) {
            Debug.Log("Logging patchUnlockAchievement for " + name);
            // TO DO : Figure out how to make the percentUnlocked record for Hidden. We need to get the Hidden Value.

            int found = APIV.ourAchievementData.FindIndex(a => a.AchievementName == name);
            if (found != -1) {
                PlayerPrefs.SetInt("CustomAchievement_" + name, percentUnlocked);
                SetValue(APIV.ourAchievementData[found], "hidden", percentUnlocked < 100);
                if (percentUnlocked == 0) PlayerPrefs.SetInt("CustomAchievementToasted_" + name, percentUnlocked);
                else ShowToast(name);
            }
        }

        public static void AddCustomAchievement(string name, string icon, string description, bool hidden = false) {
            int found = APIV.ourAchievementData.FindIndex(a => a.AchievementName == name);
            int found2 = AchievementsCtrl.Instance.GetAchievementData().FindIndex(a => a.AchievementName == name);
            if (found != -1 || found2 != -1) return;
            AchievementData newAchievement = ScriptableObject.CreateInstance<AchievementData>();
            SetValue(newAchievement, "achievementName", name);
            LuaPowerLang.ImportTerm("AchievementNames/" + name, name);
            Sprite sprite = LuaPowerSprites.GetSprite(icon);
            SetValue(newAchievement, "achievementImage", sprite.texture);
            SetValue(newAchievement, "localizationDescriptionKey", description, true);
            LuaPowerLang.ImportTerm("AchievementDescriptions/" + name, description);
            SetValue(newAchievement, "localizationNameKey", name, true);
            SetValue(newAchievement, "localizationAchievedDescriptionKey", description);
            int PercentUnlocked = PlayerPrefs.GetInt("CustomAchievement_" + name);
            SetValue(newAchievement, "hidden", hidden && (PercentUnlocked < 100));
            ourAchievementData.Add(newAchievement);
        }

        private static void SetValue(AchievementData newAchievement, string fieldName, string name, bool translate = false) {
            Type typ = typeof(AchievementData);
            FieldInfo type = typ.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            type.SetValue(newAchievement, name);
            Debug.Log("Logging SetValue for " + name);
        }

        private static void SetValue(AchievementData newAchievement, string fieldName, Texture2D icon) {
            Type typ = typeof(AchievementData);
            FieldInfo type = typ.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            type.SetValue(newAchievement, icon);
        }

        private static void SetValue(AchievementData newAchievement, string fieldName, int value) {
            Type typ = typeof(AchievementData);
            FieldInfo type = typ.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            type.SetValue(newAchievement, value);
        }
        private static void SetValue(AchievementData newAchievement, string fieldName, bool value) {
            Type typ = typeof(AchievementData);
            FieldInfo type = typ.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            type.SetValue(newAchievement, value);
        }

        private static void ShowToast(string name) {
            foreach (AchievementData achData in ourAchievementData) {
                if (achData.AchievementName == name) {
                    int toasted = PlayerPrefs.GetInt("CustomAchievementToasted_" + name);
                    if (toasted != 100) {
                        AchievementPopup newAchievementPopup = S.Instantiate(AchievementsCtrl.Instance.achievementPrefab, AchievementsCtrl.Instance.achievementPopupGrid);
                        newAchievementPopup.Set(achData, AchievementsCtrl.Instance.achievementPopupGrid.childCount);
                        PlayerPrefs.SetInt("CustomAchievementToasted_" + name, 100);
                    }
                }
            }
        }

    }
}
