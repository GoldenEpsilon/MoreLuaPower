using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using MoonSharp.Interpreter;
using UnityEngine;
using I2.Loc;

// Allows the addition of custom default and player targeted lines from bosses and other beings to their line pool.
// Similar to how DogeBosses can have Intro, Execution etc lines without needing the localization file.

namespace EdenGossip_AdditiveLines
{
    [HarmonyPatch(typeof(TalkBox), nameof(TalkBox.GetRandomLine))]
    internal class Gossip_Lines
    {
        [HarmonyPostfix]
        // Forms new line pools with old and custom lines
        internal static void GetTalkative (ref string key, ref string __result)
        {
            List<string> keyMini = new List<string>(DecodeConversation(key));
            List<string> oldLines = new List<string>();

            string currentSkin = S.I.runCtrl.ctrl.currentPlayer.beingObj.animName;
            if (Gossip_Data.GetEdenGossip(keyMini[0], keyMini[1], keyMini[2] + "/" + currentSkin).Count > 0)
            {
                keyMini[2] += "/" + currentSkin;
            }

            int num = 1;
            while (LocalizationManager.GetTranslation(key + num, true, 0, true, false, null, null) != null)
            {
                oldLines.Add(LocalizationManager.GetTranslation(key + num, true, 0, true, false, null, null));
                num++;
            }

            List<string> lines = new List<string>(Gossip_Data.GetEdenGossip(keyMini[0], keyMini[1], keyMini[2]));

            lines.AddRange(oldLines);

            if (lines.Count > 0)
            {
                __result = lines[UnityEngine.Random.Range(0, lines.Count)];
            }
        }

        // Deconstructs the key given by the game into Boss, Term and Player arguments
        internal static List<string> DecodeConversation (string password)
        {
            BC ctrl = S.I.runCtrl.ctrl;
            List<string> decryption = new List<string>(3);

            string player = (string)ctrl.currentPlayer.beingObj.nameString.Clone();
            if (!password.Contains(player))
            {
                player = "Default";
            }
            password = password.Replace(player, "");

            string key = password.Substring(password.LastIndexOf("/") + 1);
            string boss = password.Replace("/" + key, "");

            decryption.Add(boss);
            decryption.Add(key);
            decryption.Add(player);

            return decryption;
        }
    }

    public class Gossip_Data
    {
        //Boss --> Key --> Player / Default --> List
        private static Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> servers = 
            new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        public static void AddEdenGossip (string boss, string key, string player, string line)
        {
            if (player == "default") { player = "Default"; }
            if (player == "DEFAULT") { player = "Default"; }

            if (servers.ContainsKey(boss))
            {
                if (servers[boss].ContainsKey(key))
                {
                    if (servers[boss][key].ContainsKey(player))
                    {
                        servers[boss][key][player].Add(line);
                        Debug.Log("[Gossip] Adding " + player + " response from " + boss + " for term '" + key + "'.");
                        return; //End if added line
                    }
                    else
                    {
                        servers[boss][key].Add(player, new List<string>());  //Add Player dictionary
                    }
                }
                else
                {
                    servers[boss].Add(key, new Dictionary<string, List<string>>()); //Add Key dictionary
                }
            }
            else
            {
                servers.Add(boss, new Dictionary<string, Dictionary<string, List<string>>>()); //Add Boss dictionary
            }

            AddEdenGossip(boss, key, player, line); //Run it back!
        }
        public static void RemoveEdenGossip(string boss, string key, string player, string line)
        {

            if (servers.ContainsKey(boss))
            {
                if (servers[boss].ContainsKey(key))
                {
                    if (servers[boss][key].ContainsKey(player))
                    {
                        servers[boss][key][player].Remove(line);
                        Debug.Log("[Gossip] Removing " + player + " response from " + boss + " for term '" + key + "'.");
                    }
                }
            }

        }
        public static List<string> GetEdenGossip (string boss, string key, string player)
        {

            if (servers.ContainsKey(boss))
            {
                if (servers[boss].ContainsKey(key))
                {
                    if (servers[boss][key].ContainsKey(player))
                    {
                        return servers[boss][key][player];
                    }
                }
            }

            return new List<string>();
        }
    }

    /* Previous Global Functions
    [HarmonyPatch(typeof(EffectActions), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    internal class Gossip_Globals
    {
        private static void Postfix()
        {
            Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["AddEdenGossip"] = new Action<string, string, string, string>(Gossip_Data.AddEdenGossip);
            Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["RemoveEdenGossip"] = new Action<string, string, string, string>(Gossip_Data.RemoveEdenGossip);
            Traverse.Create(Traverse.Create<EffectActions>().Field("_Instance").GetValue<EffectActions>()).Field("myLuaScript").GetValue<Script>().Globals["GetEdenGossip"] = new Action<string, string, string, string>(Gossip_Data.GetEdenGossip);
        }
    }
    */
}
