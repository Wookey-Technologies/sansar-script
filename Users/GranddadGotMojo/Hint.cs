//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// My Documentation


public class CustomInstrumentController : SceneObjectScript

{
    #region ConstantsVariables
    public readonly Interaction ComplexInteraction;
    public string ChannelOut = "K1";
    //public Vector InitialPosition;
    //public Quaternion InitialOrientation

    public bool Debug = false;

    private string ControlSurfaceR0C1 = "Patch1,45.2085,-32.9175,56.9315,-39.4825,56.9315,-32.9175,45.2085,-32.9175,0,20";
    private string ControlSurfaceR0C2 = "Patch2,30.8455,-39.4825,42.5685,-39.4825,42.5685,-32.9175,30.8455,-32.9175,0,20";
    private string ControlSurfaceR0C3 = "Scales1,15.9285,-39.4825,27.6515,-39.4825,27.6515,-32.9175,15.9285,-32.9175,0,20";
    private string ControlSurfaceR0C4 = "Chords1,1.5865,-39.4825,13.3095,-39.4825,13.3095,-32.9175,1.5865,-32.9175,0,20";
    private string ControlSurfaceR0C5 = "Blues,-13.3095,-39.4825,-1.5865,-39.4825,-1.5865,-32.9175,-13.3095,-32.9175,0,20";
    private string ControlSurfaceR0C6 = "Jazz,-27.8385,-39.4825,-16.1155,-39.4825,-16.1155,-32.9175,-27.8385,-32.9175,0,20";
    private string ControlSurfaceR0C7 = "Rock,-42.4515,-39.4825,-30.7285,-39.4825,-30.7285,-32.9175,-42.4515,-32.9175,0,20";
    private string ControlSurfaceR0C8 = "Country,-56.9615,-39.4825,-45.2385,-39.4825,-45.2385,-32.9175,-56.9615,-32.9175,0,20";
    private string ControlSurfaceR1C1 = "GrandPiano,45.2085,-26.2825,56.9315,-26.2825,56.9315,-19.7175,45.2085,-19.7175,0,20";
    private string ControlSurfaceR1C2 = "B3Organ,30.8455,-26.2825,42.5685,-26.2825,42.5685,-19.7175,30.8455,-19.7175,0,20";
    private string ControlSurfaceR1C3 = "AcousticBass,15.9285,-26.2825,27.6515,-26.2825,27.6515,-19.7175,15.9285,-19.7175,0,20";
    private string ControlSurfaceR1C4 = "AcousticGuitar,1.5865,-26.2825,13.3095,-26.2825,13.3095,-19.7175,1.5865,-19.7175,0,20";
    private string ControlSurfaceR1C5 = "Pizzicato,-13.3095,-26.2825,-1.5865,-26.2825,-1.5865,-19.7175,-13.3095,-19.7175,0,20";
    private string ControlSurfaceR1C6 = "Clarinet,-27.8385,-26.2825,-16.1155,-26.2825,-16.1155,-19.7175,-27.8385,-19.7175,0,20";
    private string ControlSurfaceR1C7 = "AltoSax,-42.4515,-26.2825,-30.7285,-26.2825,-30.7285,-19.7175,-42.4515,-19.7175,0,20";
    private string ControlSurfaceR1C8 = "Saw,-56.9615,-26.2825,-45.2385,-26.2825,-45.2385,-19.7175,-56.9615,-19.7175,0,20";
    private string ControlSurfaceR2C1 = "BrightPiano,45.2085,-16.3825,56.9315,-16.3825,56.9315,-9.8175,45.2085,-9.8175,0,20";
    private string ControlSurfaceR2C2 = "RockOrgan,30.8455,-16.3825,42.5685,-16.3825,42.5685,-9.8175,30.8455,-9.8175,0,20";
    private string ControlSurfaceR2C3 = "JazzBass,15.9285,-16.3825,27.6515,-16.3825,27.6515,-9.8175,15.9285,-9.8175,0,20";
    private string ControlSurfaceR2C4 = "NylonGuitar,1.5865,-16.3825,13.3095,-16.3825,13.3095,-9.8175,1.5865,-9.8175,0,20";
    private string ControlSurfaceR2C5 = "Strings,-13.3095,-16.3825,-1.5865,-16.3825,-1.5865,-9.8175,-13.3095,-9.8175,0,20";
    private string ControlSurfaceR2C6 = "Oboe,-27.8385,-16.3825,-16.1155,-16.3825,-16.1155,-9.8175,-27.8385,-9.8175,0,20";
    private string ControlSurfaceR2C7 = "TenorSax,-42.4515,-16.3825,-30.7285,-16.3825,-30.7285,-9.8175,-42.4515,-9.8175,0,20";
    private string ControlSurfaceR2C8 = "Square,-56.9615,-16.3825,-45.2385,-16.3825,-45.2385,-9.8175,-56.9615,-9.8175,0,20";
    private string ControlSurfaceR3C1 = "ElectricPiano1,45.2085,-6.0825,56.9315,-6.0825,56.9315,0.4825,45.2085,0.4825,0,20";
    private string ControlSurfaceR3C2 = "ChurchOrgan,30.8455,-6.0825,42.5685,-6.0825,42.5685,0.4825,30.8455,0.4825,0,20";
    private string ControlSurfaceR3C3 = "SlapBass,15.9285,-6.0825,27.6515,-6.0825,27.6515,0.4825,15.9285,0.4825,0,20";
    private string ControlSurfaceR3C4 = "CleanGuitar,1.5865,-6.0825,13.3095,-6.0825,13.3095,0.4825,1.5865,0.4825,0,20";
    private string ControlSurfaceR3C5 = "SynthStrings,-13.3095,-6.0825,-1.5865,-6.0825,-1.5865,0.4825,-13.3095,0.4825,0,20";
    private string ControlSurfaceR3C6 = "Flute,-27.8385,-6.0825,-16.1155,-6.0825,-16.1155,0.4825,-27.8385,0.4825,0,20";
    private string ControlSurfaceR3C7 = "Trombone,-42.4515,-6.0825,-30.7285,-6.0825,-30.7285,0.4825,-42.4515,0.4825,0,20";
    private string ControlSurfaceR3C8 = "FairLight,-56.9615,-6.0825,-45.2385,-6.0825,-45.2385,0.4825,-56.9615,0.4825,0,20";
    private string ControlSurfaceR4C1 = "ElectricPiano2,45.2085,4.0675,56.9315,4.0675,56.9315,10.6325,45.2085,10.6325,0,20";
    private string ControlSurfaceR4C2 = "Accordion,30.8455,4.0675,42.5685,4.0675,42.5685,10.6325,30.8455,10.6325,0,20";
    private string ControlSurfaceR4C3 = "PickBass,15.9285,4.0675,27.6515,4.0675,27.6515,10.6325,15.9285,10.6325,0,20";
    private string ControlSurfaceR4C4 = "Distortion,1.5865,4.0675,13.3095,4.0675,13.3095,10.6325,1.5865,10.6325,0,20";
    private string ControlSurfaceR4C5 = "Cello,-13.3095,4.0675,-1.5865,4.0675,-1.5865,10.6325,-13.3095,10.6325,0,20";
    private string ControlSurfaceR4C6 = "EnglishHorn,-27.8385,4.0675,-16.1155,4.0675,-16.1155,10.6325,-27.8385,10.6325,0,20";
    private string ControlSurfaceR4C7 = "Trumpet,-42.4515,4.0675,-30.7285,4.0675,-30.7285,10.6325,-42.4515,10.6325,0,20";
    private string ControlSurfaceR4C8 = "Prophet,-56.9615,4.0675,-45.2385,4.0675,-45.2385,10.6325,-56.9615,10.6325,0,20";
    private string ControlSurfaceR5C1 = "Vibes,45.2085,14.8175,56.9315,14.8175,56.9315,21.3825,45.2085,21.3825,0,20";
    private string ControlSurfaceR5C2 = "SteelDrums,30.8455,14.8175,42.5685,14.8175,42.5685,21.3825,30.8455,21.3825,0,20";
    private string ControlSurfaceR5C3 = "SynthBass,15.9285,14.8175,27.6515,14.8175,27.6515,21.3825,15.9285,21.3825,0,20";
    private string ControlSurfaceR5C4 = "OverDrive,1.5865,14.8175,13.3095,14.8175,13.3095,21.3825,1.5865,21.3825,0,20";
    private string ControlSurfaceR5C5 = "Violin,-13.3095,14.8175,-1.5865,14.8175,-1.5865,21.3825,-13.3095,21.3825,0,20";
    private string ControlSurfaceR5C6 = "FrenchHorn,-27.8385,14.8175,-16.1155,14.8175,-16.1155,21.3825,-27.8385,21.3825,0,20";
    private string ControlSurfaceR5C7 = "Tuba,-42.4515,14.8175,-30.7285,14.8175,-30.7285,21.3825,-42.4515,21.3825,0,20";
    private string ControlSurfaceR5C8 = "FX7,-56.9615,14.8175,-45.2385,14.8175,-45.2385,21.3825,-56.9615,21.3825,0,20";

