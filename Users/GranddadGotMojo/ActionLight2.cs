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
using System.Linq;
using System.Collections.Generic;

public class ActionLight : SceneObjectScript
{
    #region EditorProperties
    // Event, Color, Intensity, Fade Time, BlinkTime, PulseTime, Special 
    public string LightMode1 = null;
    public string LightMode2 = null;
    public string LightMode3 = null;
    public string LightMode4 = null;
    public string LightMode5 = null;
    public string LightMode6 = null;
    public string LightMode7 = null;
    public string LightMode8 = null;
    public string LightMode9 = null;
    public string LightMode10 = null;
    public string LightMode11 = null;
    public string LightMode12 = null;
    public string LightMode13 = null;
    public string LightMode14 = null;
    public string LightMode15 = null;
    public string LightMode16 = null;
    public string LightMode17 = null;
    public string LightMode18 = null;

    #endregion

    #region SimpleHelpers v2
    // Update the region tag above by incrementing the version when updating anything in the region.

    // If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
    // Does NOT accept CSV lists of groups.
    // To send or receive events to/from a specific group from outside that group prepend the group name with a > to the event name
    // my_group>on
    [DefaultValue("")]
    [DisplayName("Group")]
    public string Group = "";

    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    public interface IDebugger { bool DebugSimple { get; } }
    private bool __debugInitialized = false;
    private bool __SimpleDebugging = false;
    private string __SimpleTag = "";

    private string GenerateEventName(string eventName)
    {
        eventName = eventName.Trim();
        if (eventName.EndsWith("@"))
        {
            // Special case on@ to send the event globally (the null group) by sending w/o the @.
            return eventName.Substring(0, eventName.Length - 1);
        }
        else if (Group == "" || eventName.Contains("@"))
        {
            // No group was set or already targeting a specific group as is.
            return eventName;
        }
        else
        {
            // Append the group
            return $"{eventName}@{Group}";
        }
    }

    private void SetupSimple()
    {
        __debugInitialized = true;
        __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
        Wait(TimeSpan.FromSeconds(1));
        IDebugger debugger = ScenePrivate.FindReflective<IDebugger>("SimpleDebugger").FirstOrDefault();
        if (debugger != null) __SimpleDebugging = debugger.DebugSimple;
    }

    System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>> __subscribeActions = new System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>>();
    private Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return null;

        Func<string, Action<ScriptEventData>, Action> subscribeAction;
        if (__subscribeActions.TryGetValue(csv, out subscribeAction))
        {
            return subscribeAction(csv, callback);
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __subscribeActions[csv] = SubscribeToAllInternal;
            return SubscribeToAllInternal(csv, callback);
        }

