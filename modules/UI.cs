using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace SrPhantm.UI {
    public class UIManager : MonoBehaviour {

        ManualLogSource logger;
        public static UIBase UiBase { get; private set; }
        UI.MyPanel myPanel;

        public void Init(ManualLogSource a_logger) {
            logger = a_logger;

            UniverseLib.Universe.Init();
            UiBase = UniversalUI.RegisterUI("com.srphantm.IP.tools.UI", UiUpdate);
            myPanel = new(UiBase);
            logger.LogInfo("PhantmUI Initialized");
        }

        public void UiUpdate() {}

        public void Update() {
            if (Input.GetKeyDown("f6")) {
                UiBase.Canvas.enabled = !UiBase.Canvas.enabled;
                myPanel.Enabled = true;
            }
            if (!myPanel.Enabled) {
                UiBase.Canvas.enabled = false;
                myPanel.Enabled = true;
            } 
        }
    }

    public class MyPanel : UniverseLib.UI.Panels.PanelBase {
        public MyPanel(UIBase owner) : base(owner) {}
        public override string Name => "SrPhantm-Tools";
        public override int MinWidth => 300;
        public override int MinHeight => 200;
        public override Vector2 DefaultAnchorMin => new(0f, 0f);
        public override Vector2 DefaultAnchorMax => new(0f, 0f);
        Tools rootObj;
        InputFieldRef autorunDelayInput;
        InputFieldRef initDelayInput;

        protected override void ConstructPanelContent()
        {
            rootObj = RuntimeHelper.FindObjectsOfTypeAll<Tools>()[0];

            Text InfoText = UIFactory.CreateLabel(ContentRoot, "InfoText", "F6 to Open/Close");
            UIFactory.SetLayoutElement(InfoText.gameObject, minWidth: 100, minHeight: 25);

            GameObject conRoot = UIFactory.CreateVerticalGroup(ContentRoot, "Root", true, false, true, true);
            UIFactory.SetLayoutElement(conRoot); 
            
            GameObject toggleRenamer = CreateToggle(conRoot, "Renamer", "Renamer", out Toggle renamerToggle);
            renamerToggle.isOn = rootObj.configRenamer.Value;
            renamerToggle.onValueChanged.AddListener(OnRenamerToggle);

            autorunDelayInput = CreateInputField(conRoot, "autorunDelay", "Autorun delay", out ButtonRef autorunDelayInputButton);
            autorunDelayInput.Component.text = rootObj.configAutorunDelay.Value.ToString();
            autorunDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configAutorunDelay.Value.ToString();
            autorunDelayInputButton.OnClick += OnAutorunDelayChange;

            Text requiresRestart = UIFactory.CreateLabel(ContentRoot, "requiresRestart", "Options below will not take effect until gave is restarted.");
            UIFactory.SetLayoutElement(requiresRestart.gameObject, minWidth: 100, minHeight: 25);

            GameObject conRoot2 = UIFactory.CreateVerticalGroup(ContentRoot, "Root2", true, false, true, true);
            UIFactory.SetLayoutElement(conRoot2);

            GameObject togglePatcher = CreateToggle(conRoot2, "Patcher", "Patcher", out Toggle patcherToggle);
            patcherToggle.isOn = rootObj.configApplyPatches.Value;
            patcherToggle.onValueChanged.AddListener(OnPatcherToggle);

            initDelayInput = CreateInputField(conRoot2, "InitDelay", "Init Delay", out ButtonRef initDelayInputButton);
            initDelayInput.Component.text = rootObj.configInitDelay.Value.ToString();
            initDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configInitDelay.Value.ToString();
            initDelayInputButton.OnClick += OnInitDelayChange;

            GameObject toggleSandboxMods = CreateToggle(conRoot2, "Sandbox", "Sandbox++", out Toggle sandboxModsToggle);
            sandboxModsToggle.isOn = rootObj.configSandboxSettingsOverrides.Value;
            sandboxModsToggle.onValueChanged.AddListener(onSandboxModsToggle);
        }

        private static GameObject CreateToggle(GameObject parent, string name, string displayString ,out Toggle toggle) {
            GameObject toggleGroup = UIFactory.CreateHorizontalGroup(parent, name + "group", true, false, true, true, default, new Vector4(0, 0, 5, 0));
            Text toggleText = UIFactory.CreateLabel(toggleGroup, name + "text", displayString, TextAnchor.MiddleLeft);
            GameObject toggleObj = UIFactory.CreateToggle(toggleGroup, name + "toggle", out toggle, out Text _, default);
            UIFactory.SetLayoutElement(toggleGroup);
            UIFactory.SetLayoutElement(toggleText.gameObject, minHeight:25, minWidth:100);
            UIFactory.SetLayoutElement(toggle.gameObject, minHeight:25, minWidth:25);
            return toggleObj;
        }

        private static InputFieldRef CreateInputField(GameObject parent, string name, string displayString, out ButtonRef applyButton) {
            GameObject inputGroup = UIFactory.CreateHorizontalGroup(parent, name + "group", true, false, true, true, 5, new Vector4(0, 0, 5, 0)); 
            Text inputText = UIFactory.CreateLabel(inputGroup, name + "text", displayString);
            InputFieldRef input = UIFactory.CreateInputField(inputGroup, name + "field", "N/A");
            applyButton = UIFactory.CreateButton(inputGroup, name + "apply", "Apply", normalColor: new(0.3f, 0.3f, 0.3f));
            UIFactory.SetLayoutElement(inputGroup);
            UIFactory.SetLayoutElement(inputText.gameObject, minHeight:25, minWidth:100);
            UIFactory.SetLayoutElement(input.GameObject, preferredHeight:25, preferredWidth:50);
            UIFactory.SetLayoutElement(applyButton.GameObject, preferredHeight:25, preferredWidth:50);
            return input;
        }

        private void onSandboxModsToggle(bool val) {
            rootObj.configSandboxSettingsOverrides.Value = val;
        }

        private void OnRenamerToggle(bool val) {
            rootObj.configRenamer.Value = val;
        }

        private void OnAutorunDelayChange() {
            rootObj.configAutorunDelay.Value = float.Parse(autorunDelayInput.Component.text);
            autorunDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configAutorunDelay.Value.ToString();
        }

        private void OnPatcherToggle(bool val) {
            rootObj.configApplyPatches.Value = val;
        }

        private void OnInitDelayChange() {
            rootObj.configInitDelay.Value = float.Parse(initDelayInput.Component.text);
            initDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configInitDelay.Value.ToString();
        }
    }
}