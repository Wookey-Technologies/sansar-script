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

namespace CustomQuestScripts
{
    public abstract class QuestVisibilityBase : SceneObjectScript
    {
        [DisplayName("Toggle Collisions")]
        [Tooltip("Toggle Collisions\nIf enabled then the object's collision behavior will match that of the visibility. This will affect ALL volumes on any object with ANY meshes that change visibility.")]
        [DefaultValue(true)]
        public bool ToggleCollisions = false;
        protected abstract List<MeshComponent> ShowMeshes { get; }
        protected abstract List<MeshComponent> HideMeshes { get; }

        private List<MeshComponent> ScriptableShowMeshes = new List<MeshComponent>();
        private List<MeshComponent> ScriptableHideMeshes = new List<MeshComponent>();
        private List<RigidBodyComponent> ShowColliders = new List<RigidBodyComponent>();
        private List<RigidBodyComponent> HideColliders = new List<RigidBodyComponent>();

        public override void Init()
        {
            Script.UnhandledException += Script_UnhandledException;

            foreach (var mesh in ShowMeshes)
            {
                if (mesh == null || !mesh.IsValid) continue;

                if (mesh.IsScriptable)
                {
                    ScriptableShowMeshes.Add(mesh);
                    mesh.SetIsVisible(false);

                    findColliders(ShowColliders, mesh.ComponentId.ObjectId);
                }
                else
                {
                    Log.Write(LogLevel.Error, "The mesh " + mesh.Name + " is not scriptable and will not have it's visibility changed.");
                }
            }

            foreach (var mesh in HideMeshes)
            {
                if (mesh == null || !mesh.IsValid) continue;

                if (mesh.IsScriptable)
                {
                    ScriptableHideMeshes.Add(mesh);
                    mesh.SetIsVisible(true);

                    findColliders(HideColliders, mesh.ComponentId.ObjectId);
                }
                else
                {
                    Log.Write(LogLevel.Error, "The mesh " + mesh.Name + " is not scriptable and will not have it's visibility changed.");
                }
            }

            if (ScriptableShowMeshes.Count == 0 && ScriptableHideMeshes.Count == 0)
            {
                for (uint index = 0; index < ObjectPrivate.GetComponentCount(MeshComponent.ComponentType); ++index)
                {
                    MeshComponent mesh = ObjectPrivate.GetComponent(MeshComponent.ComponentType, index) as MeshComponent;
                    if (mesh == null || !mesh.IsValid) continue;
                    if (mesh.IsScriptable)
                    {
                        ScriptableShowMeshes.Add(mesh);
                    }
                }
            }

            if (ScriptableShowMeshes.Count == 0 && ScriptableHideMeshes.Count == 0)
            {
                Log.Write(LogLevel.Error, "No scriptable meshes set (and this object's meshes are not scriptable) on Quest Visibility script for object " + ObjectPrivate.Name);
                return;
            }

            InitVisibility();
        }

        protected void Setup(OperationCompleteEvent data)
        {
            if (data != null && data.Success == false)
            {
                Log.Write("Error fetching QuestDefinition for QuestVisibility script for quest on object " + ObjectPrivate.Name);
                return;
            }

            foreach (var agent in ScenePrivate.GetAgents())
            {
                OnAddUser(agent);
            }

            ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);
            ScenePrivate.User.Subscribe(User.RemoveUser, (userData) => VisibleToSessions.Remove(userData.User));
        }

        private void Script_UnhandledException(object o, Exception e)
        {
            Log.Write(LogLevel.Error, "ERROR", "Uncaught exception '" + e.GetType().Name + "' on QuestVisibility script! ");
        }

        private void findColliders(List<RigidBodyComponent> colliders, ObjectId id)
        {
            if (!ToggleCollisions) return;

            ObjectPrivate op = ScenePrivate.FindObject(id);
            if (op != null)
            {
                for (uint rbi = 0; rbi < op.GetComponentCount(ComponentType.RigidBodyComponent); ++rbi)
                {
                    if (op.TryGetComponent(rbi, out RigidBodyComponent rb))
                    {
                        if (rb != null)
                        {
                            if (colliders.Contains(rb)) return;
                            colliders.Add(rb);
                        }
                    }
                }
            }
        }

        public abstract void InitVisibility();

