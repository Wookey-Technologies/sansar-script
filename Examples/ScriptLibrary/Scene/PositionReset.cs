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
using System.Collections.Generic;

namespace ScriptLibrary
{
    [Tooltip("Resets the owning object to it's initial position.")]
    [DisplayName("Position Reset")]
    public class PositionReset : LibraryBase
    {
        #region EditorProperties
        [Tooltip("Set the objects this script will reset.\nOnly the object this script is on will be reset if this list is left empty.\nTo set a light, select the Volume from the object panel, right click and choose 'Copy' then add a new entry to this list, right click on it and choose Paste.")]
        [DisplayName("Volumes")]
        public List<RigidBodyComponent> RigidBodies;

        [Tooltip("Reset position on these events. Can be a comma separated list of event names.")]
        [DefaultValue("reset")]
        [DisplayName("-> Reset Event")]
        public readonly string ResetEvent;

        [Tooltip("If set and the object is being held when the Reset event is received, the object will be dropped and then reset.\nIf not set the object will not reset position until dropped manually or by other scripts.")]
        [DefaultValue(false)]
        [DisplayName("Force Drop")]
        public readonly bool ForceDrop;

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

        
        Action Unsubscribes = null;
        protected override void SimpleInit()
        {
            if (RigidBodies.Count > 0)
            {
                int removed = RigidBodies.RemoveAll(rb => rb == null || !rb.IsValid || rb.GetMotionType() == RigidBodyMotionType.MotionTypeStatic);
                if (removed > 0)
                {
                    Log.Write(LogLevel.Error, __SimpleTag, "Position Reset: " + removed +" volumes removed because they were not set correctly or are static.");
                }
                if (RigidBodies.Count == 0)
                {
                    Log.Write(LogLevel.Error, __SimpleTag, "Position Reset requires a non-static rigid bodies to work properly. Volumes were set, but none could be used.");
                    return;
                }
            }
            else
            {
                RigidBodyComponent RigidBody;
                if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out RigidBody) || RigidBody.GetMotionType() == RigidBodyMotionType.MotionTypeStatic)
                {
                    Log.Write(LogLevel.Error, __SimpleTag, "Position Reset requires a non-static rigid body to work properly.");
                    return;
                }
                RigidBodies.Add(RigidBody);
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
            // This will remove all the RBs from the list that have errors while being reset.
            RigidBodies.RemoveAll(rb => !Reset(rb));
        }

        bool Reset(RigidBodyComponent rigidBody)
        {
            if (!rigidBody.IsValid)
            {
                Log.Write(LogLevel.Warning, __SimpleTag, "Position Reset: object is no longer in the scene, removing from reset script.");
                return false;
            }

            if (rigidBody.GetHeldObjectInfo().IsHeld)
            {
                // Check now that the object is still in the scene.
                if (null == ScenePrivate.FindObject(rigidBody.ComponentId.ObjectId))
                {
                    return false;
                }

                if (ForceDrop)
                {
                    rigidBody.ReleaseHeldObject((d) => { ResetToInitialPosition(rigidBody); });
                }
                else
                {
                    rigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, (d) => { ResetToInitialPosition(rigidBody); }, false);
                }
                return true;
            }
            else
            {
                return ResetToInitialPosition(rigidBody);
            }
        }

        bool ResetToInitialPosition(RigidBodyComponent rigidBody)
        {
            ObjectPrivate objectPrivate = ScenePrivate.FindObject(rigidBody.ComponentId.ObjectId);
            if (objectPrivate != null && objectPrivate.IsValid)
            {
                try
                {
                    var motionType = rigidBody.GetMotionType();
                    WaitFor(rigidBody.SetMotionType, RigidBodyMotionType.MotionTypeKeyframed);
                    rigidBody.SetAngularVelocity(Vector.Zero);
                    rigidBody.SetLinearVelocity(Vector.Zero);
                    rigidBody.SetOrientation(objectPrivate.InitialRotation);
                    rigidBody.SetPosition(objectPrivate.InitialPosition);
                    rigidBody.SetMotionType(motionType);
                    return true;
                }
                catch
                {
                    Log.Write(LogLevel.Error, __SimpleTag, "Position Reset: error resetting object position.");
                }
            }

            return false;
        }
    }
}
