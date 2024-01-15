using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pixelfactor.IP.Engine;
using Pixelfactor.IP.Engine.Factions;

namespace SrPhantm {
    [BepInPlugin("com.srphantm.IP.tools", "SrPhantm's IP tools", "1.0.0.0")]

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

        public static void NameAllFactionGOs() {
            GameObject[] objs = GetRootGOs();
            foreach (GameObject obj in objs) {
                var factions = obj.GetComponents<Faction>();
                foreach (Faction f in factions) {
                    obj.name = f.GetLongNameElseShort();
                }
            }
        }

        public static void NameAllSectorGOs() {
            GameObject[] objs = GetRootGOs();
            foreach (GameObject obj in objs) {
                Sector[] sectors = obj.GetComponents<Sector>();
                foreach (Sector s in sectors) {
                    obj.name = s.Name;
                }
            }
        }

        public static void NameAllUnitGOs() {
            GameObject[] objs = GetRootGOs();
            foreach (GameObject obj in objs) {
                Sector[] sectors = obj.GetComponents<Sector>();
                foreach (Sector s in sectors) {
                    foreach (Transform child in s.transform) {
                        GameObject gObj = child.gameObject;
                        Unit[] units = gObj.GetComponents<Unit>();
                        foreach (Unit u in units) {
                            u.AutoNameGameObject();
                        }
                    }
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
