/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Start a coroutine to wait for collision events.
 * Which types of collisions are handled may be set on the properties panel for
 * the object when editing the scene.
 */
using Sansar.Simulation;
using Sansar.Script;
using System;

// To get full access to the Object API a script must extend from ObjectScript
public class SceneObjectScriptExample : SceneObjectScript
{
    // Components can be set in the editor if the correct component types are added to the object
    [EditorVisible]
    private RigidBodyComponent RigidBody = null;

    [DefaultValue(true)]
    [DisplayName("Track Avatar Hits")]
    [EditorVisible]
    private bool TrackAgentHits = true;

    [DefaultValue(true)]
    [DisplayName("Track Object Hits")]
    [EditorVisible]
    private bool TrackObjectHits = true;

    public SceneObjectScriptExample()
    {
        // Script initialization order:
        //    Constructor (this method)
        //    Set base class members (ObjectScript's ExperienceInfo, SceneInfo, and ObjectPrivate)
        //    Set public fields that were configured at edit time (RigidBody, TrackAgentHist, TrackObjectHits)
        //    Init()
    }

    // Override Init to set up event handlers and start coroutines.
    public override void Init()
    {
        // When unhandled exceptions occur, they may be caught with this event handler.
        // Certain exceptions may not be recoverable at all and may cause the script to
        // immediately be removed.
        Script.UnhandledException += UnhandledException;

        // If the object had its components set in the editor they should now have the member initialized
        // The component can also be found dynamically
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
        }

        // Convert the supplied bools to the correct CollisionEventType to track
        CollisionEventType trackedEvents = 0;
        if (TrackAgentHits)
        {
            trackedEvents |= CollisionEventType.CharacterContact;
        }
        if (TrackObjectHits)
        {
            trackedEvents |= CollisionEventType.RigidBodyContact;
        }

        // StartCoroutine will queue CheckForCollisions to run
        // Arguments after the coroutine method are passed as arguments to the method.
        StartCoroutine(CheckForCollisions, trackedEvents);
    }

    private void CheckForCollisions(CollisionEventType trackedEvents)
    {
        while (true)
        {
            // This will block the coroutine until a collision happens
            CollisionData data = (CollisionData)WaitFor(RigidBody.Subscribe, trackedEvents, Sansar.Script.ComponentId.Invalid);
            if (data.EventType == CollisionEventType.CharacterContact)
            {
                ScenePrivate.Chat.MessageAllUsers("I hit an avatar!");
            }
            else
            {
                ScenePrivate.Chat.MessageAllUsers("I hit an object!");
            }
        }
    }

    private void UnhandledException(object sender, Exception e)
    {
        // Depending on the script scheduling policy, the exception may or may not be recoverable.
        // An unrecoverable exception that can be handled will only be given a short time to run
        // so the handler method needs to be kept small.
        // Any exception thrown from this method will terminate the script regardless of the value
        // of UnhandledExceptionRecoverable
        if (!Script.UnhandledExceptionRecoverable)
        {
            ScenePrivate.Chat.MessageAllUsers("Unrecoverable exception happened, the script will now be removed.");
        }
        else
        {
            ScenePrivate.Chat.MessageAllUsers("This script will be allowed to continue.");
        }
    }
}