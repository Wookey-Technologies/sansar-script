//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// This script is attached to a 3d model.  It has configuration paramters to identify control surfaces on the 3d model.  
// A control surface is a portion of the model that when left mouse clicked on in desktop mode of touched with your hand and 
// trigger in VR mode by the user it will send a simple message and a Reflex Bang that sends the message associated with the 
// control surface.  A good example of this is that the 3d model is a drum set.  Each drum and cymbal you define a Control Surface
// for.  Control Surfaces are Circles.  The Control Surfaces are configured using the followinng structure"
// EventName, XcenterofControlSurface, YcenterofConntrolSurface, RadiusOfControlSurface, Zminimum, Zmaximum
// SnareDrumHit, -12, 35, 25, 0, 200
// The units above are in centimeters and they are all relative from the center of the model.  So, this means that the control surface
// defined says that if the user hits on the model in an area that is within a circle located with an origin of X=-12cm, Y=35cm with a
// radius of 25cm and anywhere from 0 to 200 cm on the Z axis a Simple Message Event named SnareDrumHit will be sent.
//
// This means that any Simple Script Effector could be listening for the Event SnareDrumHit and then execute things like turning on
// a light, moving an object, generating a sound, etc.


public class TriggerComplexInteractionRectangle : SceneObjectScript

{
    #region ConstantsVariables
    [DefaultValue("Click Me!")]
    public readonly Interaction ComplexInteraction;
    //public Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    //public double ZRotationIn = 0.0;
    public float OffTimer = 0;
    public bool Debug = false;
    public string ControlSurface1 = null;
    public string ControlSurface2 = null;
    public string ControlSurface3 = null;
    public string ControlSurface4 = null;
    public string ControlSurface5 = null;
    public string ControlSurface6 = null;
    public string ControlSurface7 = null;
    public string ControlSurface8 = null;
    public string ControlSurface9 = null;
    public string ControlSurface10 = null;
    public string ControlSurface11 = null;
    public string ControlSurface12 = null;
    public string ControlSurface13 = null;
    public string ControlSurface14 = null;
    public string ControlSurface15 = null;
    public string ControlSurface16 = null;

    private float[] ControlSurfaceAXRelative = new float[20];
    private float[] ControlSurfaceAYRelative = new float[20];
    private float[] ControlSurfaceBXRelative = new float[20];
    private float[] ControlSurfaceBYRelative = new float[20];
    private float[] ControlSurfaceCXRelative = new float[20];
    private float[] ControlSurfaceCYRelative = new float[20];
    private float[] ControlSurfaceDXRelative = new float[20];
    private float[] ControlSurfaceDYRelative = new float[20];
    private float[] ControlSurfaceZMinimum = new float[20];
    private float[] ControlSurfaceZMaximum = new float[20];
    private float[] ControlSurfaceAXRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceAYRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceBXRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceBYRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceCXRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceCYRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceDXRelativeAfterRotation = new float[20];
    private float[] ControlSurfaceDYRelativeAfterRotation = new float[20];

    private string[] ControlSurfaceMessage = new string[20];
    private AgentPrivate Hitman;
    private RigidBodyComponent RigidBody = null;
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private static readonly Vector WarehouseRot = new Vector(0.0f, 0.0f, 0.0f);
    Quaternion RotQuat = Quaternion.FromEulerAngles(WarehouseRot).Normalized();
    private double ZRotation = new double();
    private int NumOfControlSurfaces = 0;
    private string Change = null;

    #endregion

