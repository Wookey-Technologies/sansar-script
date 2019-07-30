//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using Sansar;
using Sansar.Simulation;
using Sansar.Script;
using System;
using System.Linq;
using System.Collections.Generic;

class OnOffSign : SceneObjectScript
{
    private RigidBodyComponent RigidBody = null;
    public string OnOrOffBlock;
    private Vector DisplayPos = new Vector(-1.224f, 1.65f, 4.0f);
    private Vector HideOffPos = new Vector(-1.224f, 1.218f, -6.0f);
    private Vector HideOnPos = new Vector(-1.224f, 1.852f, -6.0f);
    private float SignMass;

    public interface SendStartedRaver
    {
        List<string> SetRaverStart { get; }
    }

    private void getStartedRaver(ScriptEventData gotRaverStarted)
    {
        if (gotRaverStarted.Data == null)
        {
            return;
        }
        SendStartedRaver sendStartedRaver = gotRaverStarted.Data.AsInterface<SendStartedRaver>();
        if (sendStartedRaver == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }
        //Log.Write("sendStartedRaver.SetRaverStart[0] : " + sendStartedRaver.SetRaverStart[0]);
        //Log.Write("OnOrOffBlock: " + OnOrOffBlock);
        if (sendStartedRaver.SetRaverStart[0] == "on")
        {
            if (OnOrOffBlock == "off")
            {
                //move off block below stage
                RigidBody.SetMass(0.0f);
                RigidBody.SetPosition(HideOffPos);
                RigidBody.SetMass(SignMass);
            }
            else if (OnOrOffBlock == "on")
            {
                //move on block above start volume
                Wait(TimeSpan.FromSeconds(0.1));
                RigidBody.SetMass(0.0f);
                RigidBody.SetPosition(DisplayPos);
                RigidBody.SetMass(SignMass);
            }
        }
        else if (sendStartedRaver.SetRaverStart[0] == "off")
        {
            if (OnOrOffBlock == "on")
            {
                //move on block below stage
                Wait(TimeSpan.FromSeconds(0.1));
                RigidBody.SetMass(0.0f);
                RigidBody.SetPosition(HideOnPos);
                RigidBody.SetMass(SignMass);
            }
            else if (OnOrOffBlock == "off")
            {
                //move off block above start volume
                RigidBody.SetMass(0.0f);
                RigidBody.SetPosition(DisplayPos);
                RigidBody.SetMass(SignMass);
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
        }
        SignMass = RigidBody.GetMass();
        SubscribeToScriptEvent("RaverStarted", getStartedRaver);
    }
}