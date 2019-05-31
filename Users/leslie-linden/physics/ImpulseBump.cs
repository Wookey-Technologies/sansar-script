// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;

public class ImpulseBump : SceneObjectScript
{
    // Public properties

    [DefaultValue(true)]
    public bool InteractionImpulse;

    [Tooltip("Simple script compatible event to trigger an impulse.")]
    public readonly string ImpulseScriptEvent;

    [DefaultValue(0,0,1)]
    public Vector LinearImpulseDirection;

    [DefaultValue(100.0f)]
    public float LinearForce;

    [DefaultValue(1,1,1)]
    public Vector AngularVariance;

    [DefaultValue(100.0f)]
    public float AngularForce;

    [Tooltip("If this is set above zero the object will reset to its initial position after this many seconds without a bump.")]
    public double ResetTimeout;

    // Privates

    RigidBodyComponent _rb = null;
    Random _rng = new Random();

    bool _applyLinearImpulse = false;
    bool _applyAngularImpulse = false;

    ICoroutine _resetCoroutine = null;

    // Logic!

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            Log.Write(LogLevel.Error, "ImpulseBump can't find a RigidBodyComponent!");
            return;
        }

        if (_rb.GetMotionType() != RigidBodyMotionType.MotionTypeDynamic)
        {
            Log.Write(LogLevel.Error, "ImpulseBump only works on dynamic objects!  Set the motion type to 'Dynamic' and try again.");
            return;
        }

        // Check the input parameters to determine what behaviors will be in use
        _applyLinearImpulse = (LinearImpulseDirection.LengthSquared() > 0.0f);
        _applyAngularImpulse = (AngularVariance.LengthSquared() > 0.0f);

        // Don't trust users to give us a normalized direction vector!
        if (_applyLinearImpulse)
            LinearImpulseDirection = LinearImpulseDirection.Normalized();

        // Apply the impulse force when the object is clicked, if it has been configured to use this mode
        if (InteractionImpulse)
        {
            ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, "", true);

            addData.Interaction.Subscribe((InteractionData data) =>
            {
                ApplyImpulse();
            });
        }

        // Apply the impulse force when a script event is received, if one has been specified
        if (!string.IsNullOrWhiteSpace(ImpulseScriptEvent))
        {
            SubscribeToScriptEvent(ImpulseScriptEvent, (ScriptEventData data) =>
            {
                // Force anyone holding the object to drop it
                _rb.ReleaseHeldObject();

                ApplyImpulse();
            });
        }
    }

    void ApplyImpulse()
    {
        if (_resetCoroutine != null)
        {
            _resetCoroutine.Abort();
            _resetCoroutine = null;
        }

        // Apply linear impulse force
        if (_applyLinearImpulse)
            _rb.AddLinearImpulse(LinearImpulseDirection * LinearForce);

        // Apply angular impulse force
        if (_applyAngularImpulse)
        {
            // Generate a new random angular impulse
            Vector angularImpulse = new Vector((float)_rng.NextDouble() * AngularVariance.X,
                                               (float)_rng.NextDouble() * AngularVariance.Y,
                                               (float)_rng.NextDouble() * AngularVariance.Z);

            // Apply the angular impulse if it is non-zero
            if (angularImpulse.LengthSquared() > 0.0f)
            {
                _rb.AddAngularImpulse(angularImpulse.Normalized() * AngularForce);
            }
        }

        if ((ResetTimeout > 0.0) && (_applyLinearImpulse || _applyAngularImpulse))
        {
            _resetCoroutine = StartCoroutine(ResetAfterTimeout);
        }
    }

    void ResetAfterTimeout()
    {
        Wait(ResetTimeout);

        _rb.SetMotionType(RigidBodyMotionType.MotionTypeKeyframed);
        _rb.SetAngularVelocity(Vector.Zero);
        _rb.SetLinearVelocity(Vector.Zero);
        _rb.SetPosition(ObjectPrivate.InitialPosition);
        _rb.SetOrientation(ObjectPrivate.InitialRotation);
        _rb.SetMotionType(RigidBodyMotionType.MotionTypeDynamic);
    }
}
