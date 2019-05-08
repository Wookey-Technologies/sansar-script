// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;
using System.Linq;

public class ReflectiveCallerScript : SceneObjectScript
{
    public interface IButton { void SetButtonEnabled(bool enabled); }

    public override void Init()
    {
        IButton[] buttons = ScenePrivate.FindReflective<IButton>("ReflectiveReceiverScript").ToArray();
        foreach (IButton b in buttons)
        {
            b.SetButtonEnabled(false);
        }
    }
}
