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
using Sansar;
using System;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Resets the owning object to it's initial position.")]
    [DisplayName("Position Reset")]
    public class PositionReset : LibraryBase
    {
        #region EditorProperties
        [Tooltip("Reset position on these events. Can be a comma separated list of event names.")]
        [DefaultValue("reset")]
        [DisplayName("-> Reset Event")]
        public readonly string ResetEvent;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("reset_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("reset_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        RigidBodyComponent RigidBody;
        Action Unsubscribes = null;
        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out RigidBody) || RigidBody.GetMotionType() == RigidBodyMotionType.MotionTypeStatic)
            {
                Log.Write(LogLevel.Error, __SimpleTag, "SimpleReset requires a non-static rigid body to work properly.");
                return;
            }
            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(ResetEvent, Reset);
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (Unsubscribes != null)
            {
                Unsubscribes();
                Unsubscribes = null;
            }
        }

        void Reset(ScriptEventData data)
        {
            if (RigidBody.GetHeldObjectInfo().IsHeld)
            {
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, (d) => { ResetToInitialPosition(); }, false);
            }
            else
            {
                ResetToInitialPosition();
            }
        }

        void ResetToInitialPosition()
        {
            var motionType = RigidBody.GetMotionType();
            WaitFor(RigidBody.SetMotionType, RigidBodyMotionType.MotionTypeKeyframed);
            RigidBody.SetAngularVelocity(Vector.Zero);
            RigidBody.SetLinearVelocity(Vector.Zero);
            RigidBody.SetOrientation(ObjectPrivate.InitialRotation);
            RigidBody.SetPosition(ObjectPrivate.InitialPosition);
            RigidBody.SetMotionType(motionType);
        }
    }
}