    public override void Init()
    {
        Log.Write("In TriggerComplexInteractionRectangle");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        Change = "Stuff";
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
        }
        SubscribeToAll("noVR", InitializeComplexInteraction);
    }

    private void InitializeComplexInteraction(ScriptEventData data)
    {
        CurPos = RigidBody.GetPosition();
        //Log.Write("CurPos: " + CurPos);
        ZRotation = RigidBody.GetOrientation().GetEulerAngles().Z * 57.2958;
        Log.Write("Zrotation: " + ZRotation);

        int cntr = 0;

        if (ControlSurface1.Length > 0)
        {
            LoadControlSurfaces(ControlSurface1, cntr);
            cntr++;
        }
        if (ControlSurface2.Length > 0)
        {
            LoadControlSurfaces(ControlSurface2, cntr);
            cntr++;
        }
        if (ControlSurface3.Length > 0)
        {
            LoadControlSurfaces(ControlSurface3, cntr);
            cntr++;
        }
        if (ControlSurface4.Length > 0)
        {
            LoadControlSurfaces(ControlSurface4, cntr);
            cntr++;
        }
        if (ControlSurface5.Length > 0)
        {
            LoadControlSurfaces(ControlSurface5, cntr);
            cntr++;
        }
        if (ControlSurface6.Length > 0)
        {
            LoadControlSurfaces(ControlSurface6, cntr);
            cntr++;
        }
        if (ControlSurface7.Length > 0)
        {
            LoadControlSurfaces(ControlSurface7, cntr);
            cntr++;
        }
        if (ControlSurface8.Length > 0)
        {
            LoadControlSurfaces(ControlSurface8, cntr);
            cntr++;
        }
        if (ControlSurface9.Length > 0)
        {
            LoadControlSurfaces(ControlSurface9, cntr);
            cntr++;
        }
        if (ControlSurface10.Length > 0)
        {
            LoadControlSurfaces(ControlSurface10, cntr);
            cntr++;
        }
        if (ControlSurface11.Length > 0)
        {
            LoadControlSurfaces(ControlSurface11, cntr);
            cntr++;
        }
        if (ControlSurface12.Length > 0)
        {
            LoadControlSurfaces(ControlSurface12, cntr);
            cntr++;
        }
        if (ControlSurface13.Length > 0)
        {
            LoadControlSurfaces(ControlSurface13, cntr);
            cntr++;
        }
        if (ControlSurface14.Length > 0)
        {
            LoadControlSurfaces(ControlSurface14, cntr);
            cntr++;
        }
        if (ControlSurface15.Length > 0)
        {
            LoadControlSurfaces(ControlSurface15, cntr);
            cntr++;
        }
        if (ControlSurface16.Length > 0)
        {
            LoadControlSurfaces(ControlSurface16, cntr);
            cntr++;
        }
        NumOfControlSurfaces = cntr;
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


    #endregion

    #region Interaction

    private void LoadControlSurfaces(string ControlSurfaceInputString, int cntr)
    {
        //Log.Write("In Load Control Surfaces");
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
        Log.Write("In ComplexInteractionHandler");
        ComplexInteraction.Subscribe((InteractionData idata) =>
        {
            if (Debug)
            {
                ComplexInteraction.SetPrompt("Debug: " 
                    + "\nHit:" + idata.HitPosition.ToString());
                //Vector hitPosition = idata.HitPosition;
                Log.Write("Hit:  " + idata.HitPosition.ToString());
            }
            ExecuteInteraction(idata);
            //Log.Write("idata.HitPosition.ToString()" + idata.HitPosition.ToString());
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
        Log.Write("hitXRelative: " + hitXRelative + " hitYRelative: " + hitYRelative + " hitZRelative: " + hitZRelative);
        int cntr = 0;
        do
        {
            Log.Write("AX: " + ControlSurfaceAXRelativeAfterRotation[cntr] + " AY: " + ControlSurfaceAYRelativeAfterRotation[cntr] + " BX: " + ControlSurfaceBXRelativeAfterRotation[cntr] + " BY: " + ControlSurfaceBYRelativeAfterRotation[cntr]);
            Log.Write("CX: " + ControlSurfaceCXRelativeAfterRotation[cntr] + " CY: " + ControlSurfaceCYRelativeAfterRotation[cntr] + " DX: " + ControlSurfaceDXRelativeAfterRotation[cntr] + " DY: " + ControlSurfaceDYRelativeAfterRotation[cntr]);
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
                    Log.Write("Hit Control Surface: " + hitControlSurface);
                    sendSimpleMessage(ControlSurfaceMessage[cntr], idata);
                    break;
                }
            }
            cntr++;
        } while (cntr<NumOfControlSurfaces);
    }

    private void sendSimpleMessage(string msg, InteractionData data)
    {
        SimpleData sd = new SimpleData(this);
        sd.AgentInfo = ScenePrivate.FindAgent(data.AgentId)?.AgentInfo;
        sd.ObjectId = sd.AgentInfo.ObjectId;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
        SendToAll(msg, sd);

        if (OffTimer > 0.0)
        {
            Wait(TimeSpan.FromMilliseconds((int)OffTimer * 1000));
            SendToAll(msg + "Off", sd);
        }
    }

    #endregion
}
