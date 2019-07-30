//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using Sansar;
using Sansar.Simulation;
using Sansar.Script;
using System;
using System.Linq;
using System.Collections.Generic;

public class gotMojoMixer : SceneObjectScript
{
    // Components can be set in the editor if the correct component types are added to the object
    private RigidBodyComponent RigidBody = null;
    private string BeatBlockName;
    private SoundResource Sample1 = null;
    private string beats;
    private float XWarehousePos;
    private float YWarehousePos;
    private float ZWarehousePos;
    private string BlockGenre;
    private string pos;
    private string newpos = "99";
    private string ypos;
    private bool goodhit;
    private bool hitDetected = false;
    private static readonly Vector WarehouseRot = new Vector(0.0f, 0.0f, 0.0f);
    Quaternion RotQuat = Quaternion.FromEulerAngles(WarehouseRot).Normalized();
    private Vector OffsetPos = new Vector(0.0f, 0.0f, -1000.0f);
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private float BeatBlockMass;
    // private string oldFloor;
    private string newFloor;
    public IEventSubscription BeatBlockConfigSub;
    public IEventSubscription BeatBlockSampleConfigSub;

    #region Communications
    public interface SendBlockNamescfg
    {
        List<string> SendBlockArraycfg { get; }
    }

    private void getBeatBlockcfg(ScriptEventData gotBlockcfg)
    {
        SendBlockNamescfg sendBlockcfg = gotBlockcfg.Data.AsInterface<SendBlockNamescfg>();
        BeatBlockName = sendBlockcfg.SendBlockArraycfg[0];
        beats = sendBlockcfg.SendBlockArraycfg[1];
        BlockGenre = sendBlockcfg.SendBlockArraycfg[2];
        BeatBlockConfigSub.Unsubscribe();
    }

    public interface SendSamplescfg
    {
        List<SoundResource> SendSampleLibrarycfg { get; }
    }

