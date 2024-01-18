
using System.Drawing.Printing;
using System.Security.Cryptography;
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
                myPanel.SetActive(enabled);
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
        InputFieldRef InitDelayInput;

        protected override void ConstructPanelContent()
        {
            rootObj = RuntimeHelper.FindObjectsOfTypeAll<Tools>()[0];

            Text InfoText = UIFactory.CreateLabel(ContentRoot, "InfoText", "F6 to Open/Close");
            UIFactory.SetLayoutElement(InfoText.gameObject, minWidth: 200, minHeight: 25);

            GameObject conRoot = UIFactory.CreateVerticalGroup(ContentRoot, "Root", true, false, true, true);
            UIFactory.SetLayoutElement(conRoot); 
            
            GameObject toggleRenamerGroup = UIFactory.CreateHorizontalGroup(conRoot, "Renamer", true, false, true, true, default, new Vector4(0, 0, 5, 0));
            Text toggleRenamerText = UIFactory.CreateLabel(toggleRenamerGroup, "Renamer", "Renamer", TextAnchor.MiddleLeft);
            GameObject toggleRenamer = UIFactory.CreateToggle(toggleRenamerGroup, "Renamer", out Toggle renamerToggle, out Text _, default);
            UIFactory.SetLayoutElement(toggleRenamerGroup, minHeight:25, preferredHeight:25);
            UIFactory.SetLayoutElement(toggleRenamerText.gameObject, minWidth: 200, minHeight: 25);
            UIFactory.SetLayoutElement(toggleRenamer.gameObject, minWidth: 20, minHeight: 25);
            renamerToggle.isOn = rootObj.configRenamer.Value;
            renamerToggle.onValueChanged.AddListener(OnRenamerToggle);

            GameObject autorunDelayGroup = UIFactory.CreateHorizontalGroup(conRoot, "autorunDelay", true, false, true, true, 5, new Vector4(0, 0, 5, 0)); 
            Text autorunDelayText = UIFactory.CreateLabel(autorunDelayGroup, "", "Autorun Delay");
            autorunDelayInput = UIFactory.CreateInputField(autorunDelayGroup, "autorunDelay", "N/A");
            ButtonRef autorunDelayInputButton = UIFactory.CreateButton(autorunDelayGroup, "autorunDelay", "Apply", normalColor: new(0.2f, 0.4f, 0.2f));
            UIFactory.SetLayoutElement(autorunDelayText.gameObject, minWidth:100, minHeight:25);
            UIFactory.SetLayoutElement(autorunDelayGroup, minHeight:25, preferredHeight:25);
            UIFactory.SetLayoutElement(autorunDelayInput.GameObject, minWidth:50, minHeight:25);
            UIFactory.SetLayoutElement(autorunDelayInputButton.GameObject, minWidth:50, minHeight:25);
            autorunDelayInput.Component.text = rootObj.configAutorunDelay.Value.ToString();
            autorunDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configAutorunDelay.Value.ToString();
            autorunDelayInputButton.OnClick += OnAutorunDelayChange;

            Text requiresRestart = UIFactory.CreateLabel(ContentRoot, "requiresRestart", "Options below will not take effect until gave is restarted.");
            UIFactory.SetLayoutElement(requiresRestart.gameObject);

            GameObject conRoot2 = UIFactory.CreateVerticalGroup(ContentRoot, "Root2", true, false, true, true);
            UIFactory.SetLayoutElement(conRoot2);

            GameObject togglePatcherGroup = UIFactory.CreateHorizontalGroup(conRoot2, "Patcher", true, false, true, true, default, new Vector4(0, 0, 5, 0));
            Text togglePatcherText = UIFactory.CreateLabel(togglePatcherGroup, "Patcher", "Patcher", TextAnchor.MiddleLeft);
            GameObject togglePatcher = UIFactory.CreateToggle(togglePatcherGroup, "Patcher", out Toggle patcherToggle, out Text _, default);
            UIFactory.SetLayoutElement(togglePatcherGroup, minHeight:25, preferredHeight:25);
            UIFactory.SetLayoutElement(togglePatcherText.gameObject, minWidth: 200, minHeight: 25);
            UIFactory.SetLayoutElement(togglePatcher.gameObject, minWidth: 20, minHeight: 25);
            patcherToggle.isOn = rootObj.configApplyPatches.Value;
            patcherToggle.onValueChanged.AddListener(OnPatcherToggle);

            GameObject InitDelayGroup = UIFactory.CreateHorizontalGroup(conRoot2, "InitDelay", true, false, true, true, 5, new Vector4(0, 0, 5, 0)); 
            Text InitDelayText = UIFactory.CreateLabel(InitDelayGroup, "", "Init Delay");
            InitDelayInput = UIFactory.CreateInputField(InitDelayGroup, "InitDelay", "N/A");
            ButtonRef InitDelayInputButton = UIFactory.CreateButton(InitDelayGroup, "InitDelay", "Apply", normalColor: new(0.2f, 0.4f, 0.2f));
            UIFactory.SetLayoutElement(InitDelayText.gameObject, minWidth:100, minHeight:25);
            UIFactory.SetLayoutElement(InitDelayGroup, minHeight:25, preferredHeight:25);
            UIFactory.SetLayoutElement(InitDelayInput.GameObject, minWidth:50, minHeight:25);
            UIFactory.SetLayoutElement(InitDelayInputButton.GameObject, minWidth:50, minHeight:25);
            InitDelayInput.Component.text = rootObj.configInitDelay.Value.ToString();
            InitDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configInitDelay.Value.ToString();
            InitDelayInputButton.OnClick += OnInitDelayChange;
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
            rootObj.configInitDelay.Value = float.Parse(InitDelayInput.Component.text);
            InitDelayInput.Component.placeholder.GetComponent<Text>().text = rootObj.configInitDelay.Value.ToString();
        }
    }
}