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


public class LowerAgentGravity : SceneObjectScript
{
    #region EditorProperties
    // An Interaction public property makes the script clickable.
    // This interaction will have a default prompt of "Click Me!"
    // Public fields show in the object properties after being added to a script.
    [DefaultValue("Click Me!")]
    public Interaction InteractionPrompt;
    [DisplayName("Quest Command")]
    public string questCommand;
    [DisplayName("Gravity Factor")]
    [DefaultValue(0.1f)]
    public readonly float gravityFactor = 0.1f;
    #endregion

    public class SimpleData : Reflective
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    SimpleData _simpleData = null;

    // Init() is where the script is setup and is run when the script starts.
    public override void Init()
    {
        // initialize simple data object to send events to other scripts
        _simpleData = new SimpleData(this);

        // Subscribe to interaction events to do something when the object is clicked.
        InteractionPrompt.Subscribe(OnClick);
    }

    public void OnClick(InteractionData data)
    {
        // Find the agent that clicked.
        AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

        // Lower gravity for the agent that clicked
        agent.SetGravityFactor(gravityFactor);

        //Send the quest event
        SendQuestEvent(questCommand, agent);

    }

    void SendQuestEvent(string eventName, AgentPrivate agent)
    {
        if (agent != null)
        {
            //ObjectId and AgentInfo are used by other simple scripts to know who did it
            _simpleData.AgentInfo = agent.AgentInfo;
            _simpleData.ObjectId = agent.AgentInfo.ObjectId;
        }

        // Post script event
        PostScriptEvent(eventName, _simpleData);
    }

}