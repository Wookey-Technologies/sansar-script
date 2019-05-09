// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class InfiniteRotation : SceneObjectScript
{
    // Public properties

    [DefaultValue("<1,1,0>")]
    public Vector RotationAxis;

    [Tooltip("Length of time for one complete rotation, in seconds.")]
    [DefaultValue(60.0)]
    public readonly double RotationDuration;

    // Logic!

    public override void Init()
    {
        // Write an error to the debug console if the object is not set to movable
        if (!ObjectPrivate.IsMovable)
        {
            Log.Write($"InfiniteRotation script can't move {ObjectPrivate.Name} because the 'Movable from Script' flag was not set!");
            return;
        }

        StartCoroutine(UpdateRotation);
    }

    void UpdateRotation()
    {
        double OneThirdRotationDuration = RotationDuration / 3.0;

        Vector rotationAxis = Vector.Up;
        if (RotationAxis.LengthSquared() > 0.0f)
            rotationAxis = RotationAxis.Normalized();

        while (true)
        {
            Quaternion rotation1 = Quaternion.FromAngleAxis(Mathf.TwoPi * 1.0f / 3.0f, rotationAxis) * ObjectPrivate.InitialRotation;
            Quaternion rotation2 = Quaternion.FromAngleAxis(Mathf.TwoPi * 2.0f / 3.0f, rotationAxis) * ObjectPrivate.InitialRotation;
            Quaternion rotation3 = ObjectPrivate.InitialRotation;

            ObjectPrivate.Mover.AddRotate(rotation1, OneThirdRotationDuration, MoveMode.Linear);
            ObjectPrivate.Mover.AddRotate(rotation2, OneThirdRotationDuration, MoveMode.Linear);
            WaitFor(ObjectPrivate.Mover.AddRotate, rotation3, OneThirdRotationDuration, MoveMode.Linear);
        }
    }
}
