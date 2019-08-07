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
    [Tooltip("Controls the visibilty of an object based on a quest objective status.")]
    [DisplayName("Objective Indicator")]
    public class ObjectiveIndicator : SceneObjectScript
    {
        #region EditorProperties

        [Tooltip(@"The objective definition.")]
        [DisplayName("Objective")]
        public readonly ObjectiveDefinition ObjectiveDefinition;

        [Tooltip(@"If an inventory object is used show as the objective indicator. If this is not set, the object that the script is attached to will be used. In either case, the object's Mesh Component must have IsScriptable set to 'On'")]
        [DisplayName("Objective Indicator")]
        public ClusterResource IndicatorResource;

        [Tooltip(@"If 'Objective Indicator' is set to an inventory object, it will be shown offset from this object by this distance.")]
        [DisplayName("Indicator Offset")]
        [DefaultValue(0.0f, 0.0f, 1.0f)]
        public Vector IndicatorOffset;

        [Tooltip(@"Show the Objective Indicator when the objective can be completed.")]
        [DefaultValue(true)]
        public bool ShowWhenActive;

        [Tooltip(@"Show the Objective Indicator when the objective has been completed.")]
        [DefaultValue(false)]
        public bool ShowWhenComplete;

        #endregion

        MeshComponent indicatorMeshComponent = null;
        bool initSuccess = false;
        bool initFail = false;

        public override void Init()
        {
            if (ObjectiveDefinition == null)
            {
                Log.Write(LogLevel.Error, "Objective Definition not found.");
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

            var update = WaitFor(ObjectiveDefinition.Update);

            if (update.Success)
            {
                Log.Write(LogLevel.Info, $"Got objective definition: {ObjectiveDefinition.Title}");
            }
            else
            {
                Log.Write(LogLevel.Error, "Failed to update objective definition.");
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

            Objective objective = null;
            var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, agentInfo.SessionId) as ObjectiveDefinition.GetObjectiveData;
            if (objectiveData.Success)
            {
                objective = objectiveData.Objective;
            }

            if (objective != null)
            {
                UpdateIndicator(objective);

                objective.Subscribe(ObjectiveState.Active, (ObjectiveData data) =>
                {
                    UpdateIndicator(objective);
                });

                objective.Subscribe(ObjectiveState.Completed, (ObjectiveData data) =>
                {
                    UpdateIndicator(objective);
                });

                objective.Subscribe(ObjectiveState.None, (ObjectiveData data) =>
                {
                    UpdateIndicator(objective);
                });
            }
        }

        void UpdateIndicator(Objective objective)
        {
            bool visible = ShowWhenActive && objective.GetState() == ObjectiveState.Active;
            visible |= ShowWhenComplete && objective.GetState() == ObjectiveState.Completed;

            indicatorMeshComponent.SetIsVisible(objective.Agent, visible);
        }
    }
            
}
