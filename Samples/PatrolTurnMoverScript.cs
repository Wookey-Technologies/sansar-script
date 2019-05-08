// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class PatrolTurnMoverScript : SceneObjectScript
{
    public List<Vector> PatrolPoints;

    [DefaultValue(1.0f)]
    public float MoveSpeed;

    [DefaultValue("<0,1,0>")]
    public Vector WorldObjectForward;

    public override void Init()
    {
        if ((PatrolPoints.Count > 1) && (MoveSpeed > 0.0f))
            StartCoroutine(PatrolUpdate);
    }

    void PatrolUpdate()
    {
        int current = 0;
        int next = current + 1;

        // Start the object on the first patrol point
        ObjectPrivate.Mover.AddTranslate(PatrolPoints[current]);

        while (true)
        {
            // Calculate direction to next patrol point
            Vector toNext = PatrolPoints[next] - PatrolPoints[current];

            // Compute a world space rotation for this object to point at the next patrol point
            Quaternion rotation = Quaternion.ShortestRotation(WorldObjectForward, toNext.Normalized());
            ObjectPrivate.Mover.AddRotate(rotation);  // Immediately turn to face

            // Compute the time based on the distance and move speed
            double moveTime = toNext.Length() / MoveSpeed;

            // Move the object to the next patrol point
            WaitFor(ObjectPrivate.Mover.AddTranslate, PatrolPoints[next], moveTime, MoveMode.Linear);

            // Increment to the next patrol point
            current = next;
            next = (next + 1) % PatrolPoints.Count;
        }
    }
}
