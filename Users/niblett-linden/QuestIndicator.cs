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
    [Tooltip("Controls the visibilty of an object based on quest status.")]
    [DisplayName("Quest Indicator")]
    public class QuestIndicator : SceneObjectScript
    {
        #region EditorProperties
        [Tooltip(@"The quest definition.")]
        [DisplayName("Quest")]
        public readonly QuestDefinition QuestDefinition;

        [Tooltip(@"If an inventory object is used show as the quest indicator. If this is not set, the object that the script is attached to will be used. In either case, the object's Mesh Component must have IsScriptable set to 'On'")]
        [DisplayName("Quest Indicator")]
        public ClusterResource IndicatorResource;

        [Tooltip(@"If 'Quest Indicator' is set to an inventory object, it will be shown offset from this object by this distance.")]
        [DisplayName("Indicator Offset")]
        [DefaultValue(0.0f, 0.0f, 1.0f)]
        public Vector IndicatorOffset;

        [Tooltip(@"Show the Quest Indicator when the quest is available to be offered to the user.")]
        [DefaultValue(true)]
        public bool ShowWhenAvailable;

        [Tooltip(@"Show the Quest Indicator when user is on the quest.")]
        [DefaultValue(false)]
        public bool ShowWhenActive;

        [Tooltip(@"Show the Quest Indicator when user has completed the quest.")]
        [DefaultValue(false)]
        public bool ShowWhenComplete;

        #endregion

        MeshComponent indicatorMeshComponent = null;
        bool initSuccess = false;
        bool initFail = false;

        public override void Init()
        {
            if (QuestDefinition == null)
            {
                Log.Write(LogLevel.Error, "Quest Definition not found.");
                initFail = true;
                return;
            }

            ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);

            if (IndicatorResource == null)
            {
               ObjectPrivate.TryGetFirstComponent<MeshComponent>(out indicatorMeshComponent);
            }
            else
            {
                ScenePrivate.CreateClusterData indicatorClusterData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster,
                IndicatorResource,
                ObjectPrivate.Position + IndicatorOffset,
                ObjectPrivate.Rotation,
                Vector.Zero);

                if (indicatorClusterData.Success)
                {
                    indicatorClusterData.ClusterReference.GetObjectPrivate(0).TryGetFirstComponent<MeshComponent>(out indicatorMeshComponent);
                }
            }

            if (indicatorMeshComponent == null || !indicatorMeshComponent.IsScriptable)
            {
                Log.Write(LogLevel.Error, "Quest Indicator failed to obtain a scriptable MeshComponent.");
                initFail = true;
                return;
            }

            var update = WaitFor(QuestDefinition.Update);

            if (update.Success)
            {
                Log.Write(LogLevel.Info, $"Got quest definition: {QuestDefinition.Title}");
            }
            else
            {
                Log.Write(LogLevel.Error, "Failed to update quest definition.");
                initFail = true;
                return;
            }

            initSuccess = true;
            
        }

        void WaitForInit()
        {
            while (!initSuccess && !initFail)
            {
                Wait(1.0);
            }
        }

        void OnAddUser(UserData userData)
        {
            WaitForInit();

            AgentPrivate agent = ScenePrivate.FindAgent(userData.User);

            if (agent == null || !agent.IsValid)
            {
                Log.Write(LogLevel.Error, "Failed to get agent info, user may have left experience.");
                return;
            }

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

            Quest quest = null;
            var questData = WaitFor(QuestDefinition.GetQuest, agentInfo.SessionId) as QuestDefinition.GetQuestData;
            if (questData.Success)
            {
                quest = questData.Quest;
            }

            if (quest != null)
            {
                UpdateIndicator(quest);

                quest.Subscribe(QuestState.Active, (QuestData data) =>
                {
                    UpdateIndicator(quest);
                });

                quest.Subscribe(QuestState.Completed, (QuestData data) =>
                {
                    UpdateIndicator(quest);
                });

                quest.Subscribe(QuestState.None, (QuestData data) =>
                {
                    UpdateIndicator(quest);
                });
            }
        }

        void UpdateIndicator(Quest quest)
        {
            bool visible = ShowWhenAvailable && quest.GetState() == QuestState.None;
            visible |= ShowWhenActive && quest.GetState() == QuestState.Active;
            visible |= ShowWhenComplete && quest.GetState() == QuestState.Completed;

            indicatorMeshComponent.SetIsVisible(quest.Agent, visible);
        }
    }
            
}
