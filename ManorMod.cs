using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using SemanticVersioning;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine;

namespace BitchlandGiveMuchMoneyBepInWx
{
    [BepInPlugin("com.wolfitdm.GiveMeMuchMoneyBitchlandBepInEx", "GiveMeMuchMoneyBitchlandBepInEx Plugin", "1.0.0.0")]
    public class GiveMeMuchMoneyBitchlandBepInEx : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private ConfigEntry<bool> configEnableMe;

        public GiveMeMuchMoneyBitchlandBepInEx()
        {
        }

        public static Type MyGetType(string originalClassName)
        {
            return Type.GetType(originalClassName + ",Assembly-CSharp");
        }

        private static string pluginKey = "General.Toggles";

        public static bool enableThisMod = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            configEnableMe = Config.Bind(pluginKey,
                                              "EnableThisMod",
                                              true,
                                             "Whether or not you want enable this mod (default true also yes, you want it, and false = no)");


            enableThisMod = configEnableMe.Value;

            PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin GiveMeMuchMoneyForBitchLand BepInEx is loaded!");
        }

        public static void BL_AddChatOption(string chattext, Action onOption)
        {
            UI_Gameplay _gameplay = Main.Instance.GameplayMenu;

            bool foundChatOption = false;
            for (int i = 0; i < _gameplay.ChatOptions.Length; i++)
            {
                if (!_gameplay.ChatOptions[i].activeSelf)
                {
                    foundChatOption = true;
                    break;
                }
            }

            if (!foundChatOption)
            {
                int oldLength = _gameplay.ChatOptions.Length;

                string old_chattext = _gameplay.ChatOptions_text[oldLength - 1].text;

                old_chattext = Regex.Replace(old_chattext, @"[0-9]+\s+.\s+", "");
                Action old_chatcode = _gameplay.ChatOptions_code[oldLength - 1];


                _gameplay.ChatOptions[oldLength - 1].SetActive(value: false);

                _gameplay.AddChatOption("[Next options]", () =>
                {
                    _gameplay.RemoveAllChatOptions();
                    _gameplay.AddChatOption(chattext, onOption);
                    _gameplay.AddChatOption(old_chattext, old_chatcode);
                    _gameplay.SelectChatOption(0);
                    Main.Instance.MainThreads.Add(new Action(Main.Instance.GameplayMenu.OpenedChatOptionsThread));
                });
            }
            else
            {
                _gameplay.AddChatOption(chattext, onOption);
            }
        }
        public static void job_ArmyBuildingWork_Chat_ReceptionGuard(object __instance)
        {
            Logger.LogInfo("This method is called after the original method is called!");
            if (!enableThisMod)
            {
                return;
            }

            job_ArmyBuildingWork _this = (job_ArmyBuildingWork)__instance;
            UI_Gameplay _gameplay = Main.Instance.GameplayMenu;
            Person person = _gameplay.PersonChattingTo;
            
            BL_AddChatOption("Give Me 90 Mio Cash and add me 100 hunger", (Action)(() =>
            {
                Main.Instance.GameplayMenu.EnableMove();
                Main.Instance.Player.Money += 90000000;
                Main.Instance.Player.Hunger += 100;
                _gameplay.DisplaySubtitle("Here money and hunger for you!", (AudioClip)null, new Action(person.ThisPersonInt.EndTheChat));
            }));

            return;
        }
        public static void job_StripClub_StripChat(object __instance)
        {
            Logger.LogInfo("This method is called after the original method is called!");
            if (!enableThisMod)
            {
                return;
            }

            job_StripClub _this = (job_StripClub)__instance;
            UI_Gameplay _gameplay = Main.Instance.GameplayMenu;
            Person person = _gameplay.PersonChattingTo;
            BL_AddChatOption("Give Me 90 Mio Cash and add me 100 hunger", (Action)(() =>
            {
                Main.Instance.GameplayMenu.EnableMove();
                Main.Instance.Player.Money += 90000000;
                Main.Instance.Player.Hunger += 100;
                _gameplay.DisplaySubtitle("Here money and hunger for you!", (AudioClip)null, new Action(person.ThisPersonInt.EndTheChat));
            }));
            return;
        }

        public static void PatchAllHarmonyMethods()
        {
            if (!enableThisMod)
            {
                return;
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(job_ArmyBuildingWork), "Chat_ReceptionGuard", "job_ArmyBuildingWork_Chat_ReceptionGuard", false, true);
                PatchHarmonyMethodUnity(typeof(job_StripClub), "StripChat", "job_StripClub_StripChat", false, true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.GiveMeMuchMoneyBitchlandBepInEx";
            Type uniqueType = typeof(GiveMeMuchMoneyBitchlandBepInEx);

            // Create a new Harmony instance with a unique ID
            var harmony = new Harmony(uniqueId);

            if (originalClass == null)
            {
                Logger.LogInfo($"GetType originalClass == null");
                return;
            }

            MethodInfo patched = null;

            try
            {
                patched = AccessTools.Method(uniqueType, patchedMethodName);
            }
            catch (Exception ex)
            {
                patched = null;
            }

            if (patched == null)
            {
                Logger.LogInfo($"AccessTool.Method patched {patchedMethodName} == null");
                return;

            }

            // Or apply patches manually
            MethodInfo original = null;

            try
            {
                if (parameters == null)
                {
                    original = AccessTools.Method(originalClass, originalMethodName);
                }
                else
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
            }
            catch (AmbiguousMatchException ex)
            {
                Type[] nullParameters = new Type[] { };
                try
                {
                    if (patched == null)
                    {
                        parameters = nullParameters;
                    }

                    ParameterInfo[] parameterInfos = patched.GetParameters();

                    if (parameterInfos == null || parameterInfos.Length == 0)
                    {
                        parameters = nullParameters;
                    }

                    List<Type> parametersN = new List<Type>();

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo parameterInfo = parameterInfos[i];

                        if (parameterInfo == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name.StartsWith("__"))
                        {
                            continue;
                        }

                        Type type = parameterInfos[i].ParameterType;

                        if (type == null)
                        {
                            continue;
                        }

                        parametersN.Add(type);
                    }

                    parameters = parametersN.ToArray();
                }
                catch (Exception ex2)
                {
                    parameters = nullParameters;
                }

                try
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
                catch (Exception ex2)
                {
                    original = null;
                }
            }
            catch (Exception ex)
            {
                original = null;
            }

            if (original == null)
            {
                Logger.LogInfo($"AccessTool.Method original {originalMethodName} == null");
                return;
            }

            HarmonyMethod patchedMethod = new HarmonyMethod(patched);
            var prefixMethod = usePrefix ? patchedMethod : null;
            var postfixMethod = usePostfix ? patchedMethod : null;

            harmony.Patch(original,
                prefix: prefixMethod,
                postfix: postfixMethod);
        }

    }
}
