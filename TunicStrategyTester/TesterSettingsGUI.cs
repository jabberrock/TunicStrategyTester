using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicStrategyTester
{
    internal class TesterSettingsGUI : MonoBehaviour
    {
        private static readonly HashSet<string> InGameSceneNames = new HashSet<string>()
        {
            // "UserEngagementScene",
            // "Game Kickoff",
            // "Splashes",
            // "TitleScreen",
            // "tunic logobuild",
            "Sword Cave",
            "Ruined Shop",
            "Town Basement",
            "Ruins Passage",
            "Mountain",
            "Mountaintop",
            "Forest Boss Room",
            "Sword Access",
            "Fortress Main",
            "Fortress Basement",
            "Fortress Courtyard",
            "Fortress Arena",
            "Ziggurat_Arena",
            "Library Lab",
            "Library Hall",
            "Library Rotunda",
            "Quarry",
            "Monastery",
            "Darkwoods Tunnel",
            "Temple",
            "Overworld Redux",
            "Overworld Interiors",
            "Sewer",
            "Library Arena",
            "Crypt",
            "Town_FiligreeRoom",
            "Archipelagos Redux",
            "Atoll Redux",
            "Frog Stairs",
            "Library Exterior",
            "Void",
            "Forest Belltower",
            // "Playable Intro",
            "Resurrection",
            "Transit",
            // "titlecard",
            "archipelagos_house",
            "ziggurat2020_2",
            "ziggurat2020_1",
            "ziggurat2020_3",
            "ziggurat2020_0",
            "ziggurat2020_FTRoom",
            "Fortress East",
            "Fortress Reliquary",
            "Waterfall",
            "Overworld Cave",
            "Sewer_Boss",
            "frog cave main",
            "East Forest Redux",
            "East Forest Redux Interior",
            "East Forest Redux Laddercave",
            "Shop",
            "Furnace",
            "Windmill",
            "Swamp Redux 2",
            "Quarry Redux",
            "Cathedral Arena",
            "RelicVoid",
            "Spirit Arena",
            "Crypt Redux",
            "ShopSpecial",
            "CubeRoom",
            "PatrolCave",
            "Maze Room",
            "Cathedral Redux",
            "Changing Room",
            // "DungeonAudioTest",
            // "Credits Scroll",
            // "Credits First",
            // "GameOverDecision",
            // "FinalBossDeath",
            // "FinalBossDeath2",
            // "FinalBossBefriend",
            "Purgatory",
            // "g_elements",
            // "boids",
            // "Loading",
            "EastFiligreeCache",
            "Dusty",
            // "DPADTesting",
            // "Posterity",
        };

        public void OnGUI()
        {
            
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && InGameSceneNames.Contains(scene.name))
            {
                Cursor.visible = true;

                GUI.skin.window.fontSize = 20;

                GUI.Window(
                    101,
                    new Rect(Screen.width - 340.0f, Screen.height - 360.0f, 320.0f, 220.0f), 
                    new Action<int>(DrawWindow),
                    "Strategy Tester");
            }
        }

        private static void DrawWindow(int windowID)
        {
            GUI.skin.label.fontSize = 20;

            var shouldNext = GUI.Button(new Rect(10.0f, 40.0f, 300.0f, 40.0f), "Start Testing");
            if (shouldNext)
            {
                TesterController.Instance.StartTesting();
            }

            if (TesterController.Instance.IsRunning)
            {
                var shouldRetry = GUI.Button(new Rect(10.0f, 90.0f, 300.0f, 40.0f), "Next Attempt");
                if (shouldRetry)
                {
                    TesterController.Instance.SaveAttempt(false);
                    TesterController.Instance.NewAttempt();
                }

                var shouldClearAtemps = GUI.Button(new Rect(10.0f, 140.0f, 300.0f, 40.0f), "Ignore Attempt & Retry");
                if (shouldClearAtemps)
                {
                    TesterController.Instance.IgnoreCurrentOrLastAttempt();
                    TesterController.Instance.NewAttempt();
                }
            }
        }
    }
}
