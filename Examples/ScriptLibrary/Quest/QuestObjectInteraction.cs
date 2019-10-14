/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Provides an interface to a user created quest objective.")]
    [DisplayName("Quest Objective Interaction")]
    public class QuestObjectInteraction : QuestObjectBase
    {
        #region EditorProperties
        [Tooltip(@"The objective definition.")]
        [DisplayName("Objective")]
        public readonly ObjectiveDefinition _ObjectiveDefinition;
        protected override ObjectiveDefinition ObjectiveDefinition => _ObjectiveDefinition;

        [Tooltip(@"Interaction Prompt\nThe text that will show over every objective.")]
        [DefaultValue("Collect Me!")]
        [DisplayName("Interaction Prompt")]
        public string InteractionPrompt;

        [Tooltip("Add Meshes from other objects in the scene for them to act as Objectives identical to this one.")]
        [DisplayName("Extra Objectives")]
        public List<MeshComponent> InteractionObjectives;

        [Tooltip("Hint Text\nThe text to display to users that are inside any Hint Volumes")]
        [DisplayName("Hint Text")]
        [DefaultValue("Click the object to collect!")]
        public readonly string HintText;

        [Tooltip("Hint Volumes\nAdd Trigger Volumes in the scene for areas to show hints\nIf the number of Hint Volumes matches Extra Objectives + this objective then the hint will enable/disable with the corresponding objective.")]
        [DisplayName("Hint Volumes")]
        public List<RigidBodyComponent> InteractionHints;

        [Tooltip("Hide Collected\nHide the objective once it is collected."
            + "\nRequires the objective meshes have Is Scriptable set.")]
        [DisplayName("Hide Collected")]
        [DefaultValue(false)]
        public bool HideCollected;

        [Tooltip("Hide Inactive\nOnly show these objectives while active."
            + "\nRequires the objective meshes have Is Scriptable set.")]
        [DisplayName("Hide Inactive")]
        [DefaultValue(true)]
        public bool HideInactive;

        [Tooltip("Reset After\nSetting this above 0 will reset the visibility and enable the objective that many seconds after it is collected."
    + "\nCollected visibility and enabled state will also reset if the user logs out, leaves the scene or changes outfits.")]
        [DefaultValue(0)]
        [Range(0,600)]
        [DisplayName("Reset After")]
        public int ResetDelay;

        [Tooltip("Globally Collect\nIf enabled then collected objectives will enable/disable for everyone. If disabled only the user who collected it will see it enable or disable."
    + "\nOnly effective when the objective has a RequiredCount > 1."
    + "\nCollected visibility will also reset for any user that logs out, leaves the scene or changes outfits.")]
        [DefaultValue(false)]
        [DisplayName("Globally Collect")]
        public bool GloballyCollect;
        #endregion

        bool enabled;

        enum CollectionState
        {
            None,
            Collecting,
            Active
        }
        Dictionary<ComponentId, Dictionary<SessionId, CollectionState>> Collectors = new Dictionary<ComponentId, Dictionary<SessionId, CollectionState>>();
        private List<Interaction> Interactions = new List<Interaction>();
        CollectionState GetState(int index, SessionId sessionId)
        {
            if (GloballyCollect)
            {
                sessionId = SessionId.Invalid;
            }

            if (Collectors[InteractionObjectives[index].ComponentId].TryGetValue(sessionId, out CollectionState state))
            {
                return state;
            }

            return CollectionState.None;
        }

        protected override void InitObjective()
        {
            MeshComponent localMesh;
            if (ObjectPrivate.TryGetFirstComponent<MeshComponent>(out localMesh))
            {
                if (!InteractionObjectives.Contains(localMesh)) InteractionObjectives.Insert(0, localMesh);
            }

            InteractionObjectives = InteractionObjectives.Distinct().ToList();
            InteractionHints = InteractionHints.Distinct().ToList();
            int meshCount = 0;
            for (int i = 0; i < InteractionObjectives.Count; ++i)
            {
                int index = i;
                var mesh = InteractionObjectives[i];

                if (mesh != null && mesh.IsValid)
                {
                    ++meshCount;

                    AddInteraction(mesh.ComponentId, index);
                    Collectors[mesh.ComponentId] = new Dictionary<SessionId, CollectionState>();

                    RigidBodyComponent hint = null;
                    if (InteractionHints.Count > i) hint = InteractionHints[i];
                    else if (InteractionHints.Count > 0) hint = InteractionHints[0];

                    if (hint != null && hint.IsValid && hint.IsTriggerVolume())
                    {
                        hint.Subscribe(CollisionEventType.Trigger, (data) => HintCollide(data, index));
                    }

                    if (HideInactive && mesh.IsScriptable) mesh.SetIsVisible(false);
                    if (!mesh.IsScriptable && (HideInactive || HideCollected))
                    {
                        SimpleLog(LogLevel.Error, "Mesh '" + mesh.Name + "' is not scriptable and will not be hidden.");
                    }
                }
            }

            if (meshCount == 0)
            {
                QLog(LogLevel.Error, "Must be attached to a mesh or have valid meshes set in Extra Objectives.");
                return;
            }

            SubscribeToAll(EnableEvent, (ScriptEventData) => { enabled = true; });
            SubscribeToAll(DisableEvent, (ScriptEventData) => { enabled = false; });
            enabled = StartEnabled;

            Timer.Create(3, 73, Cleanup);
        }

        void AddInteraction(ComponentId componentId, int index)
        {
            Interactions.Add(null);

            ObjectPrivate obj = ScenePrivate.FindObject(componentId.ObjectId);
            if (obj == null || !obj.IsValid) return;

            obj.AddInteraction(InteractionPrompt, false, (data) =>
            {
                if (data.Success == true && data.Interaction != null)
                {
                    Interactions[index] = data.Interaction;
                    data.Interaction.Subscribe((interactData) => OnInteract(interactData, index));
                }
            });
        }

        // Try and clean up stale values. 
        void Cleanup()
        {
            foreach (var objective in Collectors)
            {
                List<SessionId> ToRemove = new List<SessionId>();
                foreach (var collector in objective.Value)
                {
                    if (collector.Value == CollectionState.None
                        || ScenePrivate.FindAgent(collector.Key) == null)
                    {
                        ToRemove.Add(collector.Key);
                    }
                }

                foreach (var collector in ToRemove)
                {
                    objective.Value.Remove(collector);
                }
            }
        }

        void HintCollide(CollisionData collisionData, int index)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(collisionData.HitComponentId.ObjectId);
            if (agent == null) return;

            // If leaving the volume or the objective is disabled try and clear the hint regardless
            if (collisionData.Phase == CollisionEventPhase.TriggerExit
                || !enabled)
            {
                try
                {
                    if (agent.Client.UI.HintText == HintText)
                    {
                        agent.Client.UI.HintText = "";
                    }
                }
                catch (Exception) { }
                return;
            }

            try
            {
                if (collisionData.Phase == CollisionEventPhase.TriggerEnter)
                {
                    var state = GetState(index, agent.AgentInfo.SessionId);
                    if (state == CollectionState.Active)
                    {
                        agent.Client.UI.HintText = HintText;
                    }
                }
            }
            catch (Exception) { }
        }

        void OnInteract(InteractionData interactionData, int index)
        {
            if (enabled)
            {
                try
                {
                    AgentPrivate agent = ScenePrivate.FindAgent(interactionData.AgentId);
                    if (agent == null) return;

                    var state = GetState(index, agent.AgentInfo.SessionId);
                    if (state == CollectionState.Active)
                    {
                        // agentInfo and agentInfo.SessionId are both cached and do not need try/catch if agent isn't null.
                        CollectObjective(agent.AgentInfo.SessionId, index);
                    }
                }
                catch (Exception) { }
            }
        }

        private void Set(SessionId session, int index, CollectionState state)
        {
            if (GloballyCollect)
            {
                session = SessionId.Invalid;
            }
            bool enabled = state != CollectionState.None;

            try
            {
                if (state != CollectionState.None)
                {
                    Collectors[InteractionObjectives[index].ComponentId][session] = state;
                }
                else
                {
                    Collectors[InteractionObjectives[index].ComponentId].Remove(session);
                }

                if (GloballyCollect)
                {
                    Interactions[index].SetEnabled(enabled);
                }
                else
                {
                    Interactions[index].SetEnabled(session, enabled);
                }

                if (HideCollected && InteractionObjectives[index].IsScriptable)
                {
                    if (GloballyCollect)
                    {
                        InteractionObjectives[index].SetIsVisible(enabled);
                    }
                    else
                    {
                        InteractionObjectives[index].SetIsVisible(session, enabled);
                    }
                }
            }
            catch { }
        }

        protected override void OnObjectiveCollectCompleted(AgentInfo agentInfo, Objective objective, int newCount, int index = -1)
        {
            Set(agentInfo.SessionId, index, CollectionState.None);

            try
            {
                if (ResetDelay > 0 && objective.Count < objective.Definition.RequiredCount)
                {
                    Timer.Create(ResetDelay, () =>
                    {
                        try
                        {
                            if (objective.IsValid && agentInfo.IsValid
                                && objective.GetState() == ObjectiveState.Active)
                            {
                                Set(agentInfo.SessionId, index, CollectionState.Active);
                            }
                            else
                            {
                                Set(agentInfo.SessionId, index, CollectionState.None);
                            }
                        }
                        catch (Exception) { }
                    });
                }
            }
            catch (Exception) { }
        }

        protected override void OnObjectiveCollectCanceled(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            try
            {
                if (objective.IsValid && agentInfo.IsValid
                    && objective.GetState() == ObjectiveState.Active)
                {
                    Set(agentInfo.SessionId, index, CollectionState.Active);
                }
                else
                {
                    Set(agentInfo.SessionId, index, CollectionState.None);
                }
            }
            catch (Exception)
            {
                Set(agentInfo.SessionId, index, CollectionState.None);
            }
            
        }

        protected override void OnObjectiveCollectStarted(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            Set(agentInfo.SessionId, index, CollectionState.Collecting);
            
            if (HintText != "")
            {
                try
                {
                    AgentPrivate agent = ScenePrivate.FindAgent(agentInfo.SessionId);
                    if (agent.Client.UI.HintText == HintText)
                    {
                        agent.Client.UI.HintText = "";
                    }
                }
                catch (Exception) { }
            }
        }

        void ResetSession(SessionId session, bool enabled)
        {
            if (!GloballyCollect)
            {
                foreach (var objectiveCollectors in Collectors)
                {
                    if (enabled)
                    {
                        objectiveCollectors.Value[session] = CollectionState.Active;
                    }
                    else
                    {
                        objectiveCollectors.Value.Remove(session);
                    }
                }

                foreach (var interaction in Interactions)
                {
                    if (interaction != null)
                    {
                        interaction.SetEnabled(session, enabled);
                    }
                }
            }

            if (HideInactive)
            {
                foreach (var mesh in InteractionObjectives)
                {
                    if (mesh.IsScriptable)
                    {
                        if (GloballyCollect)
                        {
                            bool globallyEnabled = Collectors[mesh.ComponentId].ContainsKey(SessionId.Invalid);
                            mesh.SetIsVisible(session, globallyEnabled);
                        }
                        else
                        {
                            mesh.SetIsVisible(session, enabled);
                        }
                    }
                }
            }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveActive(agentInfo, objective, initialJoin);
            ResetSession(agentInfo.SessionId, true);
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveCompleted(agentInfo, objective, initialJoin);
            ResetSession(agentInfo.SessionId, false);
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveLocked(agentInfo, objective, initialJoin);
            ResetSession(agentInfo.SessionId, false);
        }

        protected override void OnObjectiveReset(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveReset(agentInfo, objective, initialJoin);
            try
            {
                ResetSession(agentInfo.SessionId, objective.GetState() == ObjectiveState.Active);
            }
            catch (Exception) { }
        }

        protected override void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveJoinExperience(agentInfo, objective);
            try
            {
                ResetSession(agentInfo.SessionId, objective.GetState() == ObjectiveState.Active);
            }
            catch (Exception) { }
        }
    }
}

