// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class LightOffScript : SceneObjectScript
{
    public override void Init()
    {
        LightComponent lightComp;
        if (!ObjectPrivate.TryGetFirstComponent(out lightComp))
            Log.Write(LogLevel.Warning, "LightScript not running on an object with a light!");
        else if (!lightComp.IsScriptable)
            Log.Write(LogLevel.Warning, "LightScript not running on an object with a scriptable light!");
        else
            lightComp.SetColorAndIntensity(Color.Black, 0.0f);  // turn the light "off"
    }
}
