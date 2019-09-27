/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Simulation;
using Sansar.Script;
using Sansar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Turn attached scriptable lights on and off in response to simple script events.")]
    [DisplayName("Light On/Off")]
    public class LightOnOff : LibraryBase
    {
        #region EditorProperties
        [Tooltip("Set the lights this script will change.\nThe first scriptable light on this object will be used if no lights are set here.\nTo set a light, select the light from the object panel and copy it (ctrl-c) then add a new entry to this list, select it and paste (ctrl-v).")]
        [DisplayName("Lights")]
        public readonly List<LightComponent> LightComponents;

        [Tooltip("Event name to turn on the light. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Turn On")]
        public readonly string TurnOnEvent;

        [Tooltip("The fade time to turn on the light.")]
        [DefaultValue(0.1f)]
        [DisplayName("Turn On Fade Time")]
        [Range(0, 5)]
        public readonly float TurnOnFadeTime;

        [Tooltip("Event name to turn off the light. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Turn Off")]
        public readonly string TurnOffEvent;

        [Tooltip("The fade time to turn off the light.")]
        [DefaultValue(0.1f)]
        [DisplayName("Turn Off Fade Time")]
        [Range(0, 5)]
        public readonly float TurnOffFadeTime;

        [Tooltip("Set the light to a random color on these events. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Turn On Random")]
        public readonly string TurnRandomEvent;

        [Tooltip("The minimum intensity of the random light color.")]
        [DefaultValue(40)]
        [DisplayName("Random Min Intensity")]
        [Range(0, 100)]
        public readonly float MinRandomIntensity;

        [Tooltip("The maximum intensity of the random light color.")]
        [DefaultValue(60)]
        [DisplayName("Random Max Intensity")]
        [Range(0, 100)]
        public readonly float MaxRandomIntensity;

        [Tooltip("The fade time of the light for random light color.")]
        [DefaultValue(0.1f)]
        [DisplayName("Random Fade Time")]
        [Range(0, 5)]
        public readonly float TurnRandomFadeTime;

        [Tooltip("If true, the lights will be turned off when the world starts. Otherwise, the lights will be left on.")]
        [DefaultValue(true)]
        [DisplayName("Turn Off At Start")]
        public readonly bool TurnOffAtStart;

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

        Sansar.Color initialColor;
        float initialIntensity;

        float randomMinIntensity;
        float randomMaxIntensity;

        Sansar.Color previousColor;
        float previousIntensity = 0.0f;

        Sansar.Color targetColor;
        float targetIntensity = 0.0f;

        float interpolationTime = 0.0f;
        float interpolationDuration = 0.0f;
        bool interpolationActive = false;
        ICoroutine interpolationCoroutine = null;

        private Random rnd;

        protected override void SimpleInit()
        {
            rnd = new Random();

            if (LightComponents.Count > 0)
            {
                int removed = LightComponents.RemoveAll(light => light == null || !light.IsScriptable);
                if (removed > 0)
                {
                    Log.Write(LogLevel.Error, "LightOnOff::Init", $"{removed} lights were not set scriptable and will not be controllable by this script.");
                }
                if (LightComponents.Count == 0)
                {
                    Log.Write(LogLevel.Error, "LightOnOff::Init", "None of the selected Lights were scriptable.");
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
                        Log.Write(LogLevel.Error, "LightOnOff::Init", "Some lights on this object are not scriptable (first found: " + lc.Name + ")");
                        foundUnscriptable = true;
                    }
                }

                if (LightComponents.Count == 0)
                {
                    Log.Write(LogLevel.Error, "LightOnOff::Init", "No scriptable lights found on this object: " + ObjectPrivate.Name);
                    return;
                }
            }

            initialColor = LightComponents[0].GetNormalizedColor();
            initialIntensity = LightComponents[0].GetRelativeIntensity();

            randomMinIntensity = (MinRandomIntensity < MaxRandomIntensity ? MinRandomIntensity : MaxRandomIntensity);
            randomMaxIntensity = (MinRandomIntensity < MaxRandomIntensity ? MaxRandomIntensity : MinRandomIntensity);

            // Turn off the light if specified
            if (TurnOffAtStart) SetColorAndIntensityOfAllLights(Sansar.Color.Black, 0.0f);

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void SetColorAndIntensityOfAllLights(Sansar.Color c, float intensity)
        {
            foreach (var light in LightComponents)
            {
                light.SetColorAndIntensity(c, intensity);
            }
        }

        private void SetRandomColorAndIntensityOfAllLights()
        {
            foreach (var light in LightComponents)
            {
                // Pick a random color but don't let it be too dark or else the relative intensity doesn't work well
                Sansar.Vector randomVector = new Sansar.Vector((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                if (randomVector.LengthSquared() < 0.5f)
                    randomVector = randomVector.Normalized();
                Sansar.Color randomColor = new Sansar.Color(randomVector.X, randomVector.Y, randomVector.Z);

                // Pick a random intensity from min to max
                float randomIntensity = randomMinIntensity + (randomMaxIntensity - randomMinIntensity) * (float)rnd.NextDouble();

                light.SetColorAndIntensity(randomColor, randomIntensity);
            }
        }

        private bool HasFadeTime()
        {
            return (TurnOnFadeTime > 0.0f || TurnOffFadeTime > 0.0f || TurnRandomFadeTime > 0.0f);
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
                subscriptions = SubscribeToAll(TurnOnEvent, (data) =>
                {
                    if (TurnOnFadeTime > 0.0f)
                    {
                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = initialColor;
                        targetIntensity = initialIntensity;
                        interpolationDuration = TurnOnFadeTime;
                        interpolationTime = TurnOnFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetColorAndIntensityOfAllLights(initialColor, initialIntensity);
                    }
                });

                subscriptions += SubscribeToAll(TurnOffEvent, (data) =>
                {
                    if (TurnOffFadeTime > 0.0f)
                    {
                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = Sansar.Color.Black;
                        targetIntensity = 0.0f;
                        interpolationDuration = TurnOffFadeTime;
                        interpolationTime = TurnOffFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetColorAndIntensityOfAllLights(Sansar.Color.Black, 0.0f);
                    }
                });

                subscriptions += SubscribeToAll(TurnRandomEvent, (data) =>
                {
                    if (TurnRandomFadeTime > 0.0f)
                    {
                        // Pick a random color but don't let it be too dark or else the relative intensity doesn't work well
                        Sansar.Vector randomVector = new Sansar.Vector((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                        while (randomVector.LengthSquared() < 0.1f)
                            randomVector = new Sansar.Vector((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                        if (randomVector.LengthSquared() < 0.5f)
                            randomVector = randomVector.Normalized();
                        Sansar.Color randomColor = new Sansar.Color(randomVector.X, randomVector.Y, randomVector.Z);

                        // Pick a random intensity from min to max
                        float randomIntensity = randomMinIntensity + (randomMaxIntensity - randomMinIntensity) * (float)rnd.NextDouble();

                        previousColor = LightComponents[0].GetNormalizedColor();
                        previousIntensity = LightComponents[0].GetRelativeIntensity();
                        targetColor = randomColor;
                        targetIntensity = randomIntensity;
                        interpolationDuration = TurnRandomFadeTime;
                        interpolationTime = TurnRandomFadeTime;
                        interpolationActive = true;
                    }
                    else
                    {
                        interpolationActive = false;
                        SetRandomColorAndIntensityOfAllLights();
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