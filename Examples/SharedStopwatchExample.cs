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
using System.Diagnostics;

// This class is a simple stopwatch that can respond to requests for the current elapsed time
// and resets. The elapsed time is also broadcast periodically.
public class SharedStopwatchExample : SceneObjectScript
{
    #region ResetTime
    // This is the event object. Setting the base class to Reflective allows access to the public 
    // properties and methods without needing a reference to this script
    public class ResetTime : Reflective
    {
        // The id of the script which made the last reset request.
        // The setter is made internal so that our script can call
        // it but other scripts cannot.
        public ScriptId SourceScriptId { get; internal set; }
        
        // The current start time.
        // The setter is made internal so that our script can call
        // it but other scripts cannot.
        public long StartTime { get; internal set; }

        // The calculated elapsed time
        public TimeSpan Elapsed
        {
            get
            {
                return TimeSpan.FromTicks(Stopwatch.GetTimestamp() - StartTime);
            }
        }
        // The count of times the timer has been reset
        // The setter is made internal so that our script can call
        // it but other scripts cannot.
        public int ResetCount { get; internal set; }
    }
    #endregion ResetTime

    #region EditorProperties
    [DefaultValue("request")]
    public readonly string requestCommand = "request";

    [DefaultValue("elapsed")]
    public readonly string elapsedCommand = "elapsed";

    [DefaultValue("reset")]
    public readonly string resetCommand = "reset";

    [DefaultValue(1)]
    public readonly double broadcastMinutes = .15;
    #endregion EditorProperties

    #region EventHandlers
    // Handler for the requestCommand, sends the current information back to the requesting script
    private void requestElapsed(ScriptEventData uptime)
    {
        sendElapsed(uptime.SourceScriptId);
    }

    // Handler for the resetCommand, resets the start time
    private void resetElapsed(ScriptEventData uptime)
    {
        resetElapsed(uptime.SourceScriptId);
    }
    #endregion EventHandlers

    #region Implementation

    // Object which tracks all the info we need
    private ResetTime resetTime = new ResetTime();

    // Posts the current time information to the given script id
    // targetScriptId will be AllScripts for the broadcast
    private void sendElapsed(ScriptId targetScriptId)
    {
        PostScriptEvent(targetScriptId, elapsedCommand, resetTime);
    }

    // Resets the elapsed time and tracks the id of the script making the request
    private void resetElapsed(ScriptId id)
    {
        Log.Write(LogLevel.Info, Script.ID.ToString(), $"reset requested by script {id}");
        resetTime.SourceScriptId = id;
        resetTime.StartTime = Stopwatch.GetTimestamp();
        resetTime.ResetCount++;
    }
    #endregion Implementation

    #region Overrides
    public override void Init()
    {
        // write this script id to the log to track messages
        Log.Write(LogLevel.Info, Script.ID.ToString(), nameof(SharedStopwatchExample));

        // sets the initial timer and script id to this script
        resetElapsed(Script.ID);

        // listen for direct requests for the elapsed time
        SubscribeToScriptEvent(requestCommand, requestElapsed);

        // listen for requests to reset the time
        SubscribeToScriptEvent(resetCommand, resetElapsed);

        // set up a timer that broadcasts the elapsed time
        Timer.Create(TimeSpan.FromMinutes(broadcastMinutes), 
            TimeSpan.FromMinutes(broadcastMinutes), 
            () => sendElapsed(ScriptId.AllScripts));
    }
    #endregion Overrides

}