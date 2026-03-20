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
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine;

namespace BitchlandGiveMuchMoneyBepInWx
{
    [BepInPlugin("com.wolfitdm.ManorMod", "ManorMod Plugin", "1.0.0.0")]
    public class ManorMod : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private static ConfigEntry<bool> configNoCosts = null;
        private static ConfigEntry<KeyCode> configKeyCodeR = null;
        private static ConfigEntry<KeyCode> configKeyCodeS = null;

        public ManorMod()
        {
        }

        public static Type MyGetType(string originalClassName)
        {
            return Type.GetType(originalClassName + ",Assembly-CSharp");
        }

        private static string pluginKey = "General.Toggles";

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            configNoCosts = Config.Bind("General.Toggles",
                                                         "ActionsNoCost",
                                                          true,
                                                         "Actions are free yes = true, false = no");

            configKeyCodeR = Config.Bind("General.KeyControls",
                                             "KeyCodeToGetUnlimitedPowerAndContribution",
                                              KeyCode.R,
                                             "KeyCodeToGetUnlimitedPowerAndContribution, default R");

            configKeyCodeS = Config.Bind("General.KeyControls",
                                 "KeyCodeToToggleActionsAreFree",
                                  KeyCode.S,
                                 "KeyCodeToToggleActionsAreFree, default S");

            MagicCardNoCosts = configNoCosts.Value;

            PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin ManorMod BepInEx is loaded!");
        }

        private static bool MagicCardNoCosts = true;
        private void OnGUI()
        {
            if (Input.GetKeyUp(configKeyCodeR.Value))
            {
                foreach (GameState instance in GameStateInstance)
                {
                    instance.contribution += 9000;
                    instance.playerPower += 9000;

                    Logger.LogInfo($"ContributionPower and player power 9000 added");
                    Logger.LogInfo($"You see the changes in next turn");
                }
            }

            if (Input.GetKeyUp(configKeyCodeS.Value))
            {
                configNoCosts.Value = MagicCardNoCosts = !MagicCardNoCosts;
                Logger.LogInfo($"MagicCardNoCosts: {MagicCardNoCosts}");
            }
        }

        private static List<GameState> GameStateInstance = new List<GameState>();
        public static void ConsumeContribution(int amount, object __instance)
        {
            GameState instance = (GameState)__instance;
            if (!GameStateInstance.Contains(instance)) {
                GameStateInstance.Add(instance);
            }
        }

        public static void ConsumePlayerPower(int amount, object __instance)
        {
            GameState instance = (GameState)__instance;
            if (!GameStateInstance.Contains(instance))
            {
                GameStateInstance.Add(instance);
            }
        }
        public static void CalculatePlayerPowerGain(object __instance)
        {
            GameState instance = (GameState)__instance;
            if (!GameStateInstance.Contains(instance))
            {
                GameStateInstance.Add(instance);
            }
        }

        public static bool MasterMagicCostMultiplier(bool directMasterOrWitchAction, CharacterCard onTarget, bool awareMatters, ref float __result)
        {
            if (!MagicCardNoCosts)
            {
                return true;
            }
            __result = 0;
            return false;
        }
        public static void PatchAllHarmonyMethods()
        {
            try
            {
                PatchHarmonyMethodUnity(typeof(GameState), "ConsumeContribution", "ConsumeContribution", false, true);
                PatchHarmonyMethodUnity(typeof(GameState), "ConsumePlayerPower", "ConsumePlayerPower", false, true);
                PatchHarmonyMethodUnity(typeof(GameState), "CalculatePlayerPowerGain", "CalculatePlayerPowerGain", false, true);
                PatchHarmonyMethodUnity(typeof(CardInteractions), "MasterMagicCostMultiplier", "MasterMagicCostMultiplier", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }
        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.GiveMeMuchMoneyBitchlandBepInEx";
            Type uniqueType = typeof(ManorMod);

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
