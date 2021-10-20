using HarmonyLib;
using System.Collections.Generic;

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("Start")]
class UseCustomCheats
{
    static void Postfix(Player __instance) {
        if (!(__instance.rewiredPlayer == null)) {
            foreach (CustomCheatcode Cheatcode in CustomCheatcodesList.CheatsList) {
                if (Cheatcode.CharName == __instance.beingObj.beingID) {
                    if (__instance.rewiredPlayer.GetButton(Cheatcode.ButtonName)) {
                        SpellObject LuaUser = S.I.deCtrl.CreateSpellBase("Kunai", __instance, false);

                        EffectActions.CallFunctionWithItem(Cheatcode.FunctionName, LuaUser);
                    }
                }
            }
        }
    }
}


public class CustomCheatcode
{
    public string CharName;
    public string ButtonName;
    public string FunctionName;

    public CustomCheatcode(string Char, string Button, string function) {
        CharName = Char;
        ButtonName = Button;
        FunctionName = function;
    }
}

public static class CustomCheatcodesList
{
    public static List<CustomCheatcode> CheatsList = new List<CustomCheatcode>();

    public static void SetCheatcode(string Charname, string Button, string function) {
        CustomCheatcodesList.CheatsList.Add(new CustomCheatcode(Charname, Button, function));
    }
}