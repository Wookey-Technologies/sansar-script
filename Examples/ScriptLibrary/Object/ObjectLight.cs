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
    [DisplayName("Light")]
    public class ObjectLight : ObjectBase
    {
        #region EditorProperties
        [Tooltip("Set the light properties as described below. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Set Light")]
        public readonly string ModeAEvent;

        [Tooltip("The color of the light")]
        [DisplayName("Color")]
        [DefaultValue(1,1,1,1)]
        public readonly Sansar.Color ModeAColor;

        [Tooltip("The intensity of the light")]
        [DefaultValue(60)]
        [DisplayName("Intensity")]
        [Range(0,100)]
        public readonly float ModeAIntensity;

        [Tooltip("Reset the light to its original parameters. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Reset Light")]
        public readonly string ModeBEvent;

        [Tooltip("The fade time of the light when changing modes")]
        [DefaultValue(0.0f)]
        [DisplayName("Fade Time")]
        [Range(0, 5)]
        public readonly float ModeFadeTime;

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

        private List<LightComponent> lights = null;
        private Action subscriptions = null;
        private bool AbortColorChange = false;
        private bool ColorChanging = false;
        private Sansar.Color[] InitialColors;
        private float[] InitialIntensities;
        private Action CancelTimer = null;

        protected override void SimpleInit()
        {
            lights = new List<LightComponent>();

            uint lightCount = ObjectPrivate.GetComponentCount(ComponentType.LightComponent);

            for (uint i = 0; i < lightCount; i++)
            {
                LightComponent lc = (LightComponent) ObjectPrivate.GetComponent(ComponentType.LightComponent, i);
                if (lc.IsScriptable)
                {
                    lights.Add(lc);
                    break;
                }
            }

            if (lights.Count == 0)
            {
                Log.Write(LogLevel.Error, "SimpleLight::Init", "Object must have at least one scriptable light added at edit time for SimpleLight script to work.");
                return;
            }

            InitialColors = new Sansar.Color[lights.Count];
            InitialIntensities = new float[lights.Count];
            for (int i=0; i<lights.Count;++i)
            {
                InitialColors[i] = lights[i].GetNormalizedColor();
                InitialIntensities[i] = lights[i].GetRelativeIntensity();
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);

        }

        private void SetColorAndIntensity(LightComponent light, Sansar.Color targetColor, float targetIntensity)
        {
            if (ModeFadeTime <= 0.1f)
            {
                light.SetColorAndIntensity(targetColor, targetIntensity);
            }
            else
            {
                int steps = (int)(ModeFadeTime / 0.1f);
                Sansar.Color currentColor = light.GetNormalizedColor();
                Sansar.Color colorDelta = (targetColor - currentColor) / steps;

                float currentIntensity = light.GetRelativeIntensity();
                float intensityDelta = (targetIntensity - currentIntensity) / steps;
                for(int i=0; i< steps; ++i)
                {
                    currentColor += colorDelta;
                    currentIntensity += intensityDelta;

                    light.SetColorAndIntensity(currentColor, currentIntensity);

                    Wait(TimeSpan.FromSeconds(0.1));
                    if (AbortColorChange) return;
                }

                // Ensure we reach the target:
                light.SetColorAndIntensity(targetColor, targetIntensity);
            }
        }

        private void ResetLights(ScriptEventData data)
        {
            if (ModeFadeTime > 0.0f)
            {
                StopInterpolation();
            }
            ColorChanging = true;
            CancelTimer = Timer.Create(TimeSpan.FromSeconds(ModeFadeTime), () => { ColorChanging = false; }).Unsubscribe;
            for (int i=0;i<lights.Count;++i)
            {
                StartCoroutine(SetColorAndIntensity, lights[i], InitialColors[i], InitialIntensities[i]);
            }
        }

        private void SetLights(ScriptEventData data)
        {
            if (ModeFadeTime > 0.0f)
            {
                StopInterpolation();
            }
            ColorChanging = true;
            CancelTimer = Timer.Create(TimeSpan.FromSeconds(ModeFadeTime), () => { ColorChanging = false; }).Unsubscribe;
            foreach (var light in lights)
            {
                StartCoroutine(SetColorAndIntensity, light, ModeAColor, ModeAIntensity);
            }
        }

        private void StopInterpolation()
        {
            if (ColorChanging)
            {
                if (CancelTimer != null)
                {
                    CancelTimer();
                    CancelTimer = null;
                }
                AbortColorChange = true;
                Wait(TimeSpan.FromSeconds(0.15));
                AbortColorChange = false;
                ColorChanging = false;
            }
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (subscriptions == null)
            {
                subscriptions = SubscribeToAll(ModeAEvent, SetLights);
                subscriptions += SubscribeToAll(ModeBEvent, ResetLights);
            }
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