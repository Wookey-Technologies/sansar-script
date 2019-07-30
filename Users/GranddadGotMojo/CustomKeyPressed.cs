//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

#define SansarBuild
#define InstrumentBuild

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

//Version used using normal collisions with an object and not 

public class CustomKeyPressed : SceneObjectScript

{

    #region ConstantsVariables

    public string ChannelOut = "K1";
    private SessionId Jammer = new SessionId();
    private SessionId LastJammer = new SessionId();
    AgentPrivate TheUser = null;
    List<IEventSubscription> ButtonSubscriptions = new List<IEventSubscription>();
    private bool Shifted = false;
    private bool SelectUp = false;
    private AgentPrivate Hitman;
    private RigidBodyComponent rigidBody;
    private CollisionData collsionData;

    #endregion

    #region Communication

    public class SendKeyInfo : Reflective
    {
        public string iChannelOut { get; set; }
        public string iKeySent { get; set; }
    }

    void SubscribeClientToButton(Client client, string Button)
    {
        ButtonSubscriptions.Add(client.SubscribeToCommand(Button, CommandAction.Pressed, CommandReceived, CommandCanceled));
        ButtonSubscriptions.Add(client.SubscribeToCommand(Button, CommandAction.Released, CommandReceived, CommandCanceled));
    }

    void UnsubscribeAllButtons()
    {
        foreach (IEventSubscription sub in ButtonSubscriptions) sub.Unsubscribe();
        ButtonSubscriptions.Clear();
    }

    void SubscribeKeyPressed(AgentPrivate agent, string Message)
    {
        if (agent == null) return;

        if (Message == "sub" && TheUser == null)
        {
            TheUser = agent;
            Client client = agent.Client;
            SubscribeClientToButton(client, "Keypad0");
            SubscribeClientToButton(client, "Keypad1");
            SubscribeClientToButton(client, "Keypad2");
            SubscribeClientToButton(client, "Keypad3");
            SubscribeClientToButton(client, "Keypad4");
            SubscribeClientToButton(client, "Keypad5");
            SubscribeClientToButton(client, "Keypad6");
            SubscribeClientToButton(client, "Keypad7");
            SubscribeClientToButton(client, "Keypad8");
            SubscribeClientToButton(client, "Keypad9");
            SubscribeClientToButton(client, "Action1");
            SubscribeClientToButton(client, "Action2");
            SubscribeClientToButton(client, "Action3");
            SubscribeClientToButton(client, "Action4");
            SubscribeClientToButton(client, "Action5");
            SubscribeClientToButton(client, "Action6");
            SubscribeClientToButton(client, "Action7");
            SubscribeClientToButton(client, "Action8");
            SubscribeClientToButton(client, "Action9");
            SubscribeClientToButton(client, "Action0");
            SubscribeClientToButton(client, "Modifier");
        }

        if (Message == "unsub")
        {
            UnsubscribeAllButtons();
            TheUser = null;
            return;
        }
    }

    void CommandReceived(CommandData Button)
    {
        if ((Button.Command == "Modifier") && (Button.Action == CommandAction.Pressed))  //shift acts a toggle
        {
            if (Shifted == true) Shifted = false;
            else Shifted = true;
        }
        if ((Button.Command == "SelectUp") && (Button.Action == CommandAction.Pressed))  //shift acts a toggle
        {
            if (SelectUp == true) SelectUp = false;
            else SelectUp = true;
        }


        //Log.Write("Button.Command: " + Button.Command + " Button.Action: " + Button.Action);
        if ((Button.Command == "Action1") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XC2");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XCS2");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XD2");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XDS2");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XE2");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XF2");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XFS2");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XG2");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XGS2");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XA2");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XAS2");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XB2");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XC3");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XCS3");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XD3");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XDS3");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XE3");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XF3");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XFS3");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (!(SelectUp))) SendKey("XG3");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XGS3");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XA3");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XAS3");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XB3");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XC4");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XCS4");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XD4");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XDS4");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XE4");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XF4");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XFS4");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XG4");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XGS4");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XA4");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XAS4");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XB4");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XC5");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XCS5");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XD5");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Pressed) && (Shifted) && (!(SelectUp))) SendKey("XDS5");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XE5");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XF5");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XFS5");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XG5");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XGS5");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XA5");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XAS5");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XB5");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XC6");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XCS6");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XD6");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XDS6");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XE6");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XF6");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XFS6");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XG6");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XGS6");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XA6");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XAS6");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Pressed) && (!(Shifted)) && (SelectUp)) SendKey("XB6");

        //Log.Write("Button.Command: " + Button.Command + " Button.Action: " + Button.Action);
        if ((Button.Command == "Action1") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XC2Up");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XCS2Up");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XD2Up");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XDS2Up");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XE2Up");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XF2Up");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XFS2Up");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XG2Up");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XGS2Up");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XA2Up");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XAS2Up");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XB2Up");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XC3Up");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XCS3Up");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XD3Up");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XDS3Up");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XE3Up");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XF3Up");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XFS3Up");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (!(SelectUp))) SendKey("XG3Up");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XGS3Up");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XAUp3");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XAS3");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XB3Up");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XC4Up");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XCS4Up");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XD4Up");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XDS4");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XE4Up");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XF4Up");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XFS4Up");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XG4Up");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XGS4Up");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XA4Up");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XAS4Up");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XB4Up");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XC5Up");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XCS5Up");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XD5Up");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Released) && (Shifted) && (!(SelectUp))) SendKey("XDS5Up");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XE5Up");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XF5Up");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XFS5Up");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XG5Up");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XGS5Up");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XA5Up");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XAS5Up");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XB5Up");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XC6Up");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XCS6Up");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XD6Up");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XDS6Up");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XE6Up");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XF6Up");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XFS6Up");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XG6Up");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XGS6Up");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XA6Up");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XAS6Up");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Released) && (!(Shifted)) && (SelectUp)) SendKey("XB6Up");

    }

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: " + data.Message);
    }

    #endregion

    public override void Init()
    {
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        //Log.Write("In Custom PC Key Pressed");
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody))
        {
            Log.Write("Calling OnCollide");
            rigidBody.Subscribe(CollisionEventType.AllCollisions, OnCollide);
        }
        else
        {
        }
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    private void OnCollide(CollisionData Data)
    {

        //Log.Write("In OnCollide");
        collsionData = Data;
        Hitman = ScenePrivate.FindAgent(Data.HitComponentId.ObjectId);
        //SceneInfo info = ScenePrivate.SceneInfo;
        Jammer = Hitman.AgentInfo.SessionId;
        if (Jammer != LastJammer)
        {
            SubscribeKeyPressed(Hitman, "unsub");
            Wait(TimeSpan.FromSeconds(2));  //wait a couple of seconds for the keys to unsubscribe
            SubscribeKeyPressed(Hitman, "sub");
            //Log.Write("Keys subscribed for Player: " + Hitman.AgentInfo.Name);
            LastJammer = Jammer;
        }
    }

    void SendKey(string KeyIn)
    {
        //sendSimpleMessage(KeyIn, collsionData);
        //Log.Write("CustomKeyPressed: " + KeyIn);
        SendKeyInteraction(KeyIn);
    }

    private void SendKeyInteraction(string Key)
    {
        SendKeyInfo sendKeyInfo = new SendKeyInfo();
        sendKeyInfo.iChannelOut = ChannelOut;
        sendKeyInfo.iKeySent = Key;
        PostScriptEvent(ScriptId.AllScripts, "KeySent", sendKeyInfo);
    }
}