    private float[] ControlSurfaceAXRelative = new float[60];
    private float[] ControlSurfaceAYRelative = new float[60];
    private float[] ControlSurfaceBXRelative = new float[60];
    private float[] ControlSurfaceBYRelative = new float[60];
    private float[] ControlSurfaceCXRelative = new float[60];
    private float[] ControlSurfaceCYRelative = new float[60];
    private float[] ControlSurfaceDXRelative = new float[60];
    private float[] ControlSurfaceDYRelative = new float[60];
    private float[] ControlSurfaceZMinimum = new float[60];
    private float[] ControlSurfaceZMaximum = new float[60];
    private float[] ControlSurfaceAXRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceAYRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceBXRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceBYRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceCXRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceCYRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceDXRelativeAfterRotation = new float[60];
    private float[] ControlSurfaceDYRelativeAfterRotation = new float[60];

    private bool TrackSelected = false;
    private string SelectedTrackIn;
    private SoundResource SelectedSoundResource;
    private string SelectedSampleName;

    private string UsersToListenTo = "ALL";
    private List<string> ValidUsers = new List<string>();
    private AudioComponent audio;
    private PlaySettings playSettings = PlaySettings.PlayOnce;
    bool validUser = false;

    private string[] ControlSurfaceMessage = new string[60];
    private AgentPrivate Hitman;
    private AgentInfo Jammer;

    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private static readonly Vector WarehouseRot = new Vector(0.0f, 0.0f, 0.0f);
    Quaternion RotQuat = Quaternion.FromEulerAngles(WarehouseRot).Normalized();
    private double ZRotation = new double();
    private int NumOfControlSurfaces = 0;
    private string Change = null;
    private PlayHandle currentPlayHandle;

