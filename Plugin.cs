using UnityEngine;
using HarmonyLib;

using System.Collections;
using System.Linq;
using System.Reflection.Emit;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Factions;
using Pixelfactor.IP.Engine.WorldGeneration;
using Pixelfactor.IP.Engine.Sandbox;

namespace SrPhantm {

    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.1.3.0")]
    public class Tools : BaseUnityPlugin {
        ConfigEntry<bool> configRenamer; 
        ConfigEntry<bool> configSandboxSettingsOverrides;
        ConfigEntry<float> configAutorunDelay;
        ConfigEntry<bool> configApplyPatches;
        ConfigEntry<int> configInitDelay;

        public void Awake() {
            configInitDelay = Config.Bind("Base", "InitDelay", 3, "How long to delay before setting up Objects.");
            configAutorunDelay = Config.Bind("Base", "Delay", 30.0f, "Time between runs in seconds.");
            configSandboxSettingsOverrides = Config.Bind("SandBoxSettings", "Enabled", true, "Enable/Disable sandbox settings mods");
            configRenamer = Config.Bind("Renamer", "Enabled", true, "Enable/Disable GameObject renamer.");
            configApplyPatches = Config.Bind("Patcher", "Patch", true, "Enable/Disable Harmony patches.");
            Logger.LogInfo("Loaded configuration");

            if (configApplyPatches.Value == true) {
                HarmonyPatcher.Patch(Logger);
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

            Logger.LogInfo("Object injected successfully");
            Destroy(this);
        }
    }

    class SandboxSettingsOverrides : MonoBehaviour {
        ManualLogSource logger;
        ConfigEntry<bool> configRun;

        public void Init(ManualLogSource a_logger, ConfigEntry<bool> a_configRun) {
            logger = a_logger;
            configRun = a_configRun;
        }

        public void Start() {
            if(configRun.Value) {
                GameController gameController = GameObject.FindObjectOfType<GameController>();
                var sectorCounts = gameController.GameSettings.SandboxSettings.SectorCounts;
                var s = sectorCounts.AsEnumerable();
                s = s.Append(new SandboxSectorCount { Name = "Extra Large", MaxCount = 128, MinCount = 128 });
                s = s.AddItem(new SandboxSectorCount { Name = "Massive", MaxCount = 256, MinCount = 256 });
                s = s.AddItem(new SandboxSectorCount { Name = "Super Mass.", MaxCount = 512, MinCount = 512 });
                s = s.AddItem(new SandboxSectorCount { Name = "Hyper Mass.", MaxCount = 1028, MinCount = 1028 });
                s = s.AddItem(new SandboxSectorCount { Name = "Ultra Mass.", MaxCount = 2048, MinCount = 2048 });
                s = s.Reverse();
                s= s.AddItem(new SandboxSectorCount { Name = "Lonely", MaxCount = 1, MinCount = 1 });
                s = s.Reverse();
                sectorCounts =  s.Cast<SandboxSectorCount>().ToArray();
                gameController.GameSettings.SandboxSettings.SectorCounts = sectorCounts;
                gameController.GameSettings.SandboxSettings.DefaultSectorCount = sectorCounts[3];
                logger.LogInfo("Updated SandBoxSettings.SectorCounts");

            } else {
                Destroy(this);
            }
            Destroy(this);
        }
    }

    class Renamer : MonoBehaviour {
        ManualLogSource logger;
        ConfigEntry<bool> configRun;
        ConfigEntry<float> configAutorunDelay;
        float nextRun = 0.0f;

        public void Init(ManualLogSource a_logger, ConfigEntry<bool> a_configRun, ConfigEntry<float> a_configAutorunDelay) {
            logger = a_logger;
            configRun = a_configRun;
            configAutorunDelay = a_configAutorunDelay;
        }

        public void Update() {
            if (configRun.Value && Time.time > nextRun) {
                NameAll();
                nextRun = Time.time + configAutorunDelay.Value;
            } else if (!configRun.Value) {
                Destroy(this);
            }
        }

        public void NameAll() {
            NameAllFactionGOs();
            NameAllSectorGOs();
            NameAllUnitGOs();
            logger.LogInfo("Updated names");
        }

        public void NameAllFactionGOs() {
            Faction[] factions = GameObject.FindObjectsOfType<Faction>();
            foreach (Faction f in factions) {
                f.name = f.GetLongNameElseShort();
            }
        }

        public static void NameAllSectorGOs() {
            Sector[] sectors = GameObject.FindObjectsOfType<Sector>();
            foreach (Sector s in sectors) {
                s.name = s.Name;
            }
        }

        public static void NameAllUnitGOs() {
            Unit[] units = GameObject.FindObjectsOfType<Unit>();
            foreach (Unit u in units) {
                u.AutoNameGameObject();
                UnitComponentHolder uch = u.GetComponent<UnitComponentHolder>();
                if (uch != null) {
                    u.name = u.name + " " + uch.ShipName;
                }
            }
        }
    }

    class HarmonyPatcher {
        public static void Patch(ManualLogSource logger) {
            Harmony.DEBUG = true;
            var harmony = new Harmony("com.srphantm.IP.tools.patches");
            harmony.PatchAll();
            logger.LogInfo("Patches applied");
        }
    }

    [HarmonyPatch(typeof(WorldBlueprintSectorGenerator))]
    [HarmonyPatch(nameof(WorldBlueprintSectorGenerator.Generate))]
    class WorldBlueprintSectorGenerator_Patch {
        static System.Collections.Generic.IEnumerable<CodeInstruction> Transpiler(System.Collections.Generic.IEnumerable<CodeInstruction> instructions) {
            var codes = new System.Collections.Generic.List<CodeInstruction>(instructions);
            bool setNext = false;
            for (var i = 0; i < codes.Count; i++) {
                var strOperand = codes[i].operand as string;
                if (strOperand == " and ") {
                    setNext = true;
                }
                if (setNext == true && codes[i].opcode == OpCodes.Ldloc_3 &&
                codes[i+1].opcode == OpCodes.Ldc_I4_1 &&
                codes[i+2].opcode == OpCodes.Add &&
                codes[i+3].opcode == OpCodes.Stloc_3) {
                    codes[i].opcode = OpCodes.Nop;
                    codes[i+1].opcode = OpCodes.Nop;
                    codes[i+2].opcode = OpCodes.Nop;
                    codes[i+3].opcode = OpCodes.Nop;
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}
