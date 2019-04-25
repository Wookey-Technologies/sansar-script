/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar.Simulation;
using Sansar.Script;
using System;

public class ScriptEventSourceExample : SceneObjectScript
{
    #region ResetTime
    // This is the event interface that this script will use. Only the public methods and properties
    // that will be referenced are used.
    public interface ResetTime 
    {
        // The current start time.
        // The setter is not public so this script will only ask for the getter
        DateTime StartTime { get; }

        // The calculated elapsed time
        TimeSpan Elapsed { get; }
    }
    #endregion ResetTime

    #region EditorProperties
    [DefaultValue("request")]
    public readonly string requestCommand = "request";

    [DefaultValue("elapsed")]
    public readonly string elapsedCommand = "elapsed";

    [DefaultValue("reset")]
    public readonly string resetCommand = "reset";

    [DefaultValue(3)]
    public readonly double resetMinutes = .2;
    #endregion EditorProperties

    #region EventHandlers

    private void elapsed(ScriptEventData elapsed)
    {
        if(elapsed.Data == null)
        {
            Log.Write(LogLevel.Warning, Script.ID.ToString(), "Expected non-null event data");
            return;
        }
        ResetTime resetTime = elapsed.Data.AsInterface<ResetTime>();

        if(resetTime == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        Log.Write(LogLevel.Info, Script.ID.ToString(), $"Elapsed time = {resetTime.Elapsed} since {resetTime.StartTime}");
    }

    #endregion EventHandlers

    public override void Init()
    {
        // write this script id to the log to track messages
        Log.Write(LogLevel.Info, Script.ID.ToString(), nameof(ScriptEventSourceExample));

        // Subscribe to elapsed messages
        SubscribeToScriptEvent(elapsedCommand, elapsed);

        // set up a timer to periodically reset the elapsed time
        Timer.Create(TimeSpan.FromMinutes(resetMinutes),
            TimeSpan.FromMinutes(resetMinutes),
            () => PostScriptEvent(resetCommand));
        
    }


}