        // Chaining
        __subscribeActions[csv] = (_csv, _callback) =>
        {
            System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));

            string initial = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(initial);

            Action unsub = null;
            Action<ScriptEventData> wrappedCallback = null;
            wrappedCallback = (data) =>
            {
                string first = chainedCommands[0];
                chainedCommands.RemoveAt(0);
                chainedCommands.Add(first);
                if (unsub != null) unsub();
                unsub = SubscribeToAllInternal(first, wrappedCallback);
                Log.Write(LogLevel.Info, "CHAIN Subscribing to " + first);
                _callback(data);
            };

            unsub = SubscribeToAllInternal(initial, wrappedCallback);
            return unsub;
        };

        return __subscribeActions[csv](csv, callback);
    }

    private Action SubscribeToAllInternal(string csv, Action<ScriptEventData> callback)
    {
        Action unsubscribes = null;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (__SimpleDebugging)
        {
            Log.Write(LogLevel.Info, __SimpleTag, "Subscribing to " + events.Length + " events: " + string.Join(", ", events));
        }
        Action<ScriptEventData> wrappedCallback = callback;

        foreach (string eventName in events)
        {
            if (__SimpleDebugging)
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), (ScriptEventData data) =>
                {
                    Log.Write(LogLevel.Info, __SimpleTag, "Received event " + GenerateEventName(eventName));
                    wrappedCallback(data);
                });
                unsubscribes += sub.Unsubscribe;
            }
            else
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), wrappedCallback);
                unsubscribes += sub.Unsubscribe;
            }
        }
        return unsubscribes;
    }

    System.Collections.Generic.Dictionary<string, Action<string, Reflective>> __sendActions = new System.Collections.Generic.Dictionary<string, Action<string, Reflective>>();
    private void SendToAll(string csv, Reflective data)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return;

        Action<string, Reflective> sendAction;
        if (__sendActions.TryGetValue(csv, out sendAction))
        {
            sendAction(csv, data);
            return;
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __sendActions[csv] = SendToAllInternal;
            SendToAllInternal(csv, data);
            return;
        }

        // Chaining
        System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));
        __sendActions[csv] = (_csv, _data) =>
        {
            string first = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(first);

            Log.Write(LogLevel.Info, "CHAIN Sending to " + first);
            SendToAllInternal(first, _data);
        };
        __sendActions[csv](csv, data);
    }

    private void SendToAllInternal(string csv, Reflective data)
    {
        if (string.IsNullOrWhiteSpace(csv)) return;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (__SimpleDebugging) Log.Write(LogLevel.Info, __SimpleTag, "Sending " + events.Length + " events: " + string.Join(", ", events) + (Group != "" ? (" to group " + Group) : ""));
        foreach (string eventName in events)
        {
            PostScriptEvent(GenerateEventName(eventName), data);
        }
    }
    #endregion

    LightComponent light = null;
    LightComponent light2 = null;
    //Action subscriptions = null;

    bool SecondLight = false;

    // Event, Color, Intensity, Fade Time, BlinkTime, PulseTime, Special 
    private string[] LightEvent = new string[18];
    private Sansar.Color[] LightColor = new Sansar.Color[18];
    private bool[] RandomColor = new bool[18];
    private string[] LightIntensity = new string[18];
    private string[] LightAngle = new string[18];
    private string[] LightFallOff = new string[18]; 
    private string[] Effect = new string[18];
    private string[] EffectTime = new string[18];
    private string[] EffectParm = new string[18];
    private string[] BaseLightIntensity = new string[18];

    private float deltaTime = 0.1f;
    private float fltEffectTime = 0.0f;
    private float fltLightIntensity = 0.0f;
    private float fltLightAngle = 90.0f;
    private float fltLightFallOff = 0.5f;
    private float fltBaseLightIntensity = 0.0f;
    Sansar.Color targetColor;
    private float interpolationTime = 0.0f;
    private bool spotActive = false;
    private bool fadeActive = false;
    private bool blinkActive = false;
    private bool pulseActive = false;
    private Random random = new Random();

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out light))
        {
            Log.Write(LogLevel.Error, "SimpleLight::Init", "Object must have a scriptable light added at edit time for SimpleLight script to work.");
            return;
        }

        if (ObjectPrivate.TryGetComponent(1, out light2))
        {
            SecondLight = true;
            //Log.Write("Second Light");
        }  

        if (!light.IsScriptable)
        {
            Log.Write(LogLevel.Error, "SimpleLight::Init", "Light must be set to scriptable at edit time for SimpleLight script to work.");
            return;
        }
        if (LightMode1.Length > 0) ParseLights(0, LightMode1);
        if (LightMode2.Length > 0) ParseLights(1, LightMode2);
        if (LightMode3.Length > 0) ParseLights(2, LightMode3);
        if (LightMode4.Length > 0) ParseLights(3, LightMode4);
        if (LightMode5.Length > 0) ParseLights(4, LightMode5);
        if (LightMode6.Length > 0) ParseLights(5, LightMode6);
        if (LightMode7.Length > 0) ParseLights(6, LightMode7);
        if (LightMode8.Length > 0) ParseLights(7, LightMode8);
        if (LightMode9.Length > 0) ParseLights(8, LightMode9);
        if (LightMode10.Length > 0) ParseLights(9, LightMode10);
        if (LightMode11.Length > 0) ParseLights(10, LightMode11);
        if (LightMode12.Length > 0) ParseLights(11, LightMode12);
        if (LightMode13.Length > 0) ParseLights(12, LightMode13);
        if (LightMode14.Length > 0) ParseLights(13, LightMode14);
        if (LightMode15.Length > 0) ParseLights(14, LightMode15);
        if (LightMode16.Length > 0) ParseLights(15, LightMode16);
        if (LightMode17.Length > 0) ParseLights(16, LightMode17);
        if (LightMode18.Length > 0) ParseLights(17, LightMode18);

    }

    private void ParseLights(int LightProgram, string LightMode)
    {
        //Log.Write("In ParseLight LightProgram: " + LightProgram + "  LightModeString: " + LightMode);
        List<string> LightArray = new List<string>();
        LightArray.Clear();
        LightArray = LightMode.Split(',').ToList();
        LightEvent[LightProgram] = LightArray[0];
        //Log.Write("LightEvent: " + LightArray[0]);
        LightIntensity[LightProgram] = LightArray[1];
        if (LightArray[2] == "random")
        {
            RandomColor[LightProgram] = true;
            LightColor[LightProgram].R = (float) random.NextDouble();
            LightColor[LightProgram].G = (float) random.NextDouble();
            LightColor[LightProgram].B = (float) random.NextDouble();
            LightColor[LightProgram].A = (float) Int32.Parse(LightArray[5]);
        }
        else
        {
            RandomColor[LightProgram] = false;
            LightColor[LightProgram].R = Int32.Parse(LightArray[2]);
            LightColor[LightProgram].G = Int32.Parse(LightArray[3]);
            LightColor[LightProgram].B = Int32.Parse(LightArray[4]);
            LightColor[LightProgram].A = Int32.Parse(LightArray[5]);
        }

        LightAngle[LightProgram] = LightArray[6];
        LightFallOff[LightProgram] = LightArray[7];
        Effect[LightProgram] = LightArray[8];
        EffectTime[LightProgram] = LightArray[9];
        if ((LightArray[8] == "fade") || (LightArray[8] == "pulse"))
        {
            EffectParm[LightProgram] = LightArray[10];
            //Log.Write("EffectParm[" + LightProgram + "]: " + EffectParm[LightProgram]);
            BaseLightIntensity[LightProgram] = LightArray[11];
        }

        SubscribeToAll(LightEvent[LightProgram], ExecuteLightProgram);
        //Log.Write("Finished Parsing Light Program");
    }

    private void ExecuteLightProgram(ScriptEventData data)
    {
        //Log.Write("In Execute Light data message: " + data.Message);

        //Log.Write("Animation Event: " + AnimationEvent[0]);
        if (data.Message == LightEvent[0]) PlayLightProgram(0, data);
        else if (data.Message == LightEvent[1]) PlayLightProgram(1, data);
        else if (data.Message == LightEvent[2]) PlayLightProgram(2, data);
        else if (data.Message == LightEvent[3]) PlayLightProgram(3, data);
        else if (data.Message == LightEvent[4]) PlayLightProgram(4, data);
        else if (data.Message == LightEvent[5]) PlayLightProgram(5, data);
        else if (data.Message == LightEvent[6]) PlayLightProgram(6, data);
        else if (data.Message == LightEvent[7]) PlayLightProgram(7, data);
        else if (data.Message == LightEvent[8]) PlayLightProgram(8, data);
        else if (data.Message == LightEvent[9]) PlayLightProgram(9, data);
        else if (data.Message == LightEvent[10]) PlayLightProgram(10, data);
        else if (data.Message == LightEvent[11]) PlayLightProgram(11, data);
        else if (data.Message == LightEvent[12]) PlayLightProgram(12, data);
        else if (data.Message == LightEvent[13]) PlayLightProgram(13, data);
        else if (data.Message == LightEvent[14]) PlayLightProgram(14, data);
        else if (data.Message == LightEvent[15]) PlayLightProgram(15, data);
        else if (data.Message == LightEvent[16]) PlayLightProgram(16, data);
        else if (data.Message == LightEvent[17]) PlayLightProgram(17, data);
    }

    private void PlayLightProgram(int LightProgram, ScriptEventData data)
    {
        fltEffectTime = float.Parse(EffectTime[LightProgram]);
        fltLightIntensity = float.Parse(LightIntensity[LightProgram]);
        fltLightAngle = float.Parse(LightAngle[LightProgram]);
        fltLightFallOff = float.Parse(LightFallOff[LightProgram]);

        spotActive = false;
        blinkActive = false;
        fadeActive = false;
        pulseActive = false;
        light.SetAngle(fltLightAngle);
        light.SetAngularFalloff(fltLightFallOff);
        if (RandomColor[LightProgram])
        {
            targetColor.R = (float)random.NextDouble();
            targetColor.G = (float)random.NextDouble();
            targetColor.B = (float)random.NextDouble();
            targetColor.A = LightColor[LightProgram].A;
        }
        else
        {
            targetColor = LightColor[LightProgram];
        }

        if (Effect[LightProgram] == "fade")
        {
            //Log.Write("Fade LightProgram:" + LightProgram);
            //Log.Write("Color: " + LightColor[LightProgram]);
            fltBaseLightIntensity = float.Parse(BaseLightIntensity[LightProgram]);
            //Log.Write("deltaTime: " + EffectParm[LightProgram]);
            deltaTime = float.Parse(EffectParm[LightProgram]);
            //Log.Write("deltaTime: " + deltaTime);
            FadeIn(fltEffectTime, deltaTime);
        }
        if (Effect[LightProgram] == "pulse")
        {
            Log.Write("Pulse1");
            fltBaseLightIntensity = float.Parse(BaseLightIntensity[LightProgram]);
            Log.Write("Pulse2");
            Pulse(fltEffectTime, deltaTime, RandomColor[LightProgram]);
        }
        if (Effect[LightProgram] == "blink")
        {
            Blink(fltEffectTime, RandomColor[LightProgram]);
        } 
        if (Effect[LightProgram] == "spot")
        {
            Spot(fltEffectTime, RandomColor[LightProgram]);
        }
    }

    private void Spot(float fltEffectTimeIn, bool RandomColor)
    {
        //Log.Write("In Spot");
        TimeSpan ts = TimeSpan.FromSeconds(fltEffectTimeIn);
        spotActive = true;
        //Log.Write("fltEffectTimeIn: " + fltEffectTimeIn);

        if (fltEffectTimeIn < 0.01f)
        {
            //Log.Write("In static spot");
            spotActive = false;
            if (RandomColor)
            {
                targetColor.R = (float)random.NextDouble();
                targetColor.G = (float)random.NextDouble();
                targetColor.B = (float)random.NextDouble();
                targetColor.A = 1;
            }
            light.SetColorAndIntensity(targetColor, fltLightIntensity);
            if (SecondLight) light2.SetColorAndIntensity(targetColor, fltLightIntensity / 4);
        }
        else
        {
            while (spotActive)
            {
                if (RandomColor)
                {
                    targetColor.R = (float)random.NextDouble();
                    targetColor.G = (float)random.NextDouble();
                    targetColor.B = (float)random.NextDouble();
                    targetColor.A = 1;
                }
                light.SetColorAndIntensity(targetColor, fltLightIntensity);
                if (SecondLight) light2.SetColorAndIntensity(targetColor, fltLightIntensity / 4);
                Wait(ts);
            }
        }
    }

    private void Blink(float fltEffectTimeIn, bool RandomColor)
    {
        //Log.Write("In Blink");
        TimeSpan ts = TimeSpan.FromSeconds(fltEffectTimeIn);
        blinkActive = true;

        while (blinkActive)
        {
            //Log.Write("Blink Intensity: " + fltLightIntensity);
            //Log.Write("Blink Color: " + targetColor);
            if (RandomColor)
            {
                targetColor.R = (float)random.NextDouble();
                targetColor.G = (float)random.NextDouble();
                targetColor.B = (float)random.NextDouble();
                targetColor.A = 1;
            }
            light.SetColorAndIntensity(targetColor, fltLightIntensity);
            if (SecondLight) light2.SetColorAndIntensity(targetColor, fltLightIntensity);
            Wait(ts);
            if (!blinkActive) break;
            light.SetColorAndIntensity(targetColor, 0.0f);
            if (SecondLight) light2.SetColorAndIntensity(targetColor, 0.0f/4);
            Wait(ts);
        }
    }

    private void Pulse(float fltEffectTimeIn, float deltaTimeIn, bool RandomColor)
    {
        //Log.Write("In Pulse");
        pulseActive = true;

        while (pulseActive)
        {
            if (RandomColor)
            {
                targetColor.R = (float)random.NextDouble();
                targetColor.G = (float)random.NextDouble();
                targetColor.B = (float)random.NextDouble();
                targetColor.A = 1;
            }
            FadeIn(fltEffectTimeIn, deltaTimeIn);
            if (!pulseActive) break;
            FadeOut(fltEffectTimeIn, deltaTimeIn);
        }
    }

    private void FadeIn(float fltEffectTimeIn, float deltaTimeIn)
    {
        TimeSpan ts = TimeSpan.FromSeconds(deltaTimeIn);
        //Log.Write("In FadeIn");
        float timeLeft = fltEffectTimeIn;
        float AdjustIntensity = fltLightIntensity / fltBaseLightIntensity;
        //Log.Write("fltLightIntensity: " + fltLightIntensity);
        //Log.Write("fltBaseLightIntensity: " + fltBaseLightIntensity);
        //Log.Write("AdjustIntensity: " + AdjustIntensity);
        fadeActive = true;
        light.SetColorAndIntensity(targetColor, fltBaseLightIntensity);

        while (fadeActive)
        {
            Wait(ts);  //wait a tenth of a second
            timeLeft = timeLeft - deltaTimeIn;
            //Log.Write("timeLeft: " + timeLeft);
            interpolationTime = Math.Max(timeLeft, 0.0f);
            //Log.Write("interpolationTime: " + interpolationTime);
            float t = interpolationTime / fltEffectTimeIn;
            //Log.Write("t: " + t);
            float intensity = fltLightIntensity * (1.0f - t) / AdjustIntensity  + fltBaseLightIntensity;
            //Log.Write("intensity: " + intensity);
            light.SetColorAndIntensity(targetColor, intensity);
            if (SecondLight) light2.SetColorAndIntensity(targetColor, intensity / 4);
            fadeActive = (interpolationTime > 0.0f);
        }
    }

    private void FadeOut(float fltEffectTimeIn, float deltaTimeIn)
    {
        TimeSpan ts = TimeSpan.FromSeconds(deltaTimeIn);
        //Log.Write("In FadeOut");
        float timeLeft = fltEffectTimeIn;
        float AdjustIntensity = fltLightIntensity / fltBaseLightIntensity;
        fadeActive = true;

        while (fadeActive)
        {
            Wait(ts);  //wait a tenth of a second
            timeLeft = timeLeft - deltaTimeIn;
            //Log.Write("timeLeft: " + timeLeft);
            interpolationTime = Math.Max(timeLeft, 0.0f);
            //Log.Write("interpolationTime: " + interpolationTime);
            float t = interpolationTime / fltEffectTimeIn;
            //Log.Write("t: " + t);
            float intensity = fltLightIntensity * (t) / AdjustIntensity + fltBaseLightIntensity;
            //Log.Write("intensity: " + intensity);
            light.SetColorAndIntensity(targetColor, intensity);
            if (SecondLight) light2.SetColorAndIntensity(targetColor, intensity / 4);
            fadeActive = (interpolationTime > 0.0f);
        }
    }
}