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
    [DisplayName(nameof(QuestObject))]
    public abstract class QuestObjectBase : LibraryBase
    {
        #region EditorProperties

        protected abstract ObjectiveDefinition ObjectiveDefinition { get; }

        [Tooltip("If the Objective has a RequiredCount then completing the objective with this object will count Value times."
+ "\nHas no effect if the Objective does not have a RequiredCount, or RequiredCount < 1."
            + "\nSetting Count Value to 0 will complete the objective regardless of counts.")]
        [DisplayName("Count Value")]
        [DefaultValue(1)]
        public readonly int ObjectiveValue;

        [Tooltip("Collect Duration\nHow long in seconds does it take to collect this objective. If set to 0 collection is instant."
    + "\nIf the collection is not instant a user will not be able to collect another item until their current collection completes.")]
        [DefaultValue(0)]
        [DisplayName("Collect Time")]
        [Range(0,60)]
        public int CollectTime;

        [Tooltip("Collect Text\nIf Collect Time is > 0 this text will show with the progress bar for the collection.")]
        [DefaultValue("Collecting...")]
        [DisplayName("Collect Text")]
        public string CollectText;

        [Tooltip("Collect Color\nIf Collect Time is > 0 the collecting progress bar will be this color.")]
        [DefaultValue(0.1f,0.8f,0.3f)]
        [DisplayName("Collect Color")]
        public Color CollectColor;

        [Tooltip("Cancel Distance\nIf the objective has a Collect Time, Cancel Distance can be set so that if the user moves more than this far from where they started collecting the collecting will be canceled."
    + "\nIf set to 0 then movement will not cancel the collecting.")]
        [DefaultValue(2)]
        [Range(0, 25)]
        [DisplayName("Cancel Distance")]
        public float CancelDistance;

        [Tooltip("Collected Hint\nIf set a hint will be shown briefly after a hint is collected. If the quest has a required count the hint will include the progress on that collection.")]
        [DefaultValue(true)]
        [DisplayName("Collected Hint")]
        public bool CollectedHintsEnabled = true;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("objective_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("objective_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond when the scene is loaded
If StartEnabled is false then the script will not respond until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        HashSet<SessionId> activeSessionIds = new HashSet<SessionId>();
        float CancelDistanceSquared = 0;
        protected bool BaseErrored = false;

        protected override void SimpleInit()
        {
            CancelDistanceSquared = CancelDistance * CancelDistance;
            var addUserSubscription = ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);

            // for edge case where users can join scene before AddUser subscription is added
            foreach (var agent in ScenePrivate.GetAgents())
            {
                OnAddAgent(agent);
            }

            var removeUserSubscription = ScenePrivate.User.Subscribe(User.RemoveUser, (UserData data) =>
            {
                activeSessionIds.Remove(data.User);
            });

            if (!VerifyDefinition())
            {
                addUserSubscription.Unsubscribe();
                removeUserSubscription.Unsubscribe();
                BaseErrored = true;
            }
        }

        bool VerifyDefinition()
        { 
            if (ObjectiveDefinition == null)
            {
                Log.Write(LogLevel.Error, "Objective Definition not found.");
                return false;
            }

            if (ObjectiveDefinition.Ready)
            {
                return true;
            }

            var update = WaitFor(ObjectiveDefinition.Update);

            if (!update.Success)
            {
                Log.Write(LogLevel.Error, "Failed to update objective definition.");
                return false;
            }

            return true;
        }

        protected void CollectObjective(SessionId session, int index = -1)
        {
            var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, session) as ObjectiveDefinition.GetObjectiveData;

            if (objectiveData.Success)
            {
                try
                {
                    if (objectiveData.Objective.GetState() != ObjectiveState.Active)
                    {
                        return;
                    }
                } catch (Exception) { }

                AgentPrivate ap = ScenePrivate.FindAgent(session);
                if (ap == null) return;

                if (CollectTime == 0)
                {
                    OnObjectiveCollectStarted(ap.AgentInfo, objectiveData.Objective, index);
                    FinalizeCollectObjective(ap.AgentInfo, objectiveData.Objective, index);
                    return;
                }

                try
                {
                    if (ap.Client.UI.GetProgressBars().Count() == 0)
                    {
                        OnObjectiveCollectStarted(ap.AgentInfo, objectiveData.Objective, index);
                        var bar = ap.Client.UI.AddProgressBar();
                        bar.Start(CollectText, (float)CollectTime, CollectColor, (data) => 
                        {
                            if (data.Success)
                                FinalizeCollectObjective(ap.AgentInfo, objectiveData.Objective, index);
                            else
                                OnObjectiveCollectCanceled(ap.AgentInfo, objectiveData.Objective, index);
                        });

                        if (CancelDistance > 0.001)
                        {
                            StartCoroutine(DistanceCheck, ap, bar, index);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        protected void DistanceCheck(AgentPrivate agent, UIProgressBar bar, int index = -1)
        {
            ObjectPrivate op;
            Vector startingPosition;
            try
            {
                op = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
                if (op == null || !op.IsValid) return;

                startingPosition = op.Position;
            }
            catch (Exception)
            {
                return;
            }

            while(true)
            {
                try
                {
                    if (!agent.IsValid
                        || !op.IsValid
                        || !bar.IsValid)
                    {
                        return;
                    }

                    if ((op.Position - startingPosition).LengthSquared() > CancelDistanceSquared)
                    {
                        bar.Cancel();
                    }

                    Wait(0.2f);
                }
                catch(Exception)
                {
                    return;
                }
            }
        }

        protected void CancelCollection(AgentPrivate agent, int index = -1)
        {
            if (CollectTime > 0)
            {
                if (agent != null && agent.IsValid )
                {
                    try
                    {
                        foreach (var bar in agent.Client.UI.GetProgressBars())
                        {
                            if (bar.Label == CollectText)
                            {
                                bar.Cancel();
                                break;
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        protected void FinalizeCollectObjective(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            try
            {
                int collectedSoFar = 0;
                if (ObjectiveValue == 0 || objective.Definition.RequiredCount <= 1)
                {
                    objective.SetState(ObjectiveState.Completed);
                }
                else
                {
                    collectedSoFar = objective.Count + ObjectiveValue;
                    collectedSoFar = Math.Min(collectedSoFar, objective.Definition.RequiredCount);
                    collectedSoFar = Math.Max(collectedSoFar, 0);
                    objective.Count = collectedSoFar;
                }
                if (CollectedHintsEnabled) StartCoroutine(CollectedHint, objective, collectedSoFar);

                OnObjectiveCollectCompleted(agentInfo, objective, collectedSoFar, index);
            }
            catch (Exception) { }
        }

        void CollectedHint(Objective objective, int collectedSoFar)
        {
            try
            {
                string hint = null;
                if (objective.Definition.RequiredCount < 1)
                {
                    hint = $"Completed: {objective.Definition.Title}";
                }
                else
                {
                    hint = $"[{collectedSoFar}/{objective.Definition.RequiredCount}] {objective.Definition.Title}";
                }

                AgentPrivate agent = ScenePrivate.FindAgent(objective.Agent);
                agent.Client.UI.HintText = hint;
                Wait(2);
                if (agent.Client.UI.HintText == hint) agent.Client.UI.HintText = "";
            }
            catch (Exception) { }
        }

        void OnAddUser(UserData data)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.User);
            OnAddAgent(agent);
        }

        void OnAddAgent(AgentPrivate agent)
        { 
            if (agent == null || !agent.IsValid)
            {
                return;
            }

            Objective objective = null;
            AgentInfo agentInfo = null;

            try
            {
                agentInfo = agent.AgentInfo;
            }
            catch
            {
                Log.Write(LogLevel.Error, "Failed to get agent info, user may have left experience.");
                return;
            }

            if (!VerifyDefinition())
            {
                return;
            }

            var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, agentInfo.SessionId) as ObjectiveDefinition.GetObjectiveData;
            if (objectiveData.Success)
            {
                objective = objectiveData.Objective;
            }


            if (objective != null && !activeSessionIds.Contains(agentInfo.SessionId))
            {
                try
                {
                    activeSessionIds.Add(agentInfo.SessionId);
                    SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {objective.Definition.Title} state: {objective.GetState()}");

                    switch (objective.GetState())
                    {
                        case ObjectiveState.Active:
                            OnObjectiveActive(agentInfo, objective, true);
                            break;

                        case ObjectiveState.Locked:
                            OnObjectiveLocked(agentInfo, objective, true);
                            break;

                        case ObjectiveState.Completed:
                            OnObjectiveCompleted(agentInfo, objective, true);
                            break;
                    }

                    objective.Subscribe(ObjectiveState.Active, (ObjectiveData d) =>
                    {
                        OnObjectiveActive(agentInfo, objective);
                    });

                    objective.Subscribe(ObjectiveState.Locked, (ObjectiveData d) =>
                    {
                        OnObjectiveLocked(agentInfo, objective);
                    });

                    objective.Subscribe(ObjectiveState.Completed, (ObjectiveData d) =>
                    {
                        OnObjectiveCompleted(agentInfo, objective);
                    });

                    objective.Subscribe(ObjectiveState.None, (ObjectiveData d) =>
                    {
                        OnObjectiveReset(agentInfo, objective);
                    });
                }
                catch (Exception) { }

                OnObjectiveJoinExperience(agentInfo, objective);
            }
        }

        protected virtual void OnObjectiveCollectCompleted(AgentInfo agentInfo, Objective objective, int newCount, int index = -1)
        {

        }

        protected virtual void OnObjectiveCollectCanceled(AgentInfo agentInfo, Objective objective, int index = -1)
        {

        }

        protected virtual void OnObjectiveCollectStarted(AgentInfo agentInfo, Objective objective, int index = -1)
        {

        }

        protected virtual void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
        }

        protected virtual void OnObjectiveActive(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Active."); } catch (Exception) { }
        }

        protected virtual void OnObjectiveLocked(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Locked."); } catch (Exception) { }
        }

        protected virtual void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            try {SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Completed."); } catch (Exception) { }
        }

        protected virtual void OnObjectiveReset(AgentInfo agentInfo, Objective objective)
        {
            try {SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Reset."); } catch (Exception) { }
        }

        protected SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData data = new SimpleData(this);
            data.AgentInfo = agentInfo;
            data.ObjectId = agentInfo.ObjectId;
            data.ObjectId = data.AgentInfo != null ? data.AgentInfo.ObjectId : ObjectPrivate.ObjectId;
            data.SourceObjectId = ObjectPrivate.ObjectId;
            return data;
        }

        protected SimpleData GetEventData(SessionId session, ObjectId source)
        {
            SimpleData data = new SimpleData(this);
            data.SourceObjectId = source;
            data.AgentInfo = ScenePrivate.FindAgent(session)?.AgentInfo;
            data.ObjectId = data.AgentInfo != null ? data.AgentInfo.ObjectId : source;
            return data;
        }

    }
}

