using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Factions;
using Pixelfactor.IP.Engine.WorldGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SrPhantm {
    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.1.0.0")]

    public class Renamer : BaseUnityPlugin {
        private ConfigEntry<bool> configAutorun; 
        private ConfigEntry<float> configAutorunDelay;
        private float nextRun = 0.0f;

        public void Awake() {
            configAutorun = Config.Bind("Renamer", "Autorun", true, "Enable/Disable autorunning renamer.");
            configAutorunDelay = Config.Bind("Renamer", "Delay", 10.0f, "Time between runs in seconds.");
            Logger.LogInfo("Loaded configuration");

            Harmony.DEBUG = true;
            var harmony = new Harmony("com.srphantm.IP.tools.patches");
            harmony.PatchAll();
            Logger.LogInfo("Patches applied");
        }

        public void Update() {
            if (configAutorun.Value && Time.time > nextRun) {
                NameAll();
                nextRun = Time.time + configAutorunDelay.Value;
            }
        }

        public void NameAll() {
            NameAllFactionGOs();
            NameAllSectorGOs();
            NameAllUnitGOs();
            Logger.LogInfo("Updated names");
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

    [HarmonyPatch(typeof(WorldBlueprintSectorGenerator))]
    [HarmonyPatch(nameof(WorldBlueprintSectorGenerator.Generate))]
    class WorldBlueprintSectorGenerator_Patch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions){
            var codes = new List<CodeInstruction>(instructions);
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
