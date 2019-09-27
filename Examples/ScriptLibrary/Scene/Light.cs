/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Control attached scriptable lights in response to simple script events.")]
    [DisplayName(nameof(Light))]
    public class Light : LibraryBase
    {
        #region EditorProperties

        [Tooltip("Set the lights this script will change.\nThe first scriptable light on this object will be used if no lights are set here.\nTo set a light, select the light from the object panel and copy it (ctrl-c) then add a new entry to this list, select it and paste (ctrl-v).")]
        [DisplayName("Lights")]
        public readonly List<LightComponent> LightComponents;

        [Tooltip("Set the light to Mode A on these events. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Mode A")]
        public readonly string ModeAEvent;

        [Tooltip("The color of the light for Mode A")]
        [DisplayName("Mode A Color")]
        [DefaultValue("(1,1,1,1)")]
        public readonly Sansar.Color ModeAColor;

        [Tooltip("The intensity of the light for Mode A")]
        [DefaultValue(60)]
        [DisplayName("Mode A Intensity")]
        [Range(0,100)]
        public readonly float ModeAIntensity;

        [Tooltip("The fade time of the light for Mode A")]
        [DefaultValue(0.0f)]
        [DisplayName("Mode A Fade Time")]
        [Range(0, 5)]
        public readonly float ModeAFadeTime;

        [Tooltip("Set the light to Mode B on these events. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Mode B")]
        public readonly string ModeBEvent;

        [Tooltip("The color of the light for Mode B")]
        [DisplayName("Mode B Color")]
        [DefaultValue("(0,0,0,0)")]
        public readonly Sansar.Color ModeBColor;

        [Tooltip("The intensity of the light for Mode B")]
        [DefaultValue(0)]
        [DisplayName("Mode B Intensity")]
        [Range(0, 100)]
        public readonly float ModeBIntensity;

        [Tooltip("The fade time of the light for Mode B")]
        [DefaultValue(0.0f)]
        [DisplayName("Mode B Fade Time")]
        [Range(0, 5)]
        public readonly float ModeBFadeTime;

        [Tooltip("Set the light to Mode C on these events. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Mode C")]
        public readonly string ModeCEvent;

        [Tooltip("The color of the light for Mode C")]
        [DisplayName("Mode C Color")]
        [DefaultValue("(0,0,0,0)")]
        public readonly Sansar.Color ModeCColor;

        [Tooltip("The intensity of the light for Mode C")]
        [DefaultValue(0)]
        [DisplayName("Mode C Intensity")]
        [Range(0, 100)]
        public readonly float ModeCIntensity;

        [Tooltip("The fade time of the light for Mode C")]
        [DefaultValue(0.0f)]
        [DisplayName("Mode C Fade Time")]
        [Range(0, 5)]
        public readonly float ModeCFadeTime;

        [Tooltip("Set the initial mode for this light from its signal name or by mode name: A or B or C")]
        [DefaultValue("B")]
        [DisplayName("Initial Mode Override")]
        public readonly string InitialModeEventName;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("light_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("light_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action subscriptions = null;

        Sansar.Color previousColor;
        float previousIntensity = 0.0f;
        Sansar.Color targetColor;
        float targetIntensity = 0.0f;
        float interpolationTime = 0.0f;
        float interpolationDuration = 0.0f;
        bool interpolationActive = false;
        ICoroutine interpolationCoroutine = null;

        protected override void SimpleInit()
        {
            if (LightComponents.Count > 0)
            {
                int removed = LightComponents.RemoveAll(light => light == null || !light.IsScriptable);
                if (removed > 0)
                {
                    Log.Write(LogLevel.Error, "Light::Init", $"{removed} lights were not set scriptable and will not be controllable by this script.");
                }
                if (LightComponents.Count == 0)
                {
                    Log.Write(LogLevel.Error, "Light::Init", "None of the selected Lights were scriptable.");
                    return;
                }
            }
            else
            {
                uint lightCount = ObjectPrivate.GetComponentCount(ComponentType.LightComponent);
                bool foundUnscriptable = false;
                for (uint i = 0; i < lightCount; i++)
                {
                    LightComponent lc = (LightComponent)ObjectPrivate.GetComponent(ComponentType.LightComponent, i);
                    if (lc.IsScriptable)
                    {
                        LightComponents.Add(lc);
                    }
                    else if (!foundUnscriptable)
                    {
                        Log.Write(LogLevel.Error, "Light::Init", "Some lights on this object are not scriptable (first found: " + lc.Name + ")");
                        foundUnscriptable = true;
                    }
                }

                if (LightComponents.Count == 0)
                {
                    Log.Write(LogLevel.Error, "Light::Init", "No scriptable lights found on this object: " + ObjectPrivate.Name);
                    return;
                }
            }
            
            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);

            // Potentially override the initial state of the light with A/B/C or an event specified for one of the modes.
            if (!string.IsNullOrWhiteSpace(InitialModeEventName))
            {
                if (InitialModeEventName.ToUpper() == "A" || ModeAEvent.Contains(InitialModeEventName))
                    SetColorAndIntensityOfAllLights(ModeAColor, ModeAIntensity);
                else if (InitialModeEventName.ToUpper() == "B" || ModeBEvent.Contains(InitialModeEventName))
                    SetColorAndIntensityOfAllLights(ModeBColor, ModeBIntensity);
                else if (InitialModeEventName.ToUpper() == "C" || ModeCEvent.Contains(InitialModeEventName))
                    SetColorAndIntensityOfAllLights(ModeCColor, ModeCIntensity);
            }
        }

        private void SetColorAndIntensityOfAllLights(Sansar.Color c, float intensity)
        {
            foreach (var light in LightComponents)
            {
                light.SetColorAndIntensity(c, intensity);
            }
        }

        private bool HasFadeTime()
        {
            return (ModeAFadeTime > 0.0f || ModeBFadeTime > 0.0f || ModeCFadeTime > 0.0f);
        }

        private void InterpolateLightColor()
        {
            const float deltaTime = 0.1f;
            TimeSpan ts = TimeSpan.FromSeconds(deltaTime);

            while (true)
            {
                Wait(ts);

                if (interpolationActive)
                {
                    interpolationTime = Math.Max(interpolationTime - deltaTime, 0.0f);

                    float t = interpolationTime / interpolationDuration;

                    Sansar.Color color = previousColor * t + targetColor * (1.0f - t);
                    float intensity = previousIntensity * t + targetIntensity * (1.0f - t);

                    SetColorAndIntensityOfAllLights(color, intensity);

                    interpolationActive = (interpolationTime > 0.0f);
                }
            }
        }

        private void StartInterpolation()
        {
            if ((interpolationCoroutine == null) && HasFadeTime())
            {
                targetColor = previousColor = LightComponents[0].GetNormalizedColor();
                targetIntensity = previousIntensity = LightComponents[0].GetRelativeIntensity();

                interpolationDuration = 0.0f;
                interpolationTime = 0.0f;
                interpolationActive = false;

                interpolationCoroutine = StartCoroutine(InterpolateLightColor);
            }
        }

        private void StopInterpolation()
        {
            if (interpolationCoroutine != null)
            {
                interpolationCoroutine.Abort();
                interpolationCoroutine = null;
            }
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (subscriptions == null)
            {
                subscriptions = SubscribeToAll(ModeAEvent, (data) =>
                {
                    if (ModeAFadeTime > 0.0f)
                    {
                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = ModeAColor;
                        targetIntensity = ModeAIntensity;
                        interpolationDuration = ModeAFadeTime;
                        interpolationTime = ModeAFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetColorAndIntensityOfAllLights(ModeAColor, ModeAIntensity);
                    }
                });

                subscriptions += SubscribeToAll(ModeBEvent, (data) =>
                {
                    if (ModeBFadeTime > 0.0f)
                    {
                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = ModeBColor;
                        targetIntensity = ModeBIntensity;
                        interpolationDuration = ModeBFadeTime;
                        interpolationTime = ModeBFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetColorAndIntensityOfAllLights(ModeBColor, ModeBIntensity);
                    }
                });

                subscriptions += SubscribeToAll(ModeCEvent, (data) =>
                {
                    if (ModeCFadeTime > 0.0f)
                    {
                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = ModeCColor;
                        targetIntensity = ModeCIntensity;
                        interpolationDuration = ModeCFadeTime;
                        interpolationTime = ModeCFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetColorAndIntensityOfAllLights(ModeCColor, ModeCIntensity);
                    }
                });
            }

            if (HasFadeTime())
                StartInterpolation();
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (subscriptions != null)
            {
                subscriptions();
                subscriptions = null;
            }

            StopInterpolation();
        }
    }
}