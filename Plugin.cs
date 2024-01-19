using UnityEngine;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;

namespace SrPhantm {

    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.2.2.0")]
    [BepInProcess("Interstellar Pilot 2.exe")]
    public class Tools : BaseUnityPlugin {
        public ConfigEntry<bool> configRenamer; 
        public ConfigEntry<bool> configSandboxSettingsOverrides;
        public ConfigEntry<float> configAutorunDelay;
        public ConfigEntry<bool> configApplyPatches;
        public ConfigEntry<float> configInitDelay;

        public void Awake() {
            configInitDelay = Config.Bind("Base", "InitDelay", 1.0f, "How long to delay before setting up Objects.");
            configAutorunDelay = Config.Bind("Base", "Delay", 30.0f, "Time between runs in seconds.");
            configSandboxSettingsOverrides = Config.Bind("SandBoxSettings", "Enabled", true, "Enable/Disable sandbox settings mods");
            configRenamer = Config.Bind("Renamer", "Enabled", true, "Enable/Disable GameObject renamer.");
            configApplyPatches = Config.Bind("Patcher", "Enabled", true, "Enable/Disable Harmony patches.");
            Logger.LogInfo("Loaded configuration");

            if (configApplyPatches.Value == true) {
                HarmonyPatches.HarmonyPatcher.Patch(Logger);
            }
            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit() {
            yield return new WaitForSeconds(configInitDelay.Value);

            GameObject hostObj = Instantiate(new GameObject("SrPhantm-Tools"));
            DontDestroyOnLoad(hostObj);

            Renamer renamer = hostObj.AddComponent<Renamer>();
            renamer.Init(Logger, configRenamer, configAutorunDelay);

            SandboxSettingsOverrides sandboxSettingsOverrides = hostObj.AddComponent<SandboxSettingsOverrides>();
            sandboxSettingsOverrides.Init(Logger, configSandboxSettingsOverrides);

            UI.UIManager uiManager = hostObj.AddComponent<UI.UIManager>();
            uiManager.Init(Logger);
            Logger.LogInfo("Object injected successfully");
        }
    }
}