        void OnAddUser(UserData data) { OnAddUser(ScenePrivate.FindAgent(data.User)); }
        void OnRemoveUser(UserData data)
        {
            VisibleToSessions.Remove(data.User);
            OnRemoveUser(ScenePrivate.FindAgent(data.User));
        }

        public abstract void OnAddUser(AgentPrivate agent);
        public abstract void OnRemoveUser(AgentPrivate agent);

        HashSet<SessionId> VisibleToSessions = new HashSet<SessionId>();

        protected void SetVisibility(SessionId id, bool visible, bool isJoin)
        {
            try
            {
                bool IsVisible = VisibleToSessions.Contains(id);
                if (visible != IsVisible || isJoin)
                {
                    if (visible) VisibleToSessions.Add(id);
                    else VisibleToSessions.Remove(id);

                    foreach (var mesh in ScriptableShowMeshes)
                    {
                        mesh.SetIsVisible(id, visible);
                    }

                    foreach (var mesh in ScriptableHideMeshes)
                    {
                        mesh.SetIsVisible(id, !visible);
                    }

                    if (ToggleCollisions)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(id);
                        if (agent != null && agent.IsValid)
                        {
                            foreach (var rb in ShowColliders)
                            {
                                agent.IgnoreCollisionWith(rb, !visible);
                            }

                            foreach (var rb in HideColliders)
                            {
                                agent.IgnoreCollisionWith(rb, visible);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Log.Write(LogLevel.Warning, "Error setting mesh visibility for quest");
            }
        }
    }

    public class CharacterIndicators : SceneObjectScript
    {
        [DisplayName("Character")]
        [Tooltip("Internal Use Only\nUse the character interface for the list of definitions.")]
        public QuestCharacter Character;

        [Tooltip("These meshes will be visible if all non-completed character quests are Active")]
        public List<MeshComponent> OnlyActive;

        [Tooltip("These meshes will be visible if all non-completed character quests are Available")]
        public List<MeshComponent> OnlyAvailable;

        [Tooltip("These meshes will be visible if all non-completed character quests are Can Turn In")]
        public List<MeshComponent> OnlyCanTurnIn;

        [Tooltip("These meshes will be visible if all non-completed character quests are either Active Or Available")]
        public List<MeshComponent> ActiveAndAvailable;

        [Tooltip("These meshes will be visible if all non-completed character quests are either Active or Can Turn In")]
        public List<MeshComponent> ActiveAndCanTurnIn;

        [Tooltip("These meshes will be visible if all non-completed character quests are either Available or Can Turn In")]
        public List<MeshComponent> AvailableAndCanTurnIn;

        [Tooltip("These meshes will be visible if there are non-completed character quests are in all three states.")]
        [DisplayName("All")]
        public List<MeshComponent> All;

        [Tooltip("These meshes will be visible if there are no non-completed character quests.")]
        [DisplayName("No Tracked Quests")]
        public List<MeshComponent> None;

        enum State
        {
            OnlyActive,
            OnlyAvailable,
            OnlyCanTurnIn,
            ActiveAndAvailable,
            ActiveAndCanTurnIn,
            AvailableAndCanTurnIn,
            All,
            None
        }

        Dictionary<SessionId, State> PerUserState = new Dictionary<SessionId, State>();
        Dictionary<State, List<MeshComponent>> MeshMap = new Dictionary<State, List<MeshComponent>>();

        void SetVisibility(List<MeshComponent> meshes, SessionId session, bool visible)
        {
            foreach (var mesh in meshes)
            {
                if (mesh != null && mesh.IsValid && mesh.IsScriptable)
                {
                    try
                    {
                        mesh.SetIsVisible(session, visible);
                    }
                    catch { }
                }
            }
        }

        void Update(SessionId session, State state)
        {
            if (PerUserState.TryGetValue(session, out State currentState))
            {
                if (currentState == state) return;

                SetVisibility(MeshMap[currentState], session, false);
                SetVisibility(MeshMap[state], session, true);
                PerUserState[session] = state;
            }
            else
            {
                PerUserState[session] = state;

                foreach (var meshes in MeshMap)
                {
                    if (meshes.Key != state) SetVisibility(meshes.Value, session, false);
                }
                SetVisibility(MeshMap[state], session, true);
                return;
            }
        }

        public override void Init()
        {
            MeshMap[State.OnlyActive] = OnlyActive;
            MeshMap[State.OnlyAvailable] = OnlyAvailable;
            MeshMap[State.OnlyCanTurnIn] = OnlyCanTurnIn;
            MeshMap[State.ActiveAndAvailable] = ActiveAndAvailable;
            MeshMap[State.ActiveAndCanTurnIn] = ActiveAndCanTurnIn;
            MeshMap[State.AvailableAndCanTurnIn] = AvailableAndCanTurnIn;
            MeshMap[State.All] = All;
            MeshMap[State.None] = None;

            ScenePrivate.User.Subscribe(User.RemoveUser, (userData) => OnRemoveUser(userData.User));
            ScenePrivate.User.Subscribe(User.AddUser, (userData) => OnAddUser(userData.User));
            foreach (var agent in ScenePrivate.GetAgents())
            {
                OnAddUser(agent.AgentInfo.SessionId);
            }
        }

        public void OnRemoveUser(SessionId session)
        {
            PerUserState.Remove(session);
        }

        public void OnAddUser(SessionId session)
        {
            try
            {
                var tracker = Character.GetCharacterTracker(session);
                tracker.Update((data) => HandleCharacterUpdate(data, tracker));
            }
            catch { }
        }

        void HandleCharacterUpdate(OperationCompleteEvent data, CharacterTracker tracker)
        {
            if (data.Success)
            {
                tracker.Subscribe((subdata) => HandleCharacterSubscribe(subdata, tracker));
            }
            else
            {
                Log.Write(LogLevel.Warning, "Error updating quest tracker for user.");
            }
        }

        void HandleCharacterSubscribe(CharacterTrackerData data, CharacterTracker tracker)
        {
            if (data.SessionId != SessionId.Invalid && tracker.IsValid)
            {
                HandleCharacterSubscribe(data.SessionId, tracker);
            }
        }

        void HandleCharacterSubscribe(SessionId session, CharacterTracker tracker)
        {
            int activeCount = tracker.ActiveQuestCount;
            int availableCount = tracker.AvailableQuestCount;
            int turnInCount = tracker.CanTurnInQuestCount;
            int totalCount = activeCount + availableCount + turnInCount;

            State newState = State.All;
            if (totalCount == 0)
            {
                newState = State.None;
            }
            else if (activeCount == totalCount)
            {
                newState = State.OnlyActive;
            }
            else if (availableCount == totalCount)
            {
                newState = State.OnlyAvailable;
            }
            else if (turnInCount == totalCount)
            {
                newState = State.OnlyCanTurnIn;
            }
            else if (turnInCount == 0)
            {
                newState = State.ActiveAndAvailable;
            }
            else if (availableCount == 0)
            {
                newState = State.ActiveAndCanTurnIn;
            }
            else if (activeCount == 0)
            {
                newState = State.AvailableAndCanTurnIn;
            }

            StartCoroutine(Update, session, newState);
        }
    }

    [Tooltip("Internal Use Only: set visibility based on character data.")]
    public class CharacterVisibility : QuestVisibilityBase
    {
        #region EditorProperties
        [DisplayName("Character")]
        [Tooltip("Internal Use Only\nUse the character interface for the list of definitions.")]
        public QuestCharacter Character;

        [Tooltip("Set to all meshes that should SHOW based on the quest objective state. They will all change together.\nIf left empty the script will change the visibility for all meshes on the same object as the script.")]
        [DisplayName("Show Meshes")]
        public List<MeshComponent> _ShowMeshes;
        protected override List<MeshComponent> ShowMeshes { get { return _ShowMeshes; } }

        [Tooltip("Set to all meshes that should HIDE based on the quest objective state. They will all change together.\nThese meshes will show ONLY when the meshes in Show Meshes are hidden.")]
        [DisplayName("Hide Meshes")]
        public List<MeshComponent> _HideMeshes;
        protected override List<MeshComponent> HideMeshes { get { return _HideMeshes; } }

        [DisplayName("If Any")]
        [Tooltip("If enabled will set visibility according to the below options if any non-completed quests on this character match the specified state. If there is only 1 non-completed quest on this character Any and All work the same.")]
        [DefaultValue(true)]
        public bool ShowOnAny;

        [DisplayName("If All")]
        [Tooltip("If enabled will set visibility according to the below options only when all non-completed quests on this character match the specified state. If there is only 1 non-completed quest on this character Any and All work the same.")]
        public bool ShowOnAll;

        [DisplayName("Show On Active")]
        [Tooltip("Show On Active\nShow if the quests are Active for the user on this Character.")]
        public bool VisibleOnActive;

        [DisplayName("Show On Available")]
        [Tooltip("Show On Available\nShow if the quests are Available for the user on this Character.")]
        [DefaultValue(true)]
        public bool VisibleOnAvailable;

        [DisplayName("Show On Can Turn In")]
        [Tooltip("Show On Can Turn In\nShow if the quests can be turned in for the user on this Character.")]
        public bool VisibleOnCanTurnIn;

        [DisplayName("Show On None")]
        [Tooltip("Show On None\nA special case for characters. Regardless of If Any and If All settings, if Show On None is true the meshes will be set visible if there are no quests in Available, Active or Can Turn In for this user on this character.")]
        public bool VisibleOnNone;
        #endregion

        public override void InitVisibility()
        {
            if (Character == null || !Character.IsValid)
            {
                Log.Write(LogLevel.Error, "The Character set on the Visibility script on object " + ObjectPrivate.Name + " is null. You must set a character to use this script.");
                return;
            }

            Setup(null);
        }

        public override void OnRemoveUser(AgentPrivate agent)
        {
        }

        public override void OnAddUser(AgentPrivate agent)
        {
            try
            {
                var tracker = Character.GetCharacterTracker(agent.AgentInfo.SessionId);
                tracker.Update(HandleCharacterUpdate);
                tracker.Subscribe((data) => HandleCharacterSubscribe(data, tracker));
            }
            catch (Exception)
            {
                if (agent != null)
                { // The following are cached and okay to access as long as agent itself isn't null
                    Log.Write(LogLevel.Warning, "Error getting quest state for character for user " + agent.AgentInfo.Name + " : " + agent.AgentInfo.AvatarUuid);
                }
                else
                {
                    Log.Write(LogLevel.Warning, "Error getting quest state for character: agent is null");
                }
            }
        }

        void HandleCharacterUpdate(OperationCompleteEvent data)
        {
            if (data.Success == false)
            {
                Log.Write(LogLevel.Warning, "Error updating quest tracker for user.");
            }
        }

        void HandleCharacterSubscribe(CharacterTrackerData data, CharacterTracker tracker)
        {
            if (data.SessionId != SessionId.Invalid && tracker.IsValid)
            {
                HandleCharacterSubscribe(data.SessionId, tracker, false);
            }
        }

        void HandleCharacterSubscribe(SessionId agent, CharacterTracker tracker, bool isJoin = true)
        {
            bool visible = false;

            int activeCount = tracker.ActiveQuestCount;
            int availableCount = tracker.AvailableQuestCount;
            int turnInCount = tracker.CanTurnInQuestCount;
            int totalCount = activeCount + availableCount + turnInCount;

            if (ShowOnAny)
            {
                visible |= VisibleOnActive & activeCount > 0;
                visible |= VisibleOnAvailable & availableCount > 0;
                visible |= VisibleOnCanTurnIn & turnInCount > 0;
            }
            else if (ShowOnAll)
            {
                visible |= VisibleOnActive & activeCount == totalCount;
                visible |= VisibleOnAvailable & availableCount == totalCount;
                visible |= VisibleOnCanTurnIn & turnInCount == totalCount;
            }
            else
            {
                visible |= VisibleOnActive & activeCount == 0;
                visible |= VisibleOnAvailable & availableCount == 0;
                visible |= VisibleOnCanTurnIn & turnInCount == 0;

            }
            visible |= VisibleOnNone && totalCount == 0;

            SetVisibility(agent, visible, isJoin);
        }
    }


    public class QuestVisibility : QuestVisibilityBase
    {
        #region EditorProperties
        [DisplayName("Quests")]
        public List<QuestDefinition> Definitions;

        [Tooltip("Set to all meshes that should SHOW based on the quest objective state. They will all change together.\nIf left empty the script will change the visibility for all meshes on the same object as the script.")]
        [DisplayName("Show Meshes")]
        public List<MeshComponent> _ShowMeshes;
        protected override List<MeshComponent> ShowMeshes { get { return _ShowMeshes; } }

        [Tooltip("Set to all meshes that should HIDE based on the quest objective state. They will all change together.\nThese meshes will show ONLY when the meshes in Show Meshes are hidden.")]
        [DisplayName("Hide Meshes")]
        public List<MeshComponent> _HideMeshes;
        protected override List<MeshComponent> HideMeshes { get { return _HideMeshes; } }

        [DisplayName("If Any")]
        [Tooltip("If enabled will set visibility according to the below options if any quests match the specified state. If there is only 1 quest Any and All work the same.")]
        [DefaultValue(true)]
        public bool ShowOnAny;

        [DisplayName("If All")]
        [Tooltip("If enabled will set visibility according to the below options only when all quests match the specified state. If there is only 1 quest Any and All work the same.")]
        public bool ShowOnAll;

        [DisplayName("Show On Offered")]
        [Tooltip("Show On Offered\nShow if the quests are Offered for the user.")]
        public bool VisibleOnOffered;

        [DisplayName("Show On Active")]
        [Tooltip("Show On Active\nShow if the quests are Active for the user.")]
        public bool VisibleOnActive;

        [DisplayName("Show On Completed")]
        [Tooltip("Show On Offered\nShow if the quests are Completed for the user.")]
        public bool VisibleOnCompleted;

        [DisplayName("Show On Objectives Completed")]
        [Tooltip("Show On Offered\nShow if the quests have all objectives completed for the user.")]
        [DefaultValue(true)]
        public bool VisibleOnObjectivesCompleted;

        [DisplayName("Show On Select Reward")]
        [Tooltip("Show On Select Reward\nShow if the quests are in 'Select Reward' state for the user.")]
        [DefaultValue(true)]
        public bool VisibleOnSelectReward;

        [DisplayName("Show On None")]
        [Tooltip("Show On None\nShow if the quests are in state None: this indicates the quest has not yet been offered to the user.")]
        public bool VisibleOnNone;
        #endregion

        class Status
        {
            public uint Offered = 0;
            public uint Active = 0;
            public uint Completed = 0;
            public uint ObjectivesCompleted = 0;
            public uint SelectReward = 0;
            public uint None = 0;
            public Action Unsubscribe = null;
        }
        private Dictionary<SessionId, Status> PerUserStatus = new Dictionary<SessionId, Status>();
        private uint All = 0;

        public override void InitVisibility()
        {
            if (Definitions.Count == 0)
            {
                Log.Write(LogLevel.Error, "The definition set on the Visibility script on object " + ObjectPrivate.Name + " is null. You must set an quest to use this script.");
                return;
            }

            int defCount = 0;
            for (int i = 0; i < Definitions.Count; ++i)
            {
                QuestDefinition def = Definitions[i];
                if (def != null)
                {
                    defCount++;
                    int Index = i;
                    def.Update((data) =>
                    {
                        if (!data.Success)
                        {
                            Log.Write(LogLevel.Error, "Error getting objective data.");
                            return;
                        }
                        defCount--;
                        All = All | (uint)(0x1 << Index);
                        if (defCount == 0) Setup(data);
                    });
                }
                else
                {
                    Log.Write(LogLevel.Error, "A definition set on the Visibility script on object " + ObjectPrivate.Name + " is null.");
                }
            }
        }

        public override void OnRemoveUser(AgentPrivate agent)
        {
            try
            {
                if (PerUserStatus.TryGetValue(agent.AgentInfo.SessionId, out Status status))
                {
                    status.Unsubscribe?.Invoke();
                    status.Unsubscribe = null;
                }
                PerUserStatus.Remove(agent.AgentInfo.SessionId);
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Warning, "Exception in OnRemoveUser " + e.GetType().Name);
            }
        }

        public override void OnAddUser(AgentPrivate agent)
        {
            try
            {
                if (!PerUserStatus.ContainsKey(agent.AgentInfo.SessionId))
                {
                    PerUserStatus[agent.AgentInfo.SessionId] = new Status();
                }

                for (int i = 0; i < Definitions.Count; ++i)
                {
                    QuestDefinition def = Definitions[i];
                    if (!def.IsValid || !def.Ready)
                    {
                        Log.Write(LogLevel.Warning, "Error getting quest state for quest for user " + agent.AgentInfo.Name + " : " + agent.AgentInfo.AvatarUuid);
                    }
                    int Index = i;
                    if (agent != null && agent.IsValid) def.GetQuest(agent, (data) => HandleQuest(data, Index));
                }
            }
            catch (Exception)
            {
                if (agent != null)
                { // The following are cached and okay to access as long as agent itself isn't null
                    Log.Write(LogLevel.Warning, "Error getting quest state for quest for user " + agent.AgentInfo.Name + " : " + agent.AgentInfo.AvatarUuid);
                }
                else
                {
                    Log.Write(LogLevel.Warning, "Error getting quest state for quest: agent is null");
                }
            }
        }

        void HandleQuest(QuestDefinition.GetQuestData data, int index)
        {
            try
            {
                if (data.Success == false || data.Quest == null || !data.Quest.IsValid)
                {
                    Log.Write("Error fetching QuestDefinition for OQuestVisibility script for quest on object " + ObjectPrivate.Name);
                    return;
                }

                Status status = null;
                if (!PerUserStatus.TryGetValue(data.Quest.Agent, out status))
                {
                    PerUserStatus[data.Quest.Agent] = new Status();
                }

                QuestStateChange(data.Quest.GetState(), data.Quest.Agent, index);
                status.Unsubscribe += data.Quest.Subscribe(QuestState.Active, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Quest.Subscribe(QuestState.Completed, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Quest.Subscribe(QuestState.None, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Quest.Subscribe(QuestState.ObjectivesCompleted, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Quest.Subscribe(QuestState.Offered, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Quest.Subscribe(QuestState.SelectReward, (stateData) => QuestStateChange(stateData, index)).Unsubscribe;
            }
            catch (Exception)
            {
                Log.Write(LogLevel.Error, "Error subscribing to quest state changes.");
            }
        }


        void QuestStateChange(QuestData data, int index) { QuestStateChange(data.State, data.AgentId, index, false); }
        void QuestStateChange(QuestState state, SessionId agent, int index, bool isJoin = true)
        {
            try
            {
                uint flag = (uint)(0x1 << index);
                Status status = null;
                if (!PerUserStatus.TryGetValue(agent, out status))
                {
                    return;
                }

                status.Offered &= ~flag;
                status.Active &= ~flag;
                status.Completed &= ~flag;
                status.ObjectivesCompleted &= ~flag;
                status.SelectReward &= ~flag;
                status.None &= ~flag;

                switch (state)
                {
                    case QuestState.Offered: status.Offered |= flag; break;
                    case QuestState.Active: status.Active |= flag; break;
                    case QuestState.Completed: status.Completed |= flag; break;
                    case QuestState.ObjectivesCompleted: status.ObjectivesCompleted |= flag; break;
                    case QuestState.SelectReward: status.SelectReward |= flag; break;
                    case QuestState.None: status.None |= flag; break;
                    default:
                        break;
                }

                bool visible = false;
                if (ShowOnAny)
                {
                    visible |= (VisibleOnOffered & status.Offered != 0);
                    visible |= (VisibleOnActive & status.Active != 0);
                    visible |= (VisibleOnCompleted & status.Completed != 0);
                    visible |= (VisibleOnObjectivesCompleted & status.ObjectivesCompleted != 0);
                    visible |= (VisibleOnSelectReward & status.SelectReward != 0);
                    visible |= (VisibleOnNone & status.None != 0);
                }
                else if (ShowOnAll)
                {
                    visible |= (VisibleOnOffered & status.Offered == All);
                    visible |= (VisibleOnActive & status.Active == All);
                    visible |= (VisibleOnCompleted & status.Completed == All);
                    visible |= (VisibleOnObjectivesCompleted & status.ObjectivesCompleted == All);
                    visible |= (VisibleOnSelectReward & status.SelectReward == All);
                    visible |= (VisibleOnNone & status.None == All);
                }
                else
                {
                    visible |= (VisibleOnOffered & status.Offered == 0);
                    visible |= (VisibleOnActive & status.Active == 0);
                    visible |= (VisibleOnCompleted & status.Completed == 0);
                    visible |= (VisibleOnObjectivesCompleted & status.ObjectivesCompleted == 0);
                    visible |= (VisibleOnSelectReward & status.SelectReward == 0);
                    visible |= (VisibleOnNone & status.None == 0);
                }

                SetVisibility(agent, visible, isJoin);
            }
            catch { }
        }
    }

    public class ObjectiveVisibility : QuestVisibilityBase
    {
        #region EditorProperties
        [DisplayName("Objectives")]
        public List<ObjectiveDefinition> Definitions;

        [Tooltip("Set to all meshes that should SHOW based on the quest objective state. They will all change together.\nIf left empty the script will change the visibility for all meshes on the same object as the script.")]
        [DisplayName("Show Meshes")]
        public List<MeshComponent> _ShowMeshes;
        protected override List<MeshComponent> ShowMeshes { get { return _ShowMeshes; } }

        [Tooltip("Set to all meshes that should HIDE based on the quest objective state. They will all change together.\nThese meshes will show ONLY when the meshes in Show Meshes are hidden.")]
        [DisplayName("Hide Meshes")]
        public List<MeshComponent> _HideMeshes;
        protected override List<MeshComponent> HideMeshes { get { return _HideMeshes; } }

        [DisplayName("If Any")]
        [Tooltip("If enabled will set visibility according to the below options if any objectives match the specified state. If there is only 1 objective Any and All work the same.")]
        [DefaultValue(true)]
        public bool ShowOnAny;

        [DisplayName("If All")]
        [Tooltip("If enabled will set visibility according to the below options only if all objectives match the specified state. If there is only 1 objective Any and All work the same.")]
        public bool ShowOnAll;

        [DisplayName("On Active")]
        [Tooltip("Show On Active\nShow if the objectives are Active for the user.")]
        [DefaultValue(true)]
        public bool VisibleOnActive;

        [DisplayName("On Completed")]
        [Tooltip("Show On Completed\nShow if the objectives are Completed for the user.")]
        public bool VisibleOnCompleted;

        [DisplayName("On Locked")]
        [Tooltip("Show On Locked\nShow if the objectives are Locked for the user: locked objectives are disabled and cannot be completed.")]
        public bool VisibleOnLocked;

        [DisplayName("On None")]
        [Tooltip("Show On None\nShow if the objectives are None for the user: this usually means the user has not been offered this quest yet.")]
        public bool VisibleOnNone;

        #endregion

        class Status
        {
            public uint Active = 0;
            public uint Completed = 0;
            public uint Locked = 0;
            public uint None = 0;
            public Action Unsubscribe = null;
        }
        private Dictionary<SessionId, Status> PerUserStatus = new Dictionary<SessionId, Status>();

        private uint All = 0;
        public override void InitVisibility()
        {
            if (Definitions.Count == 0)
            {
                Log.Write(LogLevel.Error, "Must add Objectives to track.");
                return;
            }

            int defCount = 0;
            for (int i = 0; i < Definitions.Count; ++i)
            {
                ObjectiveDefinition def = Definitions[i];
                if (def != null)
                {
                    defCount++;
                    int Index = i;
                    def.Update((data) =>
                    {
                        if (!data.Success)
                        {
                            Log.Write(LogLevel.Error, "Error getting objective data.");
                            return;
                        }
                        defCount--;
                        All = All | (uint)(0x1 << Index);
                        if (defCount == 0) Setup(data);
                    });
                }
                else
                {
                    Log.Write(LogLevel.Error, "A definition set on the Visibility script on object " + ObjectPrivate.Name + " is null.");
                }
            }
        }

        public override void OnRemoveUser(AgentPrivate agent)
        {
            try
            {
                if (PerUserStatus.TryGetValue(agent.AgentInfo.SessionId, out Status status))
                {
                    status.Unsubscribe?.Invoke();
                    status.Unsubscribe = null;
                }
                PerUserStatus.Remove(agent.AgentInfo.SessionId);
            }
            catch { }
        }

        public override void OnAddUser(AgentPrivate agent)
        {
            try
            {
                if (!PerUserStatus.ContainsKey(agent.AgentInfo.SessionId))
                {
                    PerUserStatus[agent.AgentInfo.SessionId] = new Status();
                }

                for (int i = 0; i < Definitions.Count; ++i)
                {
                    ObjectiveDefinition def = Definitions[i];
                    if (!def.IsValid || !def.Ready)
                    {
                        Log.Write(LogLevel.Warning, "Error getting quest state for quest for user " + agent.AgentInfo.Name + " : " + agent.AgentInfo.AvatarUuid);
                    }
                    int Index = i;
                    if (agent != null && agent.IsValid) def.GetObjective(agent, (data) => HandleObjective(data, Index));
                }
            }
            catch (Exception)
            {
                if (agent != null)
                {   // The following are cached and okay to access as long as agent itself isn't null
                    Log.Write(LogLevel.Warning, "Error getting quest state for quest for user " + agent.AgentInfo.Name + " : " + agent.AgentInfo.AvatarUuid);
                }
                else
                {
                    Log.Write(LogLevel.Warning, "Error getting quest state for quest agent is null");
                }
            }
        }

        void HandleObjective(ObjectiveDefinition.GetObjectiveData data, int index)
        {
            try
            {
                if (data.Success == false || data.Objective == null || !data.Objective.IsValid)
                {
                    Log.Write("Error fetching QuestDefinition for QuestVisibility script for quest on object " + ObjectPrivate.Name);
                    return;
                }

                Status status = null;
                if (!PerUserStatus.TryGetValue(data.Objective.Agent, out status))
                {
                    PerUserStatus[data.Objective.Agent] = new Status();
                }

                ObjectiveStateChange(data.Objective.GetState(), data.Objective.Agent, index);
                status.Unsubscribe += data.Objective.Subscribe(ObjectiveState.Active, (stateData) => ObjectiveStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Objective.Subscribe(ObjectiveState.Completed, (stateData) => ObjectiveStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Objective.Subscribe(ObjectiveState.None, (stateData) => ObjectiveStateChange(stateData, index)).Unsubscribe;
                status.Unsubscribe += data.Objective.Subscribe(ObjectiveState.Locked, (stateData) => ObjectiveStateChange(stateData, index)).Unsubscribe;
            }
            catch (Exception)
            {
                Log.Write(LogLevel.Error, "Error subscribing to quest state changes.");
            }
        }

        void ObjectiveStateChange(ObjectiveData data, int index) { ObjectiveStateChange(data.State, data.AgentId, index, false); }
        void ObjectiveStateChange(ObjectiveState state, SessionId agent, int index, bool isJoin = true)
        {
            try
            {
                uint setFlag = (uint)(0x1 << index);
                Status status = null;
                if (!PerUserStatus.TryGetValue(agent, out status))
                {
                    if (isJoin)
                    {
                        status = new Status();
                        PerUserStatus[agent] = status;
                    }
                    else
                    {
                        return;
                    }
                }

                status.Active &= ~setFlag;
                status.Completed &= ~setFlag;
                status.Locked &= ~setFlag;
                status.None &= ~setFlag;

                switch (state)
                {
                    case ObjectiveState.Active: status.Active |= setFlag; break;
                    case ObjectiveState.Completed: status.Completed |= setFlag; break;
                    case ObjectiveState.Locked: status.Locked |= setFlag; break;
                    case ObjectiveState.None: status.None |= setFlag; break;
                    default: break;
                }

                bool visible = false;
                if (ShowOnAny)
                {
                    visible |= (VisibleOnActive & status.Active != 0);
                    visible |= (VisibleOnCompleted & status.Completed != 0);
                    visible |= (VisibleOnLocked & status.Locked != 0);
                    visible |= (VisibleOnNone & status.None != 0);
                }
                else if (ShowOnAll)
                {
                    visible |= (VisibleOnActive & status.Active == All);
                    visible |= (VisibleOnCompleted & status.Completed == All);
                    visible |= (VisibleOnLocked & status.Locked == All);
                    visible |= (VisibleOnNone & status.None == All);
                }
                else
                {
                    visible |= (VisibleOnActive & status.Active == 0);
                    visible |= (VisibleOnCompleted & status.Completed == 0);
                    visible |= (VisibleOnLocked & status.Locked == 0);
                    visible |= (VisibleOnNone & status.None == 0);
                }

                SetVisibility(agent, visible, isJoin);
            }
            catch { }
        }
    }
}