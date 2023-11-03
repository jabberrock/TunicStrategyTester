using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunicStrategyTester
{
    internal class TesterController : MonoBehaviour
    {
        private const float GhostScale = 0.75f;
        private const int MaxCaptureFrameRate = 1000000;
        private const double CountdownDelayInSec = 2.5f;

        private Attempt attempt = null;
        private DateTime attemptStartTime;
        private readonly List<Attempt> pastAttempts = new List<Attempt>();

        private string saveName = null;
        private string saveFilePath = null;
        private string startSceneName = null;
        private Vector3 startPlayerPosition;

        private readonly List<GameObject> ghostObjects = new List<GameObject>();

        private bool newAttemptPending = false;
        private bool waitForSceneChange = false;
        private DateTime? unfreezeAt = null;
        private int originalCaptureFrameRate = 0;

        public static TesterController Instance
        {
            get
            {
                return Resources.FindObjectsOfTypeAll<TesterController>().First();
            }
        }

        public bool IsRunning
        {
            get { return this.saveName != null; }
        }

        public void StartTesting()
        {
            Logger.LogInfo("Starting new test...");

            SaveFile.SaveToDisk();

            this.saveName = SaveFile.saveDestinationName;
            this.saveFilePath = SaveFile.getBackupListForSave(this.saveName).Last();
            this.startSceneName = SceneManager.GetActiveScene().name;
            this.startPlayerPosition = PlayerCharacter.instance.lastPosition;

            // TODO: Save inventory cursor position

            this.attempt = null;
            this.pastAttempts.Clear();

            this.NewAttempt();
        }

        public void NewAttempt()
        {
            Logger.LogInfo("Starting new attempt...");

            foreach (var saveFilePath in SaveFile.getBackupListForSave(this.saveName))
            {
                if (saveFilePath != this.saveFilePath)
                {
                    File.Delete(saveFilePath);
                }
            }

            Monster.ClearRuntimeDeadMonsters();
            FileManagementGUI.LoadFileAndStart(this.saveName);
            this.newAttemptPending = true;
            this.waitForSceneChange = true;

            this.ghostObjects.Clear();
        }

        public void SaveAttempt(bool isComplete)
        {
            if (this.attempt != null)
            {
                Logger.LogInfo("Attempt saved");

                if (isComplete)
                {
                    this.attempt.MarkComplete();
                }

                this.pastAttempts.Add(this.attempt);
                this.attempt = null;
            }
        }

        public void IgnoreCurrentOrLastAttempt()
        {
            if (this.attempt != null)
            {
                Logger.LogInfo("Ignored attempt");

                this.attempt = null;
            }
            else
            {
                if (this.pastAttempts.Count > 0)
                {
                    Logger.LogInfo("Removed last attempt");

                    this.pastAttempts.RemoveAt(this.pastAttempts.Count - 1);
                }
            }
        }

        public void Update()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            if (activeScene.name != this.startSceneName)
            {
                this.SaveAttempt(true);
                this.waitForSceneChange = false;
                return;
            }

            var playerCharacter = PlayerCharacter.instance;
            if (playerCharacter == null)
            {
                return;
            }

            if (this.newAttemptPending && !this.waitForSceneChange)
            {
                this.attempt = new Attempt();

                this.unfreezeAt = DateTime.Now.AddSeconds(CountdownDelayInSec);
                this.originalCaptureFrameRate = Time.captureFramerate;
                Time.captureFramerate = MaxCaptureFrameRate;

                this.newAttemptPending = false;
            }

            FullscreenFader.FullscreenFade = 0.0f;

            if (this.unfreezeAt != null && DateTime.Now > this.unfreezeAt.Value)
            {
                Time.captureFramerate = this.originalCaptureFrameRate;
                this.unfreezeAt = null;

                this.attemptStartTime = DateTime.Now;

                PlayerCharacter.SetPosition(this.startPlayerPosition);
            }

            var timeSinceStartInSec = (DateTime.Now - this.attemptStartTime).TotalSeconds;

            if (this.attempt != null)
            {
                this.attempt.AddSample(
                    timeSinceStartInSec,
                    PlayerCharacter.instance.lastPosition,
                    PlayerCharacter.instance.lastRotation);
            }

            if (this.ghostObjects.Count != this.pastAttempts.Count)
            {
                this.EnsureGhostObjects();
            }

            if (this.ghostObjects.Count == this.pastAttempts.Count)
            {
                for (int i = 0; i < this.ghostObjects.Count; ++i)
                {
                    var sample = this.pastAttempts[i].At(timeSinceStartInSec);
                    if (sample == null)
                    {
                        continue;
                    }

                    this.ghostObjects[i].transform.SetPositionAndRotation(sample.Position, sample.Rotation);
                }
            }
            else
            {
                Logger.LogWarning("Mismatch between ghost count and past attempt count");
            }
        }

        private void EnsureGhostObjects()
        {
            this.ghostObjects.Clear();

            Logger.LogInfo("Recreating ghosts...");

            var lurePrefab = Resources.FindObjectsOfTypeAll<Bait>().FirstOrDefault();
            if (lurePrefab == null)
            {
                Logger.LogWarning("Failed to find lure prefab");
                return;
            }

            var lureObject = lurePrefab.gameObject;

            // Deactivate the lure prefab so that the instantiated ghost is initially deactivated
            var isLureActive = lureObject.activeSelf;
            lureObject.SetActive(false);

            for (var i = 0; i < pastAttempts.Count; ++i)
            {
                var ghostObject = GameObject.Instantiate(lureObject);
                ghostObject.name = $"Replay Ghost #{i}";
                ghostObject.transform.localScale = new Vector3(GhostScale, GhostScale, GhostScale);

                foreach (var child in ghostObject.GetComponents<Component>())
                {
                    var childType = child.GetIl2CppType();
                    if (childType != UnhollowerRuntimeLib.Il2CppType.Of<Animator>() &&
                        childType != UnhollowerRuntimeLib.Il2CppType.Of<Transform>())
                    {
                        GameObject.Destroy(child);
                    }
                }

                ghostObject.SetActive(true);

                // Destroy FMOD behaviors so that it doesn't play the running sound
                // This can only be done when the game object is active, otherwise GetBehaviours doesn't return any behaviors
                foreach (var behavior in ghostObject.GetComponent<Animator>().GetBehaviours<StateMachineBehaviour>())
                {
                    if (behavior.GetIl2CppType() == UnhollowerRuntimeLib.Il2CppType.Of<FMODAnimationStateBehaviour>())
                    {
                        GameObject.Destroy(behavior);
                    }
                }

                this.ghostObjects.Add(ghostObject);
            }

            lureObject.SetActive(isLureActive);

            Logger.LogInfo($"Created {this.ghostObjects.Count} ghosts");
        }

        public void OnGUI()
        {
            double? lastCompletedDuration = null;
            if (this.pastAttempts.Count > 0)
            {
                var lastAttempt = this.pastAttempts.Last();
                lastCompletedDuration = lastAttempt.CompletedDuration();
            }

            double? bestCompletedDuration = null;
            foreach (var attempt in this.pastAttempts)
            {
                var completedDuration = attempt.CompletedDuration();
                if (completedDuration == null)
                {
                    continue;
                }

                if (!bestCompletedDuration.HasValue || completedDuration.Value < bestCompletedDuration.Value)
                {
                    bestCompletedDuration = completedDuration;
                }
            }

            var recordTextBuilder = new StringBuilder();

            if (lastCompletedDuration.HasValue)
            {
                recordTextBuilder.Append(string.Format("Last: {0:0.###}", lastCompletedDuration.Value));
            }

            if (bestCompletedDuration.HasValue)
            {
                if (recordTextBuilder.Length > 0)
                {
                    recordTextBuilder.Append(", ");
                }

                recordTextBuilder.Append(string.Format("Best: {0:0.###}", bestCompletedDuration.Value));
            }

            if (bestCompletedDuration.HasValue)
            {
                GUI.skin.label.fontSize = 50;
                GUI.skin.label.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(20, Screen.height - 100, Screen.width - 40, 80), recordTextBuilder.ToString());
            }

            if (this.unfreezeAt != null)
            {
                double timeLeftInSec = (this.unfreezeAt.Value - DateTime.Now).TotalSeconds;
                if (timeLeftInSec >= 0.0)
                {
                    GUI.skin.label.fontSize = 200;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(
                        new Rect(Screen.width / 2 - 250, Screen.height / 2 - 150, 500, 300),
                        string.Format("{0:0.##}", timeLeftInSec));
                }
            }
        }
    }
}
