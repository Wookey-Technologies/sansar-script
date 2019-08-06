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
    [Tooltip("Moves an object in response to simple script events.")]
    [DisplayName(nameof(Mover))]
    public class Mover : LibraryBase
    {
        #region EditorProperties
        [Tooltip(@"Start moving on these events. Can be a comma separated list of event names.
Will be ignored if the move motion is already in progress.")]
        [DefaultValue("move")]
        [DisplayName("-> Move Event")]
        public readonly string MoveEvent;

        [Tooltip(@"Start return to base position on these events. Can be a comma separated list of event names.
Will be ignored if the reset motion is already in progress.")]
        [DefaultValue("reset")]
        [DisplayName("-> Reset Event")]
        public readonly string ResetEvent;

        [Tooltip("Offset from base position, in objects local space")]
        [DisplayName("Position Offset")]
        public readonly Vector PositionOffset;

        [Tooltip("Offset from base orientation, in objects local space. Units are in degrees around the objects local x, y and z axis.")]
        [DisplayName("Rotation Offset")]
        [DefaultValue("<0,0,60>")]
        public readonly Vector RotationOffset;

        [Tooltip("The time taken to move from the base position to the offset position. The same time is used for the return motion.")]
        [DefaultValue(2.0f)]
        [DisplayName("Over time")]
        public readonly float MoveDuration;

        [Tooltip("Event sent when the move motion begins. Use to trigger SFX etc.")]
        [DefaultValue("")]
        [DisplayName("Move Began Event ->")]
        public readonly string MoveBegan;

        [Tooltip("Event sent when the move motion begins. Use to trigger SFX etc.")]
        [DefaultValue("")]
        [DisplayName("Return Began Event ->")]
        public readonly string ReturnBegan;

        [Tooltip("If true, motion will be smoothed so that it starts and ends slowly. The total duration will still be the same.")]
        [DisplayName("Ease In/Out")]
        [DefaultValue(false)]
        public readonly bool EaseInOut;

        [Tooltip("if true, start in the moved position.")]
        [DefaultValue(false)]
        [DisplayName("Start Moved")]
        public readonly bool StartMoved;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("light_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("light_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion


        enum State
        {
            Returned,
            Moving,
            Moved,
            Returning
        }

        private Sansar.Simulation.Mover mover;
        private Vector returnPosition;
        private Vector movedPosition;

        private Quaternion returnRotation;
        private Quaternion movedRotation;
      
        private State state = State.Returned;
        private Action subscriptions;

        const float minDuration = 0.1f;

        SimpleData thisObjectData;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.IsMovable)
            {
                Log.Write(LogLevel.Error, "Object is not movable");
                return;
            }

            mover = ObjectPrivate.Mover;

            thisObjectData = new SimpleData(this);
            thisObjectData.SourceObjectId = ObjectPrivate.ObjectId;
            thisObjectData.AgentInfo = null;
            thisObjectData.ObjectId = ObjectPrivate.ObjectId;

            Quaternion rotation = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * RotationOffset);
            returnRotation = ObjectPrivate.InitialRotation;
            movedRotation = returnRotation * rotation;

            returnPosition = ObjectPrivate.InitialPosition;
            movedPosition = returnPosition + PositionOffset.Rotate(returnRotation);

            if (StartMoved)
            {
                mover.StopAndClear();
                mover.AddMove(movedPosition, movedRotation);
                state = State.Moved;
            }
            else
            {
                mover.StopAndClear();
                mover.AddMove(returnPosition, returnRotation);
                state = State.Returned;
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (subscriptions == null)
            {
                subscriptions = SubscribeToAll(ResetEvent, (data) =>
                {
                    if (state != State.Returned && state != State.Returning)
                    {
                        StartCoroutine(Return);
                    }
                });

                subscriptions += SubscribeToAll(MoveEvent, (data) =>
                {
                    if (state != State.Moved && state != State.Moving)
                    {
                        StartCoroutine(Move);
                    }
                });
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (subscriptions != null)
            {
                subscriptions();
                subscriptions = null;
            }

            mover.StopAndClear();
        }

        void Move()
        {
            if (MoveDuration < minDuration)
            {
                mover.StopAndClear();
                mover.AddMove(movedPosition, movedRotation);
                state = State.Moved;
                return;
            }

            state = State.Moving;

            mover.StopAndClear();
            mover.AddMove(movedPosition, movedRotation, MoveDuration, EaseInOut ? MoveMode.Smoothstep : MoveMode.Linear, (e) =>
            {
                state = State.Moved;
            });
            SendToAll(MoveBegan, thisObjectData);
        }

        void Return()
        {
            if (MoveDuration < minDuration)
            {
                mover.StopAndClear();
                mover.AddMove(returnPosition, returnRotation);
                state = State.Moved;
                return;
            }
            state = State.Returning;

            mover.StopAndClear();
            mover.AddMove(returnPosition, returnRotation, MoveDuration, EaseInOut ? MoveMode.Smoothstep : MoveMode.Linear, (e) =>
            {
                state = State.Returned;
            });
            SendToAll(ReturnBegan, thisObjectData);
        }
    }
}

