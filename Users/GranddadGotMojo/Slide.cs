//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using Sansar;
using Sansar.Simulation;
using Sansar.Script;
using System;
using System.Linq;
using System.Collections.Generic;

class Slide : SceneObjectScript
{
    private RigidBodyComponent RigidBody = null;
    public string SlideNumber = "1";
    public string ScreenToDisplayOn = "1";
    private float XWarehousePos;
    private float YWarehousePos;
    private float ZWarehousePos;
    private float XWarehouseRot;
    private float YWarehouseRot;
    private float ZWarehouseRot;
    private float XScreenPos = 666.666f;
    private float YScreenPos = 666.666f;
    private float ZScreenPos = 666.666f;
    private float XScreenRot = 666.666f;
    private float YScreenRot = 666.666f;
    private float ZScreenRot = 666.666f;
    private float pi = 3.14159265359f;
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private Vector ScreenPos = new Vector(0.0f, 0.0f, 0.0f);
    private float SlideMass;
    private Vector CurRot = new Vector(0.0f, 0.0f, 0.0f);
    Quaternion ScreenQuat;
    Quaternion CurQuat;

    public interface SendScreenPos
    {
        List<string> SetScreenPos { get; }
    }

    private void getScreenPos(ScriptEventData gotScreenPos)
    {
        Log.Write("SCREEN EVENT RECIEVED");
        Log.Write("In getScreenPos");
        if (gotScreenPos.Data == null)
        {
            return;
        }
        SendScreenPos sendScreenPos = gotScreenPos.Data.AsInterface<SendScreenPos>();
        if (sendScreenPos == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        XScreenPos = float.Parse(sendScreenPos.SetScreenPos[0]);
        Log.Write("XScreenPos: " + XScreenPos);
        YScreenPos = float.Parse(sendScreenPos.SetScreenPos[1]);
        ZScreenPos = float.Parse(sendScreenPos.SetScreenPos[2]);
        XScreenRot = float.Parse(sendScreenPos.SetScreenPos[3]);
        YScreenRot = float.Parse(sendScreenPos.SetScreenPos[4]);
        ZScreenRot = float.Parse(sendScreenPos.SetScreenPos[5]);
        ScreenPos = new Vector(XScreenPos, YScreenPos, ZScreenPos);
        Vector ScreenRot = new Vector(XScreenRot * pi / 180, YScreenRot * pi / 180, ZScreenRot * pi / 180);
        ScreenQuat = Quaternion.FromEulerAngles(ScreenRot).Normalized();
        CurQuat = RigidBody.GetOrientation();
        CurPos = RigidBody.GetPosition();
        SlideMass = RigidBody.GetMass();
    }

    private void GetChatCommand(ChatData Data)
    {
        string DataCmd = Data.Message;
        //Log.Write("DataCmd: " + DataCmd);
        if (SubscribeToScriptEvent("SendScreenPos", getScreenPos).Active)
        {
            Log.Write("Subscription Was Active from getChatCommmand");
        }
        else
        {
            Log.Write("Subscription Was NOT Active from getChatCommmand");
        }
        if ((DataCmd.Substring(0, 1) == "/") && (DataCmd.Length > 1))
        {
            string testSlide = DataCmd.Substring(0, 2);  //it was a slide command
            Log.Write("testSlide: " + testSlide);
            if ((testSlide == "/0") || (testSlide == "/1") || (testSlide == "/2") || (testSlide == "/3") || (testSlide == "/4") || (testSlide == "/5") || (testSlide == "/6") || (testSlide == "/7") || (testSlide == "/8") || (testSlide == "/9"))
            {
                int from = DataCmd.IndexOf("/", StringComparison.CurrentCulture);
                Log.Write("from: " + from);
                //Log.Write("length: " + DataCmd.Length);
                string test = DataCmd.Substring(from + 1, DataCmd.Length - 1);
                Log.Write("test: " + test);
                if (test == SlideNumber)
                {
                    //Take Block from Bin and Return it to Beat Box Staging Area
                    //RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeDynamic);
                    Wait(TimeSpan.FromMilliseconds(200));
                    //Log.Write("Slide: " + SlideNumber + "  Moving to Screen");
                    //SlideMass = RigidBody.GetMass();
                    CurPos = RigidBody.GetPosition();
                    //Log.Write("Slide: " + SlideNumber + "  CurPos: " + CurPos);
                    //RigidBody.SetMass(0.0f);
                    //Log.Write("Slide: " + SlideNumber + "  ScreenPos: " + ScreenPos);
                    RigidBody.SetPosition(ScreenPos);

                    //Take Block from Bin and Return it to Beat Box Staging Area
                    RigidBody.SetOrientation(ScreenQuat);
                    //Log.Write("Slide: " + SlideNumber + "  ScreenRot: " + ScreenQuat);

                    Wait(TimeSpan.FromMilliseconds(200));
                    //RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeStatic);
                    //RigidBody.SetMass(SlideMass);
                }
                else if ((testSlide == "/0") || (testSlide == "/1") || (testSlide == "/2") || (testSlide == "/3") || (testSlide == "/4") || (testSlide == "/5") || (testSlide == "/6") || (testSlide == "/7") || (testSlide == "/8") || (testSlide == "/9"))
                {
                    Log.Write("Slide: " + SlideNumber + "  Moving to Storage");
                    //RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeDynamic);
                    Wait(TimeSpan.FromMilliseconds(200));
                    //CurPos = RigidBody.GetPosition();
                    RigidBody.SetMass(0.0f);
                    //Take Block from Bin and Return it to Beat Box Staging Area
                    //CurPos = RigidBody.GetPosition();
                    RigidBody.SetPosition(CurPos);
                    //CurQuat = Quaternion.FromEulerAngles(CurRot).Normalized();
                    RigidBody.SetOrientation(CurQuat);

                    //Wait(TimeSpan.FromMilliseconds(200));
                    //RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeStatic);
                    RigidBody.SetMass(SlideMass);
                }
            }
        }
    }

    public override void Init()
    {
        
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
            //ScenePrivate.Chat.Subscribe(0, "user", hit.AgentInfo.SessionId, GetChatCommand);
            ScenePrivate.Chat.Subscribe(0, GetChatCommand);
        }
        CurPos = RigidBody.GetPosition();
        XWarehousePos = CurPos[0];
        YWarehousePos = CurPos[1];
        ZWarehousePos = CurPos[2];
        CurQuat = RigidBody.GetOrientation();
        //XWarehouseRot = CurQuat[1];
        //YWarehouseRot = CurQuat[2];
        //ZWarehouseRot = CurQuat[3];
        //CurRot = new Vector(XWarehouseRot * pi / 180, YWarehouseRot * pi / 180, ZWarehouseRot * pi / 180);
        SlideMass = RigidBody.GetMass();
        //RigidBody.SetMotionType(RigidBodyMotionType.MotionTypeStatic);
        Log.Write("EXECUTING INIT");
        if (SubscribeToScriptEvent("SendScreenPos", getScreenPos).Active)
        {
            Log.Write("Subscription Was Active");
        }
        else
        {
            Log.Write("Subscription Was Not Active");
            SubscribeToScriptEvent("SendScreenPos", getScreenPos);

        }
        
    }
}