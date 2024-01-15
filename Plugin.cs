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

namespace SrPhantm {

    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.1.0.0")]
    public class Tools : BaseUnityPlugin {
        private ConfigEntry<bool> configAutorun; 
        private ConfigEntry<float> configAutorunDelay;
        private ConfigEntry<bool> configApplyPatches;
        private ConfigEntry<int> configInitDelay;

        public void Awake() {
            configInitDelay = Config.Bind("Base", "InitDelay", 5, "How long to delay before setting up Objects.");
            configAutorun = Config.Bind("Renamer", "Autorun", true, "Enable/Disable autorunning renamer.");
            configAutorunDelay = Config.Bind("Renamer", "Delay", 10.0f, "Time between runs in seconds.");
            configApplyPatches = Config.Bind("Patcher", "Patch", true, "Enable/Disable the game patcher.");
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
            renamer.Init(Logger, configAutorun, configAutorunDelay);
            Logger.LogInfo("Object injected successfully");
            Destroy(this);
        }
    }

    class Renamer : MonoBehaviour {
        public ManualLogSource logger;
        public ConfigEntry<bool> configAutorun;
        public ConfigEntry<float> configAutorunDelay;
        public float nextRun = 0.0f;

        public void Init(ManualLogSource a_logger, ConfigEntry<bool> a_configAutorun, ConfigEntry<float> a_configAutorunDelay) {
            logger = a_logger;
            configAutorun = a_configAutorun;
            configAutorunDelay = a_configAutorunDelay;
        }

        public void Update() {
            if (configAutorun.Value && Time.time > nextRun) {
                NameAll();
                nextRun = Time.time + configAutorunDelay.Value;
            } else if (!configAutorun.Value) {
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
