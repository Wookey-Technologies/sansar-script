using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class PatrolMoverScript : SceneObjectScript
{
    public Vector Point1;
    public Vector Point2;
    public Vector Point3;
    public Vector Point4;

    [DefaultValue(1.0)]
    public double MoveTime;

    public override void Init()
    {
        StartCoroutine(PatrolUpdate);
    }

    void PatrolUpdate()
    {
        while (true)
        {
            ObjectPrivate.Mover.AddTranslate(Point1, MoveTime, MoveMode.Linear);
            ObjectPrivate.Mover.AddTranslate(Point2, MoveTime, MoveMode.Linear);
            ObjectPrivate.Mover.AddTranslate(Point3, MoveTime, MoveMode.Linear);
            WaitFor(ObjectPrivate.Mover.AddTranslate, Point4, MoveTime, MoveMode.Linear);
        }
    }
}
