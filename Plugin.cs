using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Factions;

namespace SrPhantm {
    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.0.1.0")]

    public class Tools : BaseUnityPlugin {
        private ConfigEntry<bool> configAutorun; 
        private ConfigEntry<float> configAutorunDelay;
        private float nextRun = 0.0f;

        public void Awake() {
            configAutorun = Config.Bind("Renamer", "Autorun", true, "Enable/Disable autorunning renamer.");
            configAutorunDelay = Config.Bind("Renamer", "Delay", 10.0f, "Time between runs in seconds.");
            Logger.LogInfo("Loaded configuration");
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

        private static GameObject[] GetRootGOs() {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] objs = scene.GetRootGameObjects();
            return objs;
        }
    }
}
