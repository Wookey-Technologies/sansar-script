using Sansar.Script;
using Sansar.Simulation;

public class AnimationScript : SceneObjectScript
{
    public override void Init()
    {
        AnimationComponent animComp;
        if (ObjectPrivate.TryGetFirstComponent(out animComp))
            animComp.DefaultAnimation.Play();
        else
            Log.Write(LogLevel.Warning, "AnimationScript not running on an animated object!");
    }
}
