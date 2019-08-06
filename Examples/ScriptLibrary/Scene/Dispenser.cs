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
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Creates objects in response to simple script events. Can be configured to shoot projectiles.")]
    [DisplayName(nameof(Dispenser))]
    public class Dispenser : LibraryBase    // CreateCluster requires ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Dispense the object. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Dispense Object")]
        public readonly string DispenseEvent;

        [Tooltip("Destroy oldest previously dispensed object. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Destroy Oldest")]
        public readonly string DestroyOldestEvent;

        [Tooltip("Destroy oldest previously dispensed object. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Destroy Youngest")]
        public readonly string DestroyYoungestEvent;

        [Tooltip("Destroy all previously dispensed objects. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Destroy All")]
        public readonly string DestroyAllEvent;

        [Tooltip("Maximum number of objects the dispenser will dispense before destroying the oldest first.")]
        [DefaultValue(50)]
        [DisplayName("Max Number of Objects")]
        public readonly int MaxObjectCount;

        [Tooltip("The cluster resource to dispense")]
        [DisplayName("Object")]
        public readonly ClusterResource TheObject;

        [Tooltip("Offset from base position, in object's local space")]
        [DisplayName("Position Offset")]
        public readonly Vector PositionOffset;

        [Tooltip("Offset from base orientation, in object's local space. Units are in degrees around the objects local x, y and z axis.")]
        [DisplayName("Rotation Offset")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector RotationOffset;

        [Tooltip("Initial velocity, in object's local space.")]
        [DisplayName("Velocity")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector InitialLinearVelocity;

        [Tooltip("Initial angular velocity, in object's local space.")]
        [DisplayName("Angular Velocity")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector InitialAngularVelocity;

        [Tooltip("Inherit parent motion")]
        [DisplayName("Inherit Motion")]
        [DefaultValue(true)]
        public readonly bool InheritMotion;

        [Tooltip("Offset variance, in object's local space")]
        [DisplayName("Position Variance")]
        public readonly Vector PositionVariance;

        [Tooltip("Offset from base orientation, in object's local space. Units are in degrees around the objects local x, y and z axis.")]
        [DisplayName("Rotation Variance")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector RotationVariance;

        [Tooltip("Variance on initial velocity, in object's local space.")]
        [DisplayName("Velocity Variance")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector LinearVelocityVariance;

        [Tooltip("Variance on initial angular velocity, in object's local space.")]
        [DisplayName("Angular Vel Variance")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector AngularVelocityVariance;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("dispenser_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("dispenser_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        #endregion

        Action subscription = null;

        LinkedList<ScenePrivate.CreateClusterData> dispensedObjects = null;

        Random rnd;
        RigidBodyComponent dispenserRigidBody;

        bool hasPositionVariance = false;
        bool hasRotationVariance = false;
        bool hasLinearVelocityVariance = false;
        bool hasAngularVelocityVariance = false;
        bool hasInitialAngularVelocity = false;

        protected override void SimpleInit()
        {
            if (TheObject == null)
            {
                Log.Write(LogLevel.Error, __SimpleTag, "SimpleDispenser requires a ClusterResource set to work properly.");
                return;
            }

            rnd = new Random();

            if (!ObjectPrivate.TryGetFirstComponent(out dispenserRigidBody))
            {
                dispenserRigidBody = null;
            }

            dispensedObjects = new LinkedList<ScenePrivate.CreateClusterData>();

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);

            hasPositionVariance = (PositionVariance.LengthSquared() > 0.0f);
            hasRotationVariance = (RotationVariance.LengthSquared() > 0.0f);
            hasLinearVelocityVariance = (LinearVelocityVariance.LengthSquared() > 0.0f);
            hasAngularVelocityVariance = (AngularVelocityVariance.LengthSquared() > 0.0f);
            hasInitialAngularVelocity = (InitialAngularVelocity.LengthSquared() > 0.0f) || hasAngularVelocityVariance || (InheritMotion && (dispenserRigidBody != null));
        }

        private void Subscribe(ScriptEventData sed)
        {
            if ((subscription == null) && (TheObject != null))
            {
                subscription = SubscribeToAll(DispenseEvent, (data) =>
                {
                    DispenseObject(TheObject);
                });

                subscription += SubscribeToAll(DestroyOldestEvent, (data) =>
                {
                    DestroyOldestObject();
                });

                subscription += SubscribeToAll(DestroyYoungestEvent, (data) =>
                {
                    DestroyYoungestObject();
                });

                subscription += SubscribeToAll(DestroyAllEvent, (data) =>
                {
                    DestroyAllObjects();
                });
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (subscription != null)
            {
                subscription();
                subscription = null;
            }
        }

        private float RandomNegOneToOne()
        {
            return (float)(rnd.NextDouble() * 2.0 - 1.0);
        }

        private void DispenseObject(ClusterResource clusterResource)
        {
            if ((MaxObjectCount > 0) && (dispensedObjects.Count >= MaxObjectCount))
            {
                DestroyOldestObject();
            }

            Vector rotWithVariance = RotationOffset;
            if (hasRotationVariance)
            {
                rotWithVariance += new Vector(RotationVariance.X * RandomNegOneToOne(), RotationVariance.Y * RandomNegOneToOne(), RotationVariance.Z * RandomNegOneToOne());
            }

            Quaternion rotation = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * rotWithVariance);
            Quaternion objectRotation = ObjectPrivate.Rotation;

            Vector position = PositionOffset;
            if (hasPositionVariance)
            {
                position += new Vector(PositionVariance.X * RandomNegOneToOne(), PositionVariance.Y * RandomNegOneToOne(), PositionVariance.Z * RandomNegOneToOne());
            }

            Vector velocity = InitialLinearVelocity;
            if (hasLinearVelocityVariance)
            {
                velocity += new Vector(LinearVelocityVariance.X * RandomNegOneToOne(), LinearVelocityVariance.Y * RandomNegOneToOne(), LinearVelocityVariance.Z * RandomNegOneToOne());
            }

            if (InheritMotion && (dispenserRigidBody != null))
            {
                velocity += dispenserRigidBody.GetLinearVelocity();
            }

            try
            {
                ScenePrivate.CreateClusterData dispensedObject = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster,
                clusterResource,
                ObjectPrivate.Position + position.Rotate(objectRotation),
                ObjectPrivate.Rotation * rotation,
                velocity.Rotate(objectRotation));

                if (hasInitialAngularVelocity)
                {
                    RigidBodyComponent rb;
                    if (dispensedObject.ClusterReference.GetObjectPrivates().FirstOrDefault().TryGetFirstComponent(out rb))
                    {
                        if (rb.GetMotionType() != RigidBodyMotionType.MotionTypeStatic)
                        {
                            Vector angularVel = InitialAngularVelocity;

                            if (hasAngularVelocityVariance)
                            {
                                angularVel += new Vector(AngularVelocityVariance.X * RandomNegOneToOne(), AngularVelocityVariance.Y * RandomNegOneToOne(), AngularVelocityVariance.Z * RandomNegOneToOne());
                            }

                            if (InheritMotion && (dispenserRigidBody != null))
                            {
                                angularVel += dispenserRigidBody.GetAngularVelocity();
                            }

                            rb.SetAngularVelocity(angularVel);
                        }
                    }
                }

                dispensedObjects.AddLast(dispensedObject);
            }
            catch (ThrottleException)
            {
                // Throttled
                Log.Write(LogLevel.Warning, "DispenseObject throttle hit. No object created.");
            }
        }

        private void DestroyOldestObject()
        {
            if (dispensedObjects.Count > 0)
            {
                var dispensedObject = dispensedObjects.First();
                dispensedObjects.RemoveFirst();

                dispensedObject.ClusterReference.Destroy();
            }
        }

        private void DestroyYoungestObject()
        {
            if (dispensedObjects.Count > 0)
            {
                var dispensedObject = dispensedObjects.Last();
                dispensedObjects.RemoveLast();

                dispensedObject.ClusterReference.Destroy();
            }
        }

        private void DestroyAllObjects()
        {
            while (dispensedObjects.Count > 0)
            {
                DestroyOldestObject();
            }
        }
    }
}
