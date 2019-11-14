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
using System.Runtime.CompilerServices;

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
        [DefaultValue(0.1294f,0.7373f,0.8118f)]
        [DisplayName("Collect Color")]
        public Color CollectColor;

        [Tooltip("Cancel Distance\nIf the objective has a Collect Time, Cancel Distance can be set so that if the user moves more than this far from where they started collecting the collecting will be canceled."
    + "\nIf set to 0 then movement will not cancel the collecting.")]
        [DefaultValue(2)]
        [Range(0, 25)]
        [DisplayName("Cancel Distance")]
        public float CancelDistance;

        [Tooltip("Collected Hint\nIf set a hint will be shown briefly after an objective is collected. If the quest has a required count the hint will include the progress on that collection.")]
        [DefaultValue(false)]
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

        [Tooltip("Enable Debug to log extra data to the script debug console for this script.")]
        [DefaultValue(false)]
        [DisplayName("Debug")]
        public readonly bool DebugSpam = false;
        #endregion

        Dictionary<SessionId,Objective> activeSessionIds = new Dictionary<SessionId,Objective>();
        float CancelDistanceSquared = 0;

        protected bool IsTracked(SessionId id) { return activeSessionIds.ContainsKey(id); }

        protected void QLog(LogLevel level, string message, SessionId sessionId, [CallerMemberName] string caller = "<???>")
        {
            try
            {
                if (DebugSpam || level == LogLevel.Error)
                {
                    string agentString = "<user-unknown>";
                    AgentInfo agent = ScenePrivate.FindAgent(sessionId)?.AgentInfo;
                    if (agent != null)
                    {
                        agentString = agent.Handle + ":" + agent.AvatarUuid;
                    }
                    string definitionString = "<objective-unknown>";
                    if (ObjectiveDefinition == null)
                    {
                        definitionString = "<objective-null>";
                    }
                    else if (!ObjectiveDefinition.Ready)
                    {
                        definitionString = "<objective-not-ready>";
                    }
                    else
                    {
                        definitionString = ObjectiveDefinition.Title;
                    }
                    Log.Write(level, ObjectPrivate.Name + ":" + GetType().Name + " > '" + definitionString + "' for user " + agentString);
                    Log.Write(level, caller + " > " + message);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Exception logging quest objective details: " + ex.GetType().Name);
                Log.Write(ex.ToString());
            }
        }

        protected void QLog(LogLevel level, string message, [CallerMemberName] string caller = "<???>")
        {
            try
            {
                if (DebugSpam || level == LogLevel.Error)
                {
                    string definitionString = "<objective-unknown>";
                    if (ObjectiveDefinition == null)
                    {
                        definitionString = "<objective-null>";
                    }
                    else if (!ObjectiveDefinition.Ready)
                    {
                        definitionString = "<objective-not-ready>";
                    }
                    else
                    {
                        definitionString = ObjectiveDefinition.Title;
                    }
                    Log.Write(level, ObjectPrivate.Name + ":" + GetType().Name + " > '" + definitionString);
                    Log.Write(level, caller + " > " + message);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Exception logging quest objective details: " + ex.GetType().Name);
                Log.Write(ex.ToString());
            }
        }

        protected sealed override void SimpleInit()
        {
            Script.UnhandledException += UnhandledException;

            CancelDistanceSquared = CancelDistance * CancelDistance;
            
            if (ObjectiveDefinition == null)
            {
                QLog(LogLevel.Error, "Objective Definition not found.");
                return;
            }

            ObjectiveDefinition.Update(DefinitionUpdate);
        }

        public void UnhandledException(Object o, Exception e)
        {
            QLog(LogLevel.Error, "Unhandled exception: " + e.GetType().Name + "\n" + e.ToString());
        }

        protected abstract void InitObjective();

        void DefinitionUpdate(OperationCompleteEvent data)
        { 
            if (!data.Success || !ObjectiveDefinition.Ready)
            {
                QLog(LogLevel.Error, "Failed to update objective definition.");
                return;
            }

            InitObjective();

            ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);
            ScenePrivate.User.Subscribe(User.RemoveUser, OnRemoveUser);

            // for edge case where users can join scene before AddUser subscription is added
            foreach (var agent in ScenePrivate.GetAgents())
            {
                try
                {
                    OnAddAgent(agent.AgentInfo.SessionId);
                }
                catch (Exception) { }
            }
        }

        protected void CollectObjective(SessionId session, int index = -1)
        {
            Objective objective = null;
            if (!activeSessionIds.TryGetValue(session, out objective) || objective == null)
            {
                ObjectiveDefinition.GetObjective(session, (data) => CollectObjectiveData(data, session, index));
            }
            else
            {
                CollectObjectiveData(objective, session, index);
            }
        }

        void CollectObjectiveData(ObjectiveDefinition.GetObjectiveData data, SessionId session, int index)
        {
            try
            {
                if (data.Success)
                {
                    OnAddAgentData(data.Objective, session);
                    CollectObjectiveData(data.Objective, session, index);
                }
                else
                {
                    QLog(LogLevel.Warning, "Failed to get objective.", session);
                }
            }
            catch (Exception e)
            {
                QLog(LogLevel.Warning, "Exception getting objective. " + e.GetType().Name, session);
            }
        }

        protected void CollectObjectiveData(Objective objective, SessionId session, int index = -1)
        {
            AgentPrivate ap = ScenePrivate.FindAgent(session);
            if (ap == null || !objective.IsValid)
            {
                QLog(LogLevel.Info, "Stale session information.", session);
                activeSessionIds.Remove(session);
                return;
            }

            try
            {
                if (objective.GetState() != ObjectiveState.Active)
                {
                    QLog(LogLevel.Info, "Trying to collect non-active objective.", session);
                    return;
                }

                if (CollectTime == 0)
                {
                    OnObjectiveCollectStarted(ap.AgentInfo, objective, index);
                    FinalizeCollectObjective(ap.AgentInfo, objective, index);
                    return;
                }

                if (ap.Client.UI.GetProgressBars().Count() == 0)
                {
                    var bar = ap.Client.UI.AddProgressBar();

                    OnObjectiveCollectStarted(ap.AgentInfo, objective, index);
                    bar.Start(CollectText, CollectTime, CollectColor, (data) =>
                    {
                        try
                        {
                            if (data.Success)
                                FinalizeCollectObjective(ap.AgentInfo, objective, index);
                            else
                                OnObjectiveCollectCanceled(ap.AgentInfo, objective, index);
                        }
                        catch(Exception) { }
                    });
                    
                    if (CancelDistance > 0.001)
                    {
                        StartCoroutine(DistanceCheck, ap, bar, index);
                    }
                }
            }
            catch (Exception) { }
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
                                return;
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

        AgentInfo AgentFromObjective(Objective objective)
        {
            try
            {
                return ScenePrivate.FindAgent(objective.Agent)?.AgentInfo;
            }
            catch (Exception) { }
            return null;
        }

        void CollectedHint(Objective objective, int collectedSoFar)
        {
            try
            {
                string hint = null;
                if (objective.Definition.RequiredCount <= 1)
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

        private void OnRemoveUser(UserData data)
        {
            activeSessionIds.Remove(data.User);
        }

        void OnAddUser(UserData data)
        {
            OnAddAgent(data.User);
        }

        private void ForwardCallback(ObjectiveData d, Action<AgentInfo,Objective,bool> call)
        {
            try
            {
                var objective = activeSessionIds[d.AgentId];
                if (objective == null)
                {
                    QLog(LogLevel.Info, "Stale agent.", d.AgentId);
                    return;
                }

                var info = AgentFromObjective(objective);
                if (info != null && objective.IsValid)
                {
                    call(info, objective, false);
                }
                else
                {
                    QLog(LogLevel.Warning, "Invalid agent or objective.", d.AgentId);
                }
            }
            catch (NullReferenceException nre) when (nre.Message == "Null internal reference.")
            {
                QLog(LogLevel.Warning, "Null internal reference.", d.AgentId);
            }
            catch(Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().ToString() + " forwarding " + call.Target.ToString());
            }
        }

        protected void OnAddAgent(SessionId agentId)
        {
            if (agentId == SessionId.Invalid)
            {
                QLog(LogLevel.Error, "Invalid session id");
                return;
            }

            if (activeSessionIds.ContainsKey(agentId))
            {
                QLog(LogLevel.Error, "Already tracking this user", agentId);
                return;
            }

            AgentPrivate agent = ScenePrivate.FindAgent(agentId);
            if (agent == null || !agent.IsValid)
            {
                QLog(LogLevel.Warning, "Invalid or missing agent.");
                return;
            }

           ObjectiveDefinition.GetObjective(agentId, (data) =>
           {
               if (!data.Success || data.Objective == null)
               {
                   QLog(LogLevel.Error, "Failed to get Objective for user.", agentId);
                   return;
               }

               OnAddAgentData(data.Objective, agentId);
           });
        }

        void OnAddAgentData(Objective objective, SessionId agentId)
        {
            try
            {
                activeSessionIds[agentId] = objective;
                QLog(LogLevel.Info, $"Initial objective state: " + objective.GetState());
                
                objective.Subscribe(ObjectiveState.Active, (ObjectiveData d) => ForwardCallback(d, OnObjectiveActive));
                objective.Subscribe(ObjectiveState.Locked, (ObjectiveData d) => ForwardCallback(d, OnObjectiveLocked));
                objective.Subscribe(ObjectiveState.Completed, (ObjectiveData d) => ForwardCallback(d, OnObjectiveCompleted));
                objective.Subscribe(ObjectiveState.None, (ObjectiveData d) => ForwardCallback(d, OnObjectiveReset));

                AgentInfo agentInfo = ScenePrivate.FindAgent(agentId)?.AgentInfo;
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

                    case ObjectiveState.None:
                        OnObjectiveReset(agentInfo, objective, true);
                        break;

                    default:
                        break;
                }

                OnObjectiveJoinExperience(agentInfo, objective);
            }
            catch (Exception e)
            {
                QLog(LogLevel.Error, "Exception " + e.GetType().Name + ":\n" + e.ToString());
            }
        }

        protected virtual void OnObjectiveCollectCompleted(AgentInfo agentInfo, Objective objective, int newCount, int index = -1)
        {
            QLog(LogLevel.Info, "OnObjectiveCollectCompleted.", agentInfo.SessionId);
        }

        protected virtual void OnObjectiveCollectCanceled(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            QLog(LogLevel.Info, "OnObjectiveCollectCanceled.", agentInfo.SessionId);
        }

        protected virtual void OnObjectiveCollectStarted(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            QLog(LogLevel.Info, "OnObjectiveCollectStarted.", agentInfo.SessionId);
        }

        protected virtual void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
            QLog(LogLevel.Info, "OnObjectiveJoinExperience.", agentInfo.SessionId);
        }

        protected virtual void OnObjectiveActive(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            QLog(LogLevel.Info, "OnObjectiveActive. InitialJoin = " + initialJoin, agentInfo.SessionId);
        }

        protected virtual void OnObjectiveLocked(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            QLog(LogLevel.Info, "OnObjectiveLocked. InitialJoin = " + initialJoin, agentInfo.SessionId);
        }

        protected virtual void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            QLog(LogLevel.Info, "OnObjectiveCompleted. InitialJoin = " + initialJoin, agentInfo.SessionId);
        }

        protected virtual void OnObjectiveReset(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            QLog(LogLevel.Info, "OnObjectiveReset. InitialJoin = " + initialJoin, agentInfo.SessionId);
        }

        protected SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData data = new SimpleData(this);
            data.AgentInfo = agentInfo;
            data.ObjectId = agentInfo != null ? agentInfo.ObjectId : ObjectPrivate.ObjectId;
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

