/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Use a coroutine to wait for event callbacks to track visitors to a scene.
 * For every visitor start a new coroutine to track that visitor's time.
 */
using System;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;


// This excample shows how to use the ICoroutine interface and the Lock APIs to control coroutines.
// It is not a very practical example, but demonstrates some advanced concepts.
public class AdvancedCoroutineExample : SceneObjectScript
{
    public override void Init()
    {
        // start a quick job
        ICoroutine quickWork = StartCoroutine(DoWork, 0.2);

        // Start up a couple of coroutines that will "work" for 10s
        ICoroutine worker1 = StartCoroutine(DoWork, 10.0);
        ICoroutine worker2 = StartCoroutine(DoWork, 10.0);
        worker1.Name = "WorkerOne"; // Default name would be "DoWork"
        worker2.Name = "WorkerTwo";

        // Wait for quickWork to have finished.
        WaitFor(quickWork);
        // quickWork.IsAlive will now be false

        // Abort worker1.
        worker1.Abort();
        // worker1.IsAlive will now be false
        // Note that it will never log "has finished"

        // Start up a coroutine that won't do anything until it gets a signal.
        ICoroutine signalWaiter = StartCoroutine(WaitUntilSignal);

        // Start a coroutine that will do some "work" then send a signal to signalWaiter.
        StartCoroutine(WorkThenSignal, 2.0, signalWaiter);

        WaitForAllCoroutines();
    }

    void WaitForAllCoroutines()
    {
        foreach (var c in GetAllCoroutines())
        {
            // Don't need to verify that it isn't _this_ coroutine because WaitFor on this coroutine returns immediately.
            // if (c != CurrentCoroutine) can be used to check.
            WaitFor(c);
        }
        Log.Write("All coroutines have finished");
    }

    void WaitUntilSignal()
    {
        // Useful for synchronizing. Can do some work before waiting for the signal
        int signals = WaitForSignal();
        // Then continue working after the signal.
        Log.Write(CurrentCoroutine.Name + " has finished with " + signals + " signals");
    }

    void WorkThenSignal(double seconds, ICoroutine other)
    {
        // Add the seconds and name of what we are waiting for to this coroutine's name.
        CurrentCoroutine.Name += " " + seconds.ToString() + " " + other.Name;
        // simulate working with a Wait
        Wait(TimeSpan.FromSeconds(seconds));
        other.Signal();
        Wait(TimeSpan.FromSeconds(seconds));
        Log.Write(CurrentCoroutine.Name + " has finished");
    }

    void DoWork(double seconds)
    {
        // This simulates a busy coroutine by just waiting.
        Wait(TimeSpan.FromSeconds(seconds));
        Log.Write(CurrentCoroutine.Name + " has finished");
    }

}
