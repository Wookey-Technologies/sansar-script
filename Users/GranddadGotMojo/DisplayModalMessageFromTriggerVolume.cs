//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class DisplayModalMessage : SceneObjectScript

{
    public string ModalMessage;
    public string HelpMessage;
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    ObjectId hitter;

    public override void Init()
    {
        Log.Write("Modal Script Started");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        RigidBodyComponent rigidBody;
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody)
            && rigidBody.IsTriggerVolume())
        {
            //Log.Write("Current Position: " + rigidBody.GetPosition());
            CurPos = rigidBody.GetPosition();
            rigidBody.Subscribe(CollisionEventType.Trigger, OnCollide);
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
        hitter = Data.HitComponentId.ObjectId;
        AgentPrivate hit = ScenePrivate.FindAgent(Data.HitComponentId.ObjectId);

        if (Data.Phase == CollisionEventPhase.TriggerEnter)
        {
            SceneInfo info = ScenePrivate.SceneInfo;
            ModalDialog Dlg;
            AgentPrivate agent = ScenePrivate.FindAgent(hitter);
            if (agent == null)
                return;

            Dlg = agent.Client.UI.ModalDialog;
            WaitFor(Dlg.Show, ModalMessage, "OK", "Help");
            if (Dlg.Response == "Help")
            {
                StartCoroutine(() =>
                {
                    ScenePrivate.Chat.MessageAllUsers(HelpMessage);
                    Wait(TimeSpan.FromSeconds(1));
                });
            }
        }
    }

}