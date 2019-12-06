using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;
using System;
using System.Linq;

/*
 
 Description:
    A demonstration script for Materials API that responds to events to advance the frame of a flipbook, change the tint of an object and change it's emissive properties.
 Usage:
    Add script to mesh
    Configure Log Tag parameter if using multiple objects, to disambiguate script output

Valid propertyName values are:
                    
                    absorption (float value)
                    MaterialProperties.Absorption;

                    brightness (float value)
                    MaterialProperties.Brightness;

                    emissiveintensity (float value)
                    MaterialProperties.EmissiveIntensity;
                    
                    flipbookframe (float value)
                    MaterialProperties.FlipbookFrame;

                    tint  (color value)
                    MaterialProperties.Tint;

Supported interpolation modes are:
    easein      Ease In. The value will gradually speed up and continue quickly at the end.
    easeout     Ease Out. The value will change quickly at first and slow down to smoothly reach the goal.
    linear      Linear Interpolation. The value will change at a constant rate.
    smoothstep  Smoothstep Interpolation. The value will speed up gradually and slow down to smoothly reach the goal.
    stepStep    Step. The value will step abruptly from the initial value to the final value half-way through.

*/

public class MaterialDemo : SceneObjectScript
{

    #region EditorProperties
    [DisplayName("Log Tag")]
    [Tooltip("Tag to use when writing messages to debug console")]
    public readonly string LogTag = "";

    [DisplayName("Tint Command")]
    [Tooltip("Command to change tint of mesh")]
    [DefaultValue("change_tint")]
    public string TintCommand;

    [DisplayName("Color")]
    [Tooltip("Color to change tint of mesh. example: (0.35,0,0.88,1)")]
    [DefaultValue("(0.35,0,0.88,1)")]
    public string Color;

    [DisplayName("Emissive Command")]
    [Tooltip("Command to change emissive intensity of mesh")]
    [DefaultValue("emissive_intensity")]
    public string EmissiveCommand;

    [Tooltip("Emissive Intensity to change mesh")]
    [DefaultValue(3.0f)]
    [DisplayName("Emissive Intensity")]
    public readonly float EmissiveIntensity;

    [DisplayName("Flipbook Command")]
    [Tooltip("Command to play frames of a flipbook")]
    [DefaultValue("play_flipbook")]
    public string FlipbookCommand;

    [Tooltip("Total number of frames")]
    [DefaultValue(32.0f)]
    [DisplayName("Frames")]
    public readonly float Frames;

    [Tooltip("Time to execute transition in seconds")]
    [DefaultValue(3.0f)]
    [DisplayName("Transition Time")]
    public readonly float Duration;

    [DisplayName("Interpolation Mode")]
    [Tooltip("easein, easeout, linear, smoothstep, step")]
    [DefaultValue("smoothstep")]
    public string Interpolation;

    #endregion


    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    private MeshComponent Mesh = null;

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out Mesh))
        {
            Log.Write(LogLevel.Error, LogTag, "No MeshComponent found!  Aborting.");
            return;
        } else
        {
            List<RenderMaterial> materials = Mesh.GetRenderMaterials().ToList();
            if (materials.Count == 0)
            {
                Log.Write(LogLevel.Error, LogTag, "GetRenderMaterials() == null! Aborting.");
                return;
            }
            else
            {
                foreach (RenderMaterial material in materials)
                {
                    if (material == null)
                    {
                        Log.Write(LogTag, "Material is null");
                    }
                    else
                    {
                        Log.Write(LogTag, material.Name);
                        Log.Write(LogTag, material.ToString());
                    }
                }
            }

            Log.Write(LogTag, $"Mesh.IsScriptable={Mesh.IsScriptable}");

            if (!Mesh.IsScriptable)
            {
                Log.Write(LogLevel.Warning, LogTag, $"MeshComponent {Mesh.Name} is not scriptable");
            }

            SubscribeToScriptEvent(TintCommand, (ScriptEventData data) =>
            {
                ISimpleData idata = data.Data.AsInterface<ISimpleData>();
                
                RenderMaterial m = Mesh.GetRenderMaterial(materials[0].Name);
                MaterialProperties p = m.GetProperties();

                if (!Sansar.Color.TryParse(Color, out p.Tint))
                {
                    Log.Write(LogLevel.Error, LogTag, "TintCommand: Failed to parse as Sansar.Color");
                    return;
                }
                m.SetProperties(p, Duration, InterpolationModeParse(Interpolation));
            });

            SubscribeToScriptEvent(EmissiveCommand, (ScriptEventData data) =>
            {
                ISimpleData idata = data.Data.AsInterface<ISimpleData>();

                RenderMaterial m = Mesh.GetRenderMaterial(materials[0].Name);
                MaterialProperties p = m.GetProperties();

                p.EmissiveIntensity = EmissiveIntensity;
                m.SetProperties(p, Duration, InterpolationModeParse(Interpolation));
            });

        }
    }

    InterpolationMode InterpolationModeParse(string s)
    {
        s = s.ToLower();
        if (s == "easein") return InterpolationMode.EaseIn;
        if (s == "easeout") return InterpolationMode.EaseOut;
        if (s == "linear") return InterpolationMode.Linear;
        if (s == "smoothstep") return InterpolationMode.Smoothstep;
        if (s == "step") return InterpolationMode.Step;
        Log.Write(LogLevel.Warning, $"Unknown InterpolationMode '{s}'!  Using Linear...");
        return InterpolationMode.Linear;
    }
}
