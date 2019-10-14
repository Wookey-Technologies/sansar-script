/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;

namespace ScriptLibrary
{

    [Tooltip("Monitor objects for script crashes and low available memory.")]
    [DisplayName("Script Watcher")]
    public class ScriptWatcher : SceneObjectScript
    {
        #region EditorProperties
        [DisplayName("Watched Objects")]
        [Tooltip("Watched Objects\nAdd any objects with critical scripts here. If any scripts on them crash the script will log to the Debug Console. If Reset World is set then the world will also reset.")]
        public List<RigidBodyComponent> Objects;

        [DisplayName("Watch Memory")]
        [Tooltip("Watch Memory\nIf enabled this script will log to the debug console when memory passes Critical levels. If Reset World is also set then the script will reset the world when memory passes Critical levels.")]
        [DefaultValue(false)]
        public bool MemoryWatch;

        [DisplayName("Reset World")]
        [Tooltip("Reset World\nReset the world when any scripts in watched objects crash. If disabled the script will only log to the script debug window.")]
        public bool ResetScene;
        #endregion
        private Dictionary<ObjectId, int> AllObjects = new Dictionary<ObjectId, int>();

        // Init() is where the script is setup and is run when the script starts.
        public override void Init()
        {
            foreach (var rb in Objects)
            {
                if (rb == null) continue;

                ObjectPrivate op = ScenePrivate.FindObject(rb.ComponentId.ObjectId);
                if (op != null)
                {
                    var scripts = op.LookupScripts();
                    AllObjects.Add(op.ObjectId, scripts == null ? 0 : scripts.Length);
                }
            }

            if (MemoryWatch) Memory.Subscribe(MemoryUpdate);

            Timer.Create(60, 60, Check);
        }

        public void Check()
        {
            List<ObjectId> keys = new List<ObjectId>(AllObjects.Keys);
            foreach (var opId in keys)
            {
                ObjectPrivate op = ScenePrivate.FindObject(opId);
                if (op == null
                    || !op.IsValid)
                {
                    Error(op.Name);
                    AllObjects.Remove(opId);
                    continue;
                }

                int oldScriptCount = AllObjects[opId];
                int newScriptCount = op.LookupScripts().Length;
                if (oldScriptCount > newScriptCount)
                {
                    Error(op.Name);
                }

                // Either still initializing and adding scripts or only logging.
                AllObjects[opId] = newScriptCount;
            }
        }

        public void Error(string name)
        {
            Log.Write(LogLevel.Error, "DEBUGGER", "A SCRIPT ON WATCHED OBJECT " + name + " HAS CRASHED.");
            if (ResetScene)
            {
                Log.Write(LogLevel.Error, "RESET", "RESETTING WORLD.");
                ScenePrivate.ResetScene();
            }
        }

        private bool HasLogged = false;
        void MemoryUpdate(MemoryData data)
        {
            if (data.UseLevel == MemoryUseLevel.Critical
                || data.UseLevel == MemoryUseLevel.Limit
                || Memory.UsedBytes > Memory.PolicyCritical)
            {
                if (!HasLogged)
                {
                    Log.Write(LogLevel.Error, "RESET", "SCRIPT MEMORY HAS PASSED CRITICAL LEVEL" + (ResetScene ? ", RESETTING WORLD" : "") + ". " + Memory.UsedBytes + "/" + Memory.PolicyCritical);
                    HasLogged = true;
                }

                if (ResetScene)
                {
                    ScenePrivate.ResetScene();
                }
            }
            else
            {
                HasLogged = false;
            }
        }
    }
}