using BepInEx;
using BepInEx.IL2CPP;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace TunicStrategyTester
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class TunicReplays : BasePlugin
    {
        public override void Load()
        {
            Logger.SetLogger(this.Log);

            Logger.LogInfo("Loading plugin...");

            ClassInjector.RegisterTypeInIl2Cpp<TesterController>();
            ClassInjector.RegisterTypeInIl2Cpp<TesterSettingsGUI>();

            var tunicReplays = new GameObject("TunicStrategyTester Controller") { hideFlags = HideFlags.HideAndDontSave };
            tunicReplays.AddComponent<TesterController>();
            GameObject.DontDestroyOnLoad(tunicReplays);

            var settingsGUI = new GameObject("TunicStrategyTester Settings") { hideFlags = HideFlags.HideAndDontSave };
            settingsGUI.AddComponent<TesterSettingsGUI>();
            GameObject.DontDestroyOnLoad(settingsGUI);

            Logger.LogInfo("Plugin loaded");
        }
    }
}
