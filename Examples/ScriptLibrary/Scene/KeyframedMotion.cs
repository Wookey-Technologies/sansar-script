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
    [Tooltip("Moves a keyframed physics body in response to simple script events.")]
    [DisplayName("Keyframed Motion")]
    public class KeyframedMotion : LibraryBase
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

        [Tooltip("The pivot point of the rotation, in objects local space.")]
        [DisplayName("Rotation Pivot")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector RotationPivot;

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

        [Tooltip("If > 0, motion will be smoothed so that it starts and ends slowly. The total duration will still be the same.")]
        [DisplayName("Ease In/Out")]
        [DefaultValue(0)]
        [Range(0f, 1f)]
        public readonly float EaseInOut;

        [Tooltip("if true, start in the moved position.")]
        [DefaultValue(false)]
        [DisplayName("Start Moved")]
        public readonly bool StartMoved;
        #endregion

        enum State
        {
            Returned,
            Moving,
            Moved,
            Returning
        }

        private RigidBodyComponent RigidBody;
        private Vector returnPosition;
        private Vector movedPosition;

        private Quaternion returnRotation;
        private Quaternion movedRotation;
        private Vector worldRotationAxis;
        private Vector localRotationAxis;

        private int numTurns;
        private int turnDirection;
        private int turnCount;

        private float translateSpeed;
        private float rotateSpeed;
        private State state = State.Returned;
        private Action subscriptionClose;
        private Action subscriptionOpen;

        const float precision = 0.04f;
        const float anglePrecision = 0.001f;
        const float timestep = 0.1f;
        const float minimumSpeedMultipler = 0.1f;

        SimpleData thisObjectData;

        const float PI = (float)Math.PI;
        const float TwoPI = (float)(2.0 * Math.PI);

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "Simple Mover requires a Rigidbody set to motion type Keyframed");
                return;
            }

            if (RigidBody.GetMotionType() != RigidBodyMotionType.MotionTypeKeyframed)
            {
                Log.Write(LogLevel.Error, __SimpleTag, "Simple Mover requires a Rigidbody set to motion type Keyframed");
                return;
            }

            thisObjectData = new SimpleData(this);
            thisObjectData.SourceObjectId = ObjectPrivate.ObjectId;
            thisObjectData.AgentInfo = null;
            thisObjectData.ObjectId = ObjectPrivate.ObjectId;

            WaitFor(RigidBody.SetCenterOfMass, RotationPivot);

            Subscribe(null);

            Quaternion rotation = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * RotationOffset);
            returnRotation = ObjectPrivate.InitialRotation;

            movedRotation = returnRotation * rotation;

            numTurns = (int)((RotationOffset.Length() + 179.0f) / 360f);

            bool noRotation = RotationOffset.Length() < 0.5f;

            if (noRotation)
            {
                worldRotationAxis = Vector.ObjectUp;
            }
            else
            {
                float rotationAngle;
                rotation.ToAngleAxis(out rotationAngle, out localRotationAxis);

                if (Math.Abs(rotationAngle % TwoPI) < 0.001f)
                {
                    // rotation axis won't be calculated correctly for exact multiple of 360 rotation
                    // adjust euler angles slightly and re-calculate
                    float x = RotationOffset.X;
                    float y = RotationOffset.Y;
                    float z = RotationOffset.Z;
                    if (x != 0) x = Math.Sign(x) * (Math.Abs(x) - 1.0f);
                    if (y != 0) y = Math.Sign(y) * (Math.Abs(y) - 1.0f);
                    if (z != 0) z = Math.Sign(z) * (Math.Abs(z) - 1.0f);
                    Vector adjustedOffset = new Vector(x, y, z);
                    Quaternion adjustedRotation = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * adjustedOffset);
                    float tempAngle;
                    adjustedRotation.ToAngleAxis(out tempAngle, out localRotationAxis);
                }
                worldRotationAxis = localRotationAxis.Rotate(returnRotation);
            }

            float moveAngle = GetAngleFromZero(movedRotation);
            rotateSpeed = Math.Abs(moveAngle + numTurns * TwoPI) / (MoveDuration * SpeedCurveAverage());

            if (!noRotation)
            {
                Quaternion unitRotation = Quaternion.FromAngleAxis(1f, localRotationAxis);
                turnDirection = Math.Sign(GetAngleFromZero(returnRotation * unitRotation));
            }

            returnPosition = ObjectPrivate.InitialPosition + RotationPivot.Rotate(returnRotation);
            movedPosition = returnPosition + (PositionOffset).Rotate(returnRotation);
            translateSpeed = (movedPosition - returnPosition).Length() / (MoveDuration * SpeedCurveAverage());

            if (StartMoved)
            {
                RigidBody.SetOrientation(movedRotation, (e) =>
                {
                    SetPositionOfCOM(movedPosition);
                });
                turnCount = numTurns;
                state = State.Moved;
            }
            else
            {
                SetPositionOfCOM(returnPosition);
                state = State.Returned;
            }

            if (DebugEnabled())
            {
                Log.Write("rotation angle:" + moveAngle + " around:" + localRotationAxis + " world space axis:" + worldRotationAxis + " revolutions:" + numTurns);
            }
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (subscriptionClose == null)
            {
                subscriptionClose = SubscribeToAll(ResetEvent, (data) =>
                {
                    if (state != State.Returned && state != State.Returning)
                    {
                        StartCoroutine(Return);
                    }
                });
            }
            if (subscriptionOpen == null)
            {
                subscriptionOpen = SubscribeToAll(MoveEvent, (data) =>
                {
                    if (state != State.Moved && state != State.Moving)
                    {
                        StartCoroutine(Move);
                    }
                });
            }
        }

        void Move()
        {
            if (MoveDuration < timestep)
            {
                RigidBody.SetOrientation(movedRotation, (e) =>
                {
                    SetPositionOfCOM(movedPosition);
                });
                state = State.Moved;
                return;
            }
            state = State.Moving;
            Vector fromPosition = GetPositionOfCOM();
            Quaternion fromRotation = RigidBody.GetOrientation();

            float fromAngle = GetAngleFromZero(fromRotation) + turnDirection * turnCount * TwoPI;
            float toAngle = GetAngleFromZero(movedRotation) + turnDirection * numTurns * TwoPI;

            bool translateDone = false;
            bool rotateDone = false;
            bool rotateWillComplete = false;

            if (DebugEnabled())
            {
                Log.Write("Open, " + Mathf.DegreesPerRadian * fromAngle + " -> " + Mathf.DegreesPerRadian * toAngle + " axis " + worldRotationAxis + " speed " + rotateSpeed);
            }

            SendToAll(MoveBegan, thisObjectData);

            while (state == State.Moving)
            {
                if (!translateDone) ApplyTranslation(fromPosition, movedPosition, translateSpeed, ref translateDone);
                if (!rotateDone) ApplyRotation(fromAngle, toAngle, movedRotation, rotateSpeed, ref rotateDone, ref rotateWillComplete);

                if (translateDone && rotateDone)
                {
                    RigidBody.SetOrientation(movedRotation, (e) =>
                    {
                        SetPositionOfCOM(movedPosition);
                    });
                    state = State.Moved;
                    break;
                }
                Wait(TimeSpan.FromSeconds(timestep));
            }
        }

        void Return()
        {
            if (MoveDuration < timestep)
            {
                RigidBody.SetOrientation(returnRotation, (e) =>
                {
                    SetPositionOfCOM(returnPosition);
                });
                state = State.Returned;
                return;
            }
            state = State.Returning;
            Vector fromPosition = GetPositionOfCOM();
            Quaternion fromRotation = RigidBody.GetOrientation();

            float fromAngle = GetAngleFromZero(fromRotation) + turnCount * TwoPI;

            bool translateDone = false;
            bool rotateDone = false;
            bool rotateWillComplete = false;

            if (DebugEnabled())
            {
                Log.Write("Close, " + Mathf.DegreesPerRadian * fromAngle + " ->  0 " + " from turn count " + turnCount + " axis " + worldRotationAxis + " speed " + rotateSpeed);
            }

            SendToAll(ReturnBegan, thisObjectData);

            while (state == State.Returning)
            {
                if (!translateDone) ApplyTranslation(fromPosition, returnPosition, translateSpeed, ref translateDone);
                if (!rotateDone) ApplyRotation(fromAngle, 0, returnRotation, rotateSpeed, ref rotateDone, ref rotateWillComplete);

                if (translateDone && rotateDone)
                {
                    RigidBody.SetOrientation(returnRotation, (e) =>
                    {
                        SetPositionOfCOM(returnPosition);
                    });
                    state = State.Returned;
                    break;
                }
                Wait(TimeSpan.FromSeconds(timestep));
            }
        }

        void ApplyTranslation(Vector startPosition, Vector targetPosition, float speed, ref bool isComplete)
        {
            Vector totalOffset = targetPosition - startPosition;

            if (totalOffset.Length() <= precision)
            {
                isComplete = true;
                return;
            }
            Vector moveDirection = totalOffset.Normalized();

            Vector currentOffset = targetPosition - GetPositionOfCOM();
            if (currentOffset.Length() < precision)
            {
                RigidBody.SetLinearVelocity(Vector.Zero);
                isComplete = true;
                return;
            }

            float distanceToTarget = moveDirection.Dot(currentOffset);

            if (distanceToTarget < 0) // overshot
            {
                RigidBody.SetLinearVelocity(Vector.Zero);
                isComplete = true;
                return;
            }

            if (EaseInOut > 0 && MoveDuration > 4f * timestep)
            {
                float speedMod = SpeedCurve(distanceToTarget, (movedPosition - returnPosition).Length());
                speed = speed * speedMod;
            }

            if (distanceToTarget < speed * timestep) // slow down if we think we will overshoot next timestep 
            {
                speed = distanceToTarget / timestep;
            }

            Vector velocity = speed * moveDirection;
            RigidBody.SetLinearVelocity(velocity);
        }


        void ApplyRotation(float startAngle, float targetAngle, Quaternion targetRotation, float speed, ref bool isComplete, ref bool willComplete)
        {
            float totalAngle = targetAngle - startAngle;

            if (Math.Abs(totalAngle) < anglePrecision)
            {
                isComplete = true;
                return;
            }

            int sign = Math.Sign(totalAngle);

            Quaternion currentRotation = RigidBody.GetOrientation();

            float angleNoTurn = GetAngleFromZero(currentRotation);
            float angle = angleNoTurn + turnDirection * turnCount * TwoPI;

            float angleOffset = sign * (targetAngle - angle);

            if (willComplete || Math.Abs(angleOffset) < anglePrecision)
            {
                RigidBody.SetAngularVelocity(Vector.Zero);
                RigidBody.SetOrientation(targetRotation);
                isComplete = true;
                return;
            }

            if (angleOffset < 0) // overshot
            {
                RigidBody.SetAngularVelocity(Vector.Zero);
                RigidBody.SetOrientation(targetRotation);
                isComplete = true;
                return;
            }

            if (EaseInOut > 0 && MoveDuration > 4f * timestep)
            {
                float speedMod = SpeedCurve(Math.Abs(angle), Math.Abs(GetAngleFromZero(movedRotation) + turnDirection * numTurns * TwoPI));
                speed = speed * speedMod;
            }

            if (angleOffset < speed * timestep)
            {
                speed = angleOffset / timestep;
                willComplete = true;
            }

            Vector velocity = sign * speed * worldRotationAxis;
            RigidBody.SetAngularVelocity(velocity);

            if (willComplete)
            {
                return;
            }

            float prediction = Math.Abs(angleNoTurn + sign * timestep * speed);
            if (prediction >= PI || Math.Abs(angleNoTurn) > PI && prediction < PI)
            {
                turnCount += sign;
            }
        }

        float SpeedCurve(float p, float totalP)
        {
            //       1 |     ______    
            //         |   /        \
            //         |  /          \
            // minimum |_              _
            //         |
            //       0 |________________ totalP
            //         ^---^       ^----^ 
            //       rampUpLength  rampDownLength
            if (p < 0f) p = 0f;
            if (p > totalP) p = totalP;

            float rampUpEnd = EaseInOut * totalP / 2f;
            float rampDownStart = totalP * (1f - EaseInOut / 2f);

            if (p < rampUpEnd)
            {
                float t = p / rampUpEnd;
                t = t * t * (3.0f - 2.0f * t); // apply ease in/out
                t = minimumSpeedMultipler + t * (1f - minimumSpeedMultipler);
                return t;
            }
            else if (p > rampDownStart)
            {
                float t = 1f - (p - rampDownStart) / (totalP - rampDownStart); // 1 at start of rampdown, 0 at end (p = duration)
                t = t * t * (3.0f - 2.0f * t); // apply ease in/out
                t = minimumSpeedMultipler + t * (1f - minimumSpeedMultipler);
                return t;
            }

            return 1f;
        }

        float SpeedCurveAverage()
        {
            float rampAverage = (1f - minimumSpeedMultipler) / 2f;
            return EaseInOut * rampAverage + (1f - EaseInOut);
        }

        float GetAngleFromZero(Quaternion q)
        {
            Quaternion relative = returnRotation.Inverse() * q;

            float angle;
            Vector axis;

            relative.ToAngleAxis(out angle, out axis);

            if (axis.Dot(localRotationAxis) < 0)
            {
                angle = -angle;
            }

            if (angle < -PI)
            {
                angle += TwoPI;
            }
            else if (angle > PI)
            {
                angle -= TwoPI;
            }
            return angle;
        }

        void SetPositionOfCOM(Vector v)
        {
            RigidBody.SetPosition(v - RigidBody.GetCenterOfMass().Rotate(RigidBody.GetOrientation()));
        }

        Vector GetPositionOfCOM()
        {
            return RigidBody.GetPosition() + RotationPivot.Rotate(RigidBody.GetOrientation());
        }
    }
}
