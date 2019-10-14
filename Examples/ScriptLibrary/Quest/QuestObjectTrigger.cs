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
    [DisplayName("Quest Objective Trigger")]
    public class QuestObjectTriggerVolume : QuestObjectBase
    {
        #region EditorProperties
        [Tooltip(@"The objective definition.")]
        [DisplayName("Objective")]
        public readonly ObjectiveDefinition _ObjectiveDefinition;
        protected override ObjectiveDefinition ObjectiveDefinition => _ObjectiveDefinition;

        [Tooltip("Add Trigger Volume references from the scene for them to act as Objectives identical to this one.")]
        [DisplayName("Extra Objectives")]
        public List<RigidBodyComponent> TriggerObjectives;

        [Tooltip("Hint Text\nThe text to display to users that are inside any Hint Volumes")]
        [DisplayName("Hint Text")]
        public readonly string HintText;

        [Tooltip("Hint Volumes"
            + "\nAdd Trigger Volumes in the scene for areas to show hints"
            + "\nIf the number of Hint Volumes matches Extra Objectives + this objective then the hint will enable/disable with the corresponding objective.")]
        [DisplayName("Hint Volumes")]
        public List<RigidBodyComponent> TriggerHints;

        [Tooltip("Disable Collected\nDisable each objective object as they are collected."
    + "\nOnly effective when the objective has a RequiredCount > 1 and Hide Collected if disabled."
    + "\nCollected objects will become re-enabled if the objective is not completed and the user logs out, leaves the scene or changes outfits.")]
        [DefaultValue(false)]
        [DisplayName("Disable Collected")]
        public bool DisableWhenCollected;

        [Tooltip("Reset After\nSetting this above 0 will re-enable the objective that many seconds after it is collected."
    + "\nOnly effective when the objective has a RequiredCount > 1 and Disable Collected is enabled."
    + "\nCollected objects will also become re-enabled if the objective is not completed and the user logs out, leaves the scene or changes outfits.")]
        [DefaultValue(0)]
        [Range(0,600)]
        [DisplayName("Reset After")]
        public int ResetDelay;

        [Tooltip("Globally Collect\nIf enabled then collected objectives will enable/disable for everyone, otherwise it will only enable/disable for the user who collected it."
    + "\nOnly effective when the objective has a RequiredCount > 1."
    + "\nIf disabled then collected objects will become re-enabled if the objective is not completed and the user logs out, leaves the scene or changes outfits.")]
        [DefaultValue(false)]
        [DisplayName("Globally Collect")]
        public bool GloballyCollect;
        #endregion

        bool ScriptEnabled = true;

        enum CollectionState
        {
            None,
            Collecting,
            Active
        }
        Dictionary<int, Dictionary<SessionId, CollectionState>> Collectors = new Dictionary<int, Dictionary<SessionId,CollectionState>>();

        CollectionState GetState(int index, SessionId sessionId)
        {
            if (!ObjectiveDefinition.Ready)
            {
                QLog(LogLevel.Warning, "Definition not ready.");
                return CollectionState.None;
            }
            else if (sessionId != SessionId.Invalid && !IsTracked(sessionId))
            {
                QLog(LogLevel.Warning, "Session not yet tracking.");
                OnAddAgent(sessionId);
                return CollectionState.None;
            }

            if (Collectors[index].TryGetValue(sessionId, out CollectionState state))
            {
                return state;
            }
            return CollectionState.None;
        }

        protected override void InitObjective()
        {
            RigidBodyComponent triggerVolume;
            if (ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out triggerVolume) && triggerVolume.IsTriggerVolume())
            {
                if (!TriggerObjectives.Contains(triggerVolume))
                {
                    QLog(LogLevel.Info, "Adding scripted object as trigger volume.");
                    TriggerObjectives.Insert(0, triggerVolume);
                }
            }

            TriggerObjectives = TriggerObjectives.Distinct().ToList();
            TriggerHints = TriggerHints.Distinct().ToList();
            int rbs = 0;
            for(int i=0; i< TriggerObjectives.Count; ++i)
            {
                var trigger = TriggerObjectives[i];
                
                RigidBodyComponent hint = null;
                int index = i;
                if (trigger != null && trigger.IsValid && trigger.IsTriggerVolume())
                {
                    ++rbs;
                    QLog(LogLevel.Info, "Subscribing to collision " + trigger.Name + ", index " + index);

                    Collectors[i] = new Dictionary<SessionId, CollectionState>();
                    trigger.Subscribe(CollisionEventType.Trigger, (data) => OnCollide(data, index));

                    if (TriggerHints.Count > i) hint = TriggerHints[i];
                    else if (TriggerHints.Count > 0) hint = TriggerHints[0];

                    if (hint != null && hint.IsValid && hint.IsTriggerVolume())
                    {
                        QLog(LogLevel.Info, "Subscribing to hint " + hint.Name + ", index " + index);
                        hint.Subscribe(CollisionEventType.Trigger, (data) => HintCollide(data, index));
                    }
                }
            }

            if (rbs == 0)
            {
                QLog(LogLevel.Error, "Must be attached to a trigger volume or have Trigger Volumes set in Extra Objectives.");
                return;
            }

            SubscribeToAll(EnableEvent, (ScriptEventData) => { ScriptEnabled = true; });
            SubscribeToAll(DisableEvent, (ScriptEventData) => { ScriptEnabled = false; });
            ScriptEnabled = StartEnabled;

            Timer.Create(300, 500, Cleanup);
        }

        // Try and clean up stale values. 
        void Cleanup()
        {
            foreach(var objective in Collectors)
            {
                List<SessionId> ToRemove = new List<SessionId>();
                foreach(var collector in objective.Value)
                {
                    if (collector.Value == CollectionState.None
                        || ScenePrivate.FindAgent(collector.Key) == null)
                    {
                        QLog(LogLevel.Info, "Removing user.");
                        ToRemove.Add(collector.Key);
                    }
                }

                foreach(var collector in ToRemove)
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
                || !ScriptEnabled)
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
                    SessionId sessionId = GloballyCollect ? SessionId.Invalid : agent.AgentInfo.SessionId;
                    var state = GetState(index, sessionId);
                    if (state == CollectionState.Active)
                    {
                        agent.Client.UI.HintText = HintText;
                    }
                }
            }
            catch (Exception) { }
        }

        void OnCollide(CollisionData collisionData, int index)
        {
            if (ScriptEnabled)
            {
                try
                {
                    AgentPrivate agent = ScenePrivate.FindAgent(collisionData.HitComponentId.ObjectId);
                    if (agent == null)
                    {
                        QLog(LogLevel.Info, "No agent found.");
                        return;
                    }

                    SessionId sessionId = GloballyCollect ? SessionId.Invalid : agent.AgentInfo.SessionId;
                    var state = GetState(index, sessionId);

                    QLog(LogLevel.Info, "Phase=" + collisionData.Phase + ", state=" + state.ToString() + ", Global:" + GloballyCollect, agent.AgentInfo.SessionId);
                    if (collisionData.Phase == CollisionEventPhase.TriggerEnter)
                    {
                        if (state == CollectionState.Active)
                        {
                            // agentInfo and agentInfo.SessionId are both cached and do not need try/catch if agent isn't null.
                            CollectObjective(agent.AgentInfo.SessionId, index);
                        }
                        else
                        {
                            // Maybe this isn't even a NofM quest and things are just weird?
                            if (ObjectiveDefinition.RequiredCount <= 1)
                            {
                                QLog(LogLevel.Warning, "Non-NoM quest with weird state: collecting anyway.");
                                CollectObjective(agent.AgentInfo.SessionId, index);
                            }
                        }
                    }
                    else if (collisionData.Phase == CollisionEventPhase.TriggerExit)
                    {
                        if (state == CollectionState.Collecting)
                        {
                            CancelCollection(agent, index);
                        }
                    }
                }
                catch (Exception e)
                {
                    QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
                }
            }
            else
            {
                QLog(LogLevel.Warning, "Script disabled.");
            }
        }

        protected override void OnObjectiveCollectCompleted(AgentInfo agentInfo, Objective objective, int newCount, int index = -1)
        {
            base.OnObjectiveCollectCompleted(agentInfo, objective, newCount, index);

            try
            {
                if (DisableWhenCollected)
                {
                    Collectors[index].Remove(agentInfo.SessionId);
                }
                else
                {
                    if (objective.IsValid && objective.GetState() == ObjectiveState.Active)
                    {
                        Collectors[index][agentInfo.SessionId] = CollectionState.Active;
                    }
                    else
                    {
                        Collectors[index].Remove(agentInfo.SessionId);
                    }
                }
            }
            catch (Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
            }

            if (DisableWhenCollected && ResetDelay > 0)
            {
                Timer.Create(ResetDelay, () =>
                {
                    try
                    {
                        if (objective.IsValid && objective.GetState() == ObjectiveState.Active)
                        {
                            Collectors[index][agentInfo.SessionId] = CollectionState.Active;
                        }
                    }
                    catch (Exception e)
                    {
                        QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
                    }
                });
            }
        }

        protected override void OnObjectiveCollectCanceled(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            base.OnObjectiveCollectCanceled(agentInfo, objective, index);
            try
            {
                if (objective.IsValid && objective.GetState() == ObjectiveState.Active)
                {
                    Collectors[index][agentInfo.SessionId] = CollectionState.Active;
                }
            }
            catch (Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
            }
        }

        protected override void OnObjectiveCollectStarted(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            base.OnObjectiveCollectStarted(agentInfo, objective, index);
            Collectors[index][agentInfo.SessionId] = CollectionState.Collecting;

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
                catch (Exception e)
                {
                    QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
                }
            }
        }

        void ResetSession(SessionId session, bool enabled)
        {
            foreach (var objectiveCollectors in Collectors)
            {
                try
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
                catch(Exception e)
                {
                    QLog(LogLevel.Error, "Exception " + e.GetType() + ": " + e.ToString());
                }
            }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveActive(agentInfo, objective);
            ResetSession(agentInfo.SessionId, true);
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveCompleted(agentInfo, objective);
            ResetSession(agentInfo.SessionId, false);
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveLocked(agentInfo, objective);
            ResetSession(agentInfo.SessionId, false);
        }

        protected override void OnObjectiveReset(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveReset(agentInfo, objective);
            try
            {
                ResetSession(agentInfo.SessionId, objective.GetState() == ObjectiveState.Active);
            }
            catch (Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
            }
        }

        protected override void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveJoinExperience(agentInfo, objective);
            try
            {
                ResetSession(agentInfo.SessionId, objective.GetState() == ObjectiveState.Active);
            }
            catch (Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().Name + ": " + e.ToString());
            }
        }
    }
}