    private void getSamplescfg(ScriptEventData gotSamplescfg)
    {
        if (gotSamplescfg.Data == null)
        {
            Log.Write(LogLevel.Warning, Script.ID.ToString(), "Expected non-null event data");
            return;
        }
        SendSamplescfg sendSamplescfg = gotSamplescfg.Data.AsInterface<SendSamplescfg>();
        if (sendSamplescfg == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        //Log.Write("Raver: Sample Count: " + sendSamplescfg.SendSampleLibrarycfg.Count());
        Sample1 = sendSamplescfg.SendSampleLibrarycfg.ElementAt(0);
        BeatBlockSampleConfigSub.Unsubscribe();
    }

    public class SendSamples : Reflective
    {
        public ScriptId SourceScriptId { get; internal set; }

        public List<SoundResource> SampleLibrary { get; internal set; }

        public List<SoundResource> SendSampleLibrary
        {
            get
            {
                return SampleLibrary;
            }
        }
    }

    public class SendBlockNames : Reflective
    {
        public ScriptId SourceScriptId { get; internal set; }

        public List<string> BlockNameArray { get; internal set; }

        public List<string> SendBlockArray
        {
            get
            {
                return BlockNameArray;
            }
        }
    }

    public List<string> ActiveBin = new List<string>();

    public interface SendActiveBins
    {
        List<string> SendActiveBin { get; }
    }

    private void getBin(ScriptEventData gotBin)
    {
        //Log.Write("A");
        if (gotBin.Data == null)
        {
            return;
        }
        //Log.Write("B");
        SendActiveBins sendBin = gotBin.Data.AsInterface<SendActiveBins>();
        //Log.Write("C");
        if (sendBin == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        string binToReturn;
        binToReturn = sendBin.SendActiveBin.ElementAt(0);
        //Log.Write("D");
        string VolumeFlag = sendBin.SendActiveBin.ElementAt(1);
        //Log.Write("binToReturn: " + binToReturn);
        //og.Write("VolumeFlag: " + VolumeFlag);
        // Log.Write("pos: " + pos);
        if (!(VolumeFlag == "volume"))
        {
            if ((binToReturn == pos) || (binToReturn == "all"))
            {
                ReturnBeatBlock();
            }
        }
    }

    #endregion

    public override void Init()
    {
        Script.UnhandledException += UnhandledException;
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
        }
        CurPos = RigidBody.GetPosition();
        XWarehousePos = CurPos[0];
        //Log.Write(BeatBlockName + " initial X: " + XWarehousePos);
        YWarehousePos = CurPos[1];
        //Log.Write(BeatBlockName + " initial Y: " + YWarehousePos);
        ZWarehousePos = CurPos[2];
        //Log.Write(BeatBlockName + " initial Z: " + ZWarehousePos);
        CollisionEventType trackedEvents = 0;
        trackedEvents |= CollisionEventType.RigidBodyContact;
        //SubscribeToScriptEvent("Genre", getGenre);

        string myObject;
        myObject = ObjectPrivate.ObjectId.ToString();
        string BeatBlockConfigEvent = "BeatBlockConfig" + myObject;
        string BeatBlockSampleConfigEvent = "BeatBlockSampleConfig" + myObject;
        BeatBlockConfigSub = SubscribeToScriptEvent(BeatBlockConfigEvent, getBeatBlockcfg);
        BeatBlockSampleConfigSub = SubscribeToScriptEvent(BeatBlockSampleConfigEvent, getSamplescfg);
        StartCoroutine(CheckForCollisions, trackedEvents);
    }

    private void UnhandledException(object sender, Exception e)
    {
        if (!Script.UnhandledExceptionRecoverable)
        {
            ScenePrivate.Chat.MessageAllUsers("Unrecoverable exception happened, the script will now be removed.");
        }
        else
        {
            ScenePrivate.Chat.MessageAllUsers("This script will be allowed to continue.");
        }
    }

    private void CheckForCollisions(CollisionEventType trackedEvents)
    {
        while (true)
        {
            // This will block the coroutine until a collision happens
            CollisionData data = (CollisionData)WaitFor(RigidBody.Subscribe, trackedEvents, Sansar.Script.ComponentId.Invalid);
            if (data.EventType == CollisionEventType.CharacterContact)
            {
                //Log.Write("I hit an avatar");
            }
            else
            {
                //Log.Write("I hit an object");
                //Log.Write("CollisionEventType: " + data.EventType);
                goodhit = false;
                if (!hitDetected)
                {
                    //Log.Write("BeatBlock: BeatBlockName: " + BeatBlockName);
                    //Log.Write("Position: " + RigidBody.GetPosition());
                    pos = GetXPosition(RigidBody.GetPosition().ToString4());
                    ypos = GetYPosition(RigidBody.GetPosition().ToString4());
                    if ((ypos == "-0") || (ypos == "0."))
                    {
                        goodhit = true;
                    }   
   
                    if (goodhit)
                    {
                        SendBlockNames sendBlocks = new SendBlockNames();
                        sendBlocks.BlockNameArray = new List<string>();
                        sendBlocks.BlockNameArray.Add(BeatBlockName);
                        //sendBlocks.BlockNameArray.Add(pos);
                        sendBlocks.BlockNameArray.Add(newpos);
                        sendBlocks.BlockNameArray.Add(beats);
                        PostScriptEvent(ScriptId.AllScripts, "BeatBlock", sendBlocks);
                        BuildSampleLibrary();
                        Wait(TimeSpan.FromSeconds(1.5));
                        DisplayBeatBlock();
                        Wait(TimeSpan.FromSeconds(0.5));
                        //ReturnBeatBlock();
                        //Wait(TimeSpan.FromSeconds(1));
                        LightSplash();
                        SubscribeToScriptEvent("ReturnBeatBlock", getBin);
                        //Log.Write("Here");
                        hitDetected = true;
                    }
                    else
                    {
                        //ScenePrivate.Chat.MessageAllUsers("Drop the Beat Block in a Loop Bin");
                        //send it back
                        ReturnBeatBlock();
                    }
                    
                }
             }
         }
    }

    private string GetXPosition(string strVectorIn)
    {
        int from = strVectorIn.IndexOf("<", StringComparison.CurrentCulture);
        string chunk = strVectorIn.Substring(from + 1, 2);
        //Log.Write("XPos: " + chunk);

        switch (chunk)
        {
            case "0.":
                newpos = "0";
                break;
            case "1.":
                newpos = "0";
                break;
            case "2.":
                newpos = "1";
                break;
            case "3.":
                newpos = "1";
                break;
            case "4.":
                newpos = "2";
                break;
            case "5.":
                newpos = "2";
                break;
            case "6.":
                newpos = "3";
                break;
            case "7.":
                newpos = "3";
                break;
            case "8.":
                newpos = "4";
                break;
            case "9.":
                newpos = "4";
                break;
            case "10":
                newpos = "5";
                break;
            case "11":
                newpos = "5";
                break;
            case "12":
                newpos = "6";
                break;
            case "13":
                newpos = "6";
                break;
            case "14":
                newpos = "7";
                break;
            case "15":
                newpos = "7";
                break;
            case "16":
                newpos = "8";
                break;
            case "17":
                newpos = "8";
                break;
            case "18":
                newpos = "9";
                break;
            case "19":
                newpos = "9";
                break;
        }
        //Log.Write("newpos: " + newpos);
        return newpos;
    }

    private string GetYPosition(string strVectorIn)
    {
        int from = strVectorIn.IndexOf(",", StringComparison.CurrentCulture);
        string chunk = strVectorIn.Substring(from + 1, 2);
        return chunk;
    }

    private void ReturnBeatBlock()  //Returns playing beat block from Display above Bin to 1st Floor Rack
    {

        //Take Block from Bin and Return it to Beat Box Staging Area
        BeatBlockMass = RigidBody.GetMass();
        CurPos = RigidBody.GetPosition();
        RigidBody.SetMass(0.0f);
   //     float floorZPosition = 0.0f;
   //     switch (newFloor)
   //     {
   //         case "1":
   //             floorZPosition = ZWarehousePos - 3.0f;
   //             break;
   //         case "2":
   //             floorZPosition = ZWarehousePos - 6.0f;
   //             break;
   //         case "3":
   //             floorZPosition = ZWarehousePos - 9.0f;
   //             break;
   //         case "4":
   //             floorZPosition = ZWarehousePos - 12.0f;
   //             break;
   //         case "5":
   //             floorZPosition = ZWarehousePos - 15.0f;
   //             break;
   //         case "6":
   //             floorZPosition = ZWarehousePos - 18.0f;
   //             break;
   //         case "7":
   //             floorZPosition = ZWarehousePos - 21.0f;
   //             break;
   //         case "8":
   //             floorZPosition = ZWarehousePos - 24.0f;
   //             break;
   //         case "9":
   //             floorZPosition = ZWarehousePos - 27.0f;
   //             break;
   //         default:
   //             ScenePrivate.Chat.MessageAllUsers("Not a valid floor in Sample Warehouse");
   //             break;
   //     }
        RigidBody.SetPosition(CurPos - OffsetPos);
        //Take Block from Bin and Return it to Beat Box Staging Area
        //CurPos = RigidBody.GetPosition();
        Vector WarehousePos = new Vector(XWarehousePos, YWarehousePos, ZWarehousePos);
        RigidBody.SetPosition(WarehousePos);
        RigidBody.SetOrientation(RotQuat);
        RigidBody.SetMass(BeatBlockMass);
        hitDetected = false;
    }

    private void DisplayBeatBlock() //Take Block from Bin and Place it Above The Bin to show what is currently playing
    {
        BeatBlockMass = RigidBody.GetMass();
        CurPos = RigidBody.GetPosition();
        RigidBody.SetMass(0.0f);
        float XDisplayPos = float.Parse(newpos) * 2 + 1.0f;
        float YDisplayPos = 0.7f;
        float ZDisplayPos = 2.2f;
        Vector DisplayPos = new Vector(XDisplayPos, YDisplayPos, ZDisplayPos);
        RigidBody.SetPosition(DisplayPos);
        RigidBody.SetOrientation(RotQuat);
        RigidBody.SetMass(BeatBlockMass);
    }

    private void LightSplash()
    {
        // Raise Intensity of Spot Light on Slot
        // Slowly Lower Light on Slot
    }

    private void BuildSampleLibrary()
    {
        SendSamples sendSamples = new SendSamples();
        sendSamples.SampleLibrary = new List<SoundResource>();
        if (Sample1 != null) sendSamples.SampleLibrary.Add(Sample1);
        //Wait(TimeSpan.FromSeconds(5));
        PostScriptEvent(ScriptId.AllScripts, "BeatBlockSample", sendSamples);
    }
}
