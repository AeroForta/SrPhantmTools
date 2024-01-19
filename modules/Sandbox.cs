using System.Linq;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;
using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Sandbox;

namespace SrPhantm {
    class SandboxSettingsOverrides : MonoBehaviour {
        ManualLogSource logger;
        public ConfigEntry<bool> configRun;

        public void Init(ManualLogSource a_logger, ConfigEntry<bool> a_configRun) {
            logger = a_logger;
            configRun = a_configRun;
        }

        public void Start() {
            if(configRun.Value) {
                GameController gameController = GameObject.FindObjectOfType<GameController>();
                var sectorCounts = gameController.GameSettings.SandboxSettings.SectorCounts;
                var s = sectorCounts.AsEnumerable();
                s = s.AddItem(new SandboxSectorCount { Name = "Extra Large", MaxCount = 128, MinCount = 128 });
                s = s.AddItem(new SandboxSectorCount { Name = "Massive", MaxCount = 256, MinCount = 256 });
                s = s.AddItem(new SandboxSectorCount { Name = "Super Mass.", MaxCount = 512, MinCount = 512 });
                s = s.AddItem(new SandboxSectorCount { Name = "Hyper Mass.", MaxCount = 1028, MinCount = 1028 });
                s = s.AddItem(new SandboxSectorCount { Name = "Ultra Mass.", MaxCount = 2048, MinCount = 2048 });
                s = s.Reverse();
                s = s.AddItem(new SandboxSectorCount { Name = "Lonely", MaxCount = 1, MinCount = 1 });
                s = s.Reverse();
                sectorCounts =  s.Cast<SandboxSectorCount>().ToArray();
                gameController.GameSettings.SandboxSettings.SectorCounts = sectorCounts;
                gameController.GameSettings.SandboxSettings.DefaultSectorCount = sectorCounts[3];
                logger.LogInfo("Updated SandBoxSettings.SectorCounts");
            }
        }
    }
}