    public class CurrentInstrumentInfo : Reflective
    {
        public AgentPrivate HitmanOut { get; set; }
        public string ChannelOut { get; set; }
        public string CurrentInstrument { get; set; }
    }

    #endregion

    public override void Init()
    {
        Log.Write("In CustomInstrumentController");
        ComplexInteraction.SetPrompt(".");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        ObjectPrivate.TryGetComponent<AudioComponent>(0, out audio);
        
        SubscribeToScriptEvent("SecurityList", getSecurity);
        InitializeComplexInteraction();

    }

    private void InitializeComplexInteraction()
    {

        CurPos = ObjectPrivate.InitialPosition;
        ZRotation = ObjectPrivate.InitialRotation.GetEulerAngles().Z * 57.2958;

        LoadControlSurfaces(ControlSurfaceR0C1, 0);
        LoadControlSurfaces(ControlSurfaceR0C2, 1);
        LoadControlSurfaces(ControlSurfaceR0C3, 2);
        LoadControlSurfaces(ControlSurfaceR0C4, 3);
        LoadControlSurfaces(ControlSurfaceR0C5, 4);
        LoadControlSurfaces(ControlSurfaceR0C6, 5);
        LoadControlSurfaces(ControlSurfaceR0C7, 6);
        LoadControlSurfaces(ControlSurfaceR0C8, 7);
        LoadControlSurfaces(ControlSurfaceR1C1, 8);
        LoadControlSurfaces(ControlSurfaceR1C2, 9);
        LoadControlSurfaces(ControlSurfaceR1C3, 10);
        LoadControlSurfaces(ControlSurfaceR1C4, 11);
        LoadControlSurfaces(ControlSurfaceR1C5, 12);
        LoadControlSurfaces(ControlSurfaceR1C6, 13);
        LoadControlSurfaces(ControlSurfaceR1C7, 14);
        LoadControlSurfaces(ControlSurfaceR1C8, 15);
        LoadControlSurfaces(ControlSurfaceR2C1, 16);
        LoadControlSurfaces(ControlSurfaceR2C2, 17);
        LoadControlSurfaces(ControlSurfaceR2C3, 18);
        LoadControlSurfaces(ControlSurfaceR2C4, 19);
        LoadControlSurfaces(ControlSurfaceR2C5, 20);
        LoadControlSurfaces(ControlSurfaceR2C6, 21);
        LoadControlSurfaces(ControlSurfaceR2C7, 22);
        LoadControlSurfaces(ControlSurfaceR2C8, 23);
        LoadControlSurfaces(ControlSurfaceR3C1, 24);
        LoadControlSurfaces(ControlSurfaceR3C2, 25);
        LoadControlSurfaces(ControlSurfaceR3C3, 26);
        LoadControlSurfaces(ControlSurfaceR3C4, 27);
        LoadControlSurfaces(ControlSurfaceR3C5, 28);
        LoadControlSurfaces(ControlSurfaceR3C6, 29);
        LoadControlSurfaces(ControlSurfaceR3C7, 30);
        LoadControlSurfaces(ControlSurfaceR3C8, 31);
        LoadControlSurfaces(ControlSurfaceR4C1, 32);
        LoadControlSurfaces(ControlSurfaceR4C2, 33);
        LoadControlSurfaces(ControlSurfaceR4C3, 34);
        LoadControlSurfaces(ControlSurfaceR4C4, 35);
        LoadControlSurfaces(ControlSurfaceR4C5, 36);
        LoadControlSurfaces(ControlSurfaceR4C6, 37);
        LoadControlSurfaces(ControlSurfaceR4C7, 38);
        LoadControlSurfaces(ControlSurfaceR4C8, 39);
        LoadControlSurfaces(ControlSurfaceR5C1, 40);
        LoadControlSurfaces(ControlSurfaceR5C2, 41);
        LoadControlSurfaces(ControlSurfaceR5C3, 42);
        LoadControlSurfaces(ControlSurfaceR5C4, 43);
        LoadControlSurfaces(ControlSurfaceR5C5, 44);
        LoadControlSurfaces(ControlSurfaceR5C6, 45);
        LoadControlSurfaces(ControlSurfaceR5C7, 46);
        LoadControlSurfaces(ControlSurfaceR5C8, 47);

        NumOfControlSurfaces = 48;
        ComplexInteractionHandler();
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    #region Communication

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

        public Reflective ExtraData { get; set;  }
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

    public interface ISendSecurityInfo
    {
        string SecurityString { get; }
    }

    private void getSecurity(ScriptEventData gotSecurity)
    {
        //Log.Write("ComplexLooperController: In getSecurity");
        if (gotSecurity.Data == null)
        {
            return;
        }

        ISendSecurityInfo sendSecurityInfo = gotSecurity.Data.AsInterface<ISendSecurityInfo>();
        if (sendSecurityInfo == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        UsersToListenTo = sendSecurityInfo.SecurityString;
        //Log.Write("CustomControllerLooper UsersToListenTo After getSecurity: " + UsersToListenTo);
    }

    public class SendSampleToPlayInfo : Reflective
    {
        public string SampleName { get; set; }
        public SoundResource SampleSoundResource { get; set; }
        public string TrackPan { get; set; }
        public string TrackToUse { get; set; }
        public AgentInfo Jammer { get; set; }
    }



    #endregion

    #region Interaction

    private void LoadControlSurfaces(string ControlSurfaceInputString, int cntr)
    {
        //Log.Write("In Load Control Surfaces: " + cntr);
        //Takes Relative Values read in from configuration and converts them to realworld position 
        string[] values = new string[100];
        //Log.Write("sendSamples.SendSampleLibrary.Count(): " + sendSamples.SendSampleLibrary.Count());
        //Log.Write("sendNotePositions.SendNotePosition.Count(): " + sendNotePositions.SendNotePosition.Count());

        //Log.Write("ZRotation: " + ZRotation);
        //Log.Write("cntr: " + cntr);
        values = ControlSurfaceInputString.Split(',');
        ControlSurfaceMessage[cntr] = values[0];
        ControlSurfaceAXRelative[cntr] = float.Parse(values[1]);
        ControlSurfaceAYRelative[cntr] = float.Parse(values[2]);
        ControlSurfaceBXRelative[cntr] = float.Parse(values[3]);
        ControlSurfaceBYRelative[cntr] = float.Parse(values[4]);
        ControlSurfaceCXRelative[cntr] = float.Parse(values[5]);
        ControlSurfaceCYRelative[cntr] = float.Parse(values[6]);
        ControlSurfaceDXRelative[cntr] = float.Parse(values[7]);
        ControlSurfaceDYRelative[cntr] = float.Parse(values[8]);
        ControlSurfaceZMinimum[cntr] = float.Parse(values[9]);
        //Log.Write("ControlSurfaceZMinimum[" + cntr + "]: " + ControlSurfaceZMinimum[cntr]);
        ControlSurfaceZMaximum[cntr] = float.Parse(values[10]);
        //Log.Write("ControlSurfaceZMaximum[" + cntr + "]: " + ControlSurfaceZMaximum[cntr]);

        float CosAngle = (float)Math.Cos(ZRotation * 0.0174533);
        float SinAngle = (float)Math.Sin(ZRotation * 0.0174533);

        ControlSurfaceAXRelativeAfterRotation[cntr] = (ControlSurfaceAXRelative[cntr] * CosAngle) - (ControlSurfaceAYRelative[cntr] * SinAngle);
        ControlSurfaceAYRelativeAfterRotation[cntr] = (ControlSurfaceAYRelative[cntr] * CosAngle) + (ControlSurfaceAXRelative[cntr] * SinAngle);
        ControlSurfaceBXRelativeAfterRotation[cntr] = (ControlSurfaceBXRelative[cntr] * CosAngle) - (ControlSurfaceBYRelative[cntr] * SinAngle);
        ControlSurfaceBYRelativeAfterRotation[cntr] = (ControlSurfaceBYRelative[cntr] * CosAngle) + (ControlSurfaceBXRelative[cntr] * SinAngle);
        ControlSurfaceCXRelativeAfterRotation[cntr] = (ControlSurfaceCXRelative[cntr] * CosAngle) - (ControlSurfaceCYRelative[cntr] * SinAngle);
        ControlSurfaceCYRelativeAfterRotation[cntr] = (ControlSurfaceCYRelative[cntr] * CosAngle) + (ControlSurfaceCXRelative[cntr] * SinAngle);
        ControlSurfaceDXRelativeAfterRotation[cntr] = (ControlSurfaceDXRelative[cntr] * CosAngle) - (ControlSurfaceDYRelative[cntr] * SinAngle);
        ControlSurfaceDYRelativeAfterRotation[cntr] = (ControlSurfaceDYRelative[cntr] * CosAngle) + (ControlSurfaceDXRelative[cntr] * SinAngle);

    }

    float Sign(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y)
    {
        return (p1x - p3x) * (p2y - p3y) - (p2x - p3x) * (p1y - p3y);
    }

    bool IsPointInTri(float ptX, float ptY, float v1X, float v1Y, float v2X, float v2Y, float v3X, float v3Y)
    {
        bool b1, b2, b3;

        b1 = Sign(ptX, ptY, v1X, v1Y, v2X, v2Y) < 0.0f;
        //float b1float = Sign(ptX, ptY, v1X, v1Y, v2X, v2Y);
        //Log.Write("b1float: " + b1float);
        //Log.Write("b1: " + b1);
        b2 = Sign(ptX, ptY, v2X, v2Y, v3X, v3Y) < 0.0f;
        //float b2float = Sign(ptX, ptY, v2X, v2Y, v3X, v3Y);
        //Log.Write("b2float: " + b2float);
        //Log.Write("b2: " + b2);
        b3 = Sign(ptX, ptY, v3X, v3Y, v1X, v1Y) < 0.0f;
        //float b3float = Sign(ptX, ptY, v3X, v3Y, v1X, v1Y);
        //Log.Write("b3float: " + b3float);
        //Log.Write("b3: " + b3);

        return ((b1 == b2) && (b2 == b3));
    }

    bool PointInRectangle(float ptX, float ptY, float AX, float AY, float BX, float BY, float CX, float CY, float DX, float DY)
    {
        //bool test1 = IsPointInTri(ptX, ptY, AX, AY, BX, BY, CX, CY);
        //bool test2 = IsPointInTri(ptX, ptY, AX, AY, CX, CY, DX, DY);
        //Log.Write("Test1: " + test1);
        //Log.Write("Test2: " + test2);
        if (IsPointInTri(ptX, ptY, AX, AY, BX, BY, CX, CY)) return true;   //(X, Y, Z, P)) return true;
        if (IsPointInTri(ptX, ptY, AX, AY, CX, CY, DX, DY)) return true;
        //if (PointInTriangle(X, Z, W, P)) return true;
        return false;
    }

    private void ComplexInteractionHandler()
    {
        //Log.Write("In ComplexInteractionHandler");
        ComplexInteraction.Subscribe((InteractionData idata) =>
        {
            if (Debug)
            {
                ComplexInteraction.SetPrompt("Debug: "
                    + "\nHit:" + idata.HitPosition.ToString()
                    + "\nBy:" + ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name);
                //Vector hitPosition = idata.HitPosition;
                //Log.Write("Hit:  " + idata.HitPosition.ToString());
            }

            //Log.Write("Interacting person: " + ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name);
            ValidUsers.Clear();
            ValidUsers = UsersToListenTo.Split(',').ToList();
            if (UsersToListenTo.Contains("ALL"))
            {
                //Log.Write("Valid User: ALL");
                validUser = true;
            }
            else
            {
                foreach (string ValidUser in ValidUsers)
                {
                    //Log.Write("ValidUser: " + ValidUser);
                    if (ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name == ValidUser.Trim())
                    {
                        validUser = true;
                    }
                }
            }
            if (!validUser)
            {
                //ComplexInteraction.SetPrompt("You Are Not Authorized to Use The Looper");
                //Vector hitPosition = idata.HitPosition;
                //Log.Write("Hit:  " + idata.HitPosition.ToString());
            }
            else
            {

                ExecuteInteraction(idata);
            }

        });
    }

    private void ExecuteInteraction(InteractionData idata)
    {
        //loopNote = false;
        float hitXRelative = 0;
        float hitYRelative = 0;
        float hitZRelative = 0;
        Vector hitPosition = idata.HitPosition;
        //Log.Write("CurPosX: " + CurPos.X);
        //Log.Write("CurPosY: " + CurPos.Y);
        //Log.Write("hitPosition.X: " + hitPosition.X);
        //Log.Write("hitPosition.Y: " + hitPosition.Y);
        //normalize to origin 0,0

        if (hitPosition.X > CurPos.X)
        {
            hitXRelative = (hitPosition.X - CurPos.X) * 100;
        }
        else
        {
            hitXRelative = (hitPosition.X - CurPos.X) * 100;
        }

        if (hitPosition.Y > CurPos.Y)
        {
            hitYRelative = (hitPosition.Y - CurPos.Y) * 100;
        }
        else
        {
            hitYRelative = (hitPosition.Y - CurPos.Y) * 100;
        }
        if (hitPosition.Z > CurPos.Z)
        {
            hitZRelative = (hitPosition.Z - CurPos.Z) * 100;
        }
        else
        {
            hitZRelative = (hitPosition.Z - CurPos.Z) * 100;
        }
        //Log.Write("hitXRelative: " + hitXRelative + " hitYRelative: " + hitYRelative + " hitZRelative: " + hitZRelative);
        int cntr = 0;
        do
        {
            //Log.Write("AX: " + ControlSurfaceAXRelativeAfterRotation[cntr] + " AY: " + ControlSurfaceAYRelativeAfterRotation[cntr] + " BX: " + ControlSurfaceBXRelativeAfterRotation[cntr] + " BY: " + ControlSurfaceBYRelativeAfterRotation[cntr]);
            //Log.Write("CX: " + ControlSurfaceCXRelativeAfterRotation[cntr] + " CY: " + ControlSurfaceCYRelativeAfterRotation[cntr] + " DX: " + ControlSurfaceDXRelativeAfterRotation[cntr] + " DY: " + ControlSurfaceDYRelativeAfterRotation[cntr]);
            bool pointInRectangle = PointInRectangle(hitXRelative, hitYRelative,
                ControlSurfaceAXRelativeAfterRotation[cntr], ControlSurfaceAYRelativeAfterRotation[cntr],
                ControlSurfaceBXRelativeAfterRotation[cntr], ControlSurfaceBYRelativeAfterRotation[cntr],
                ControlSurfaceCXRelativeAfterRotation[cntr], ControlSurfaceCYRelativeAfterRotation[cntr],
                ControlSurfaceDXRelativeAfterRotation[cntr], ControlSurfaceDYRelativeAfterRotation[cntr]);
            if (pointInRectangle)
            {
                //Log.Write("Point in Rectangle");
                if (hitZRelative >= ControlSurfaceZMinimum[cntr] && hitZRelative <= ControlSurfaceZMaximum[cntr])
                {
                    //Simple Message
                    string hitControlSurface = ControlSurfaceMessage[cntr];
                    Log.Write("Control Hit - Cntr: " + cntr + " Surface: " + hitControlSurface);
                    //Add logic to get Track Information
                    Log.Write("Sending: " + ControlSurfaceMessage[cntr]);
                    sendSimpleMessage(cntr, hitControlSurface, idata);

                    break;
                }
            }
            cntr++;
        } while (cntr < NumOfControlSurfaces);
    }

    private void sendSimpleMessage(int cntr, string msg, InteractionData data)
    {
        SimpleData sd = new SimpleData(this);
        //SimpleDataExt thisObjectDataExt = new SimpleDataExt(this);
        sd.AgentInfo = ScenePrivate.FindAgent(data.AgentId)?.AgentInfo;
        sd.ObjectId = sd.AgentInfo.ObjectId;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
        Hitman = ScenePrivate.FindAgent(data.AgentId);
        // assign our data to reflective data field
        SendToAll(msg, sd);
        Log.Write("Custom Instrument Controller sending: " + ChannelOut);
        //SendToAll(ChannelOut, sd);
        if (cntr > 7) //Not a Screen Navigation Top Row
        {
            CurrentInstrumentInfo sendCurrentInstrumentInfo = new CurrentInstrumentInfo();
            sendCurrentInstrumentInfo.HitmanOut = Hitman;
            sendCurrentInstrumentInfo.ChannelOut = ChannelOut;
            sendCurrentInstrumentInfo.CurrentInstrument = msg;

            //Log.Write("Sending Message: " + sampleNameIn + ", " + sampleSoundResourceIn.GetName() + ", " + trackToUseIn);
            PostScriptEvent(ScriptId.AllScripts, "CurrentInstrument", sendCurrentInstrumentInfo);
        }
    }

    #endregion
}
