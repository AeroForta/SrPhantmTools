using UnityEngine;
using BepInEx.Configuration;
using BepInEx.Logging;
using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Factions;

namespace SrPhantm {
    class Renamer : MonoBehaviour {
        ManualLogSource logger;
        public ConfigEntry<bool> configRun;
        public ConfigEntry<float> configAutorunDelay;
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
}