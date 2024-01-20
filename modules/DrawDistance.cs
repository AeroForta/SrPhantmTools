using BepInEx.Configuration;
using BepInEx.Logging;
using Pixelfactor.IP.Engine;
using UnityEngine;
using UniverseLib;

namespace SrPhantm {
    class DrawDistance : MonoBehaviour {
        public void Init(ManualLogSource logger, ConfigEntry<bool> enabled, ConfigEntry<float> dis) {
            if (!enabled.Value) {return;}

            ActiveUnitClass[] units = Resources.LoadAll<ActiveUnitClass>("");
            foreach (var unit in units) {
                logger.LogInfo(unit.gameObject.name);
                if (dis.Value != -1f) {
                    unit.DrawDistFar = dis.Value;
                    unit.DrawDistNear = dis.Value;
                }
            }
            logger.LogInfo("Draw distances updated.");
        }
    }
}