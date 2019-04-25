/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using System;
using Sansar.Script;
using Sansar.Simulation;

// This example shows how to track and report on script memory use.
// Memory use is tracked by "pool". There are two types of pools:
//   Scene: The scripts attached to the scene all share a pool that is allowed a large amount of memory
//   User: Scripts associated with each user share their own pool and are allowed a smaller amount of memory
public class MemoryExample : SceneObjectScript
{
    // Set the Report_Command in the object properties to the chat command used to get the memory report.
    [DefaultValue("/memory")]
    [DisplayName("Report Command")]
    public readonly string Report_Command = "/memory";

    private MemoryUseLevel memoryLevel = MemoryUseLevel.Low;

    public override void Init()
    {
        //When the memory level changes, store the new memory level.
        // If used in a more complex script this event could be used to clear log history or otherwise reduce used memory
        Memory.Subscribe((data) => { memoryLevel = data.UseLevel; });

        // Set a chat subscription that will call ReportMemory when anyone says the Report_Command
        ScenePrivate.Chat.Subscribe(0, null, (data) => { if (data.Message == Report_Command) ReportMemory(ScenePrivate.FindAgent(data.SourceId)); });
    }

    // Reports the last recorded memory level from an event along with current Memory data.
    private void ReportMemory(AgentPrivate agent)
    {
        agent.SendChat(String.Format("Memory info: [{0}] {1}", memoryLevel.ToString(), Memory.ToString()));
    }
}