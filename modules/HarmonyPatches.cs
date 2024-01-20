using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using BepInEx.Logging;
using Pixelfactor.IP.Engine.WorldGeneration;
using BepInEx.Configuration;

namespace SrPhantm.HarmonyPatches {
    class HarmonyPatcher {
        public static void Patch(ManualLogSource logger, ConfigEntry<bool> enabled) {
            if (!enabled.Value) {return;}
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