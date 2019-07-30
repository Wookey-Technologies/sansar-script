//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class DisplayWebPageDialog : SceneObjectScript

{
    public string WebPage;
    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    private string _showDialogCommad = "/show";
    private string _closeButton = "Close";
    private string _cancelButton = "Cancel";
    private string _moreButton = "Visit Web Page";

    public override void Init()
    {
        Log.Write("Script Started");
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
        AgentPrivate hit = ScenePrivate.FindAgent(Data.HitComponentId.ObjectId);
        if (Data.Phase == CollisionEventPhase.TriggerEnter)
        {
            SceneInfo info = ScenePrivate.SceneInfo;
            //ScenePrivate.Chat.MessageAllUsers(WebPage);

            if (hit == null)
                return;

            StartCoroutine(ShowDialog, hit);
    }
    }

    private void ShowDialog(AgentPrivate agent)
    {
        var modalDialog = agent.Client.UI.ModalDialog;
        var agentInfo = agent.AgentInfo;

        //string simpleMessage = $"AgentInfo\nName: {agentInfo.Name}";
        //string simpleMessage = "Click for URL to: " + 
        //WaitFor(modalDialog.Show, simpleMessage, _closeButton, _moreButton);
        //if (modalDialog.Response == _moreButton)
        //{
            //string detailedMessage = $"AgentInfo\nName: {agentInfo.Name}\nObjectId: {agentInfo.ObjectId}\nSessionId: {agentInfo.SessionId}";
            string detailedMessage = "<a href=%22https://www.mozilla.org/en-US/%22>the Mozilla homepage</a>";
        WaitFor(modalDialog.Show, detailedMessage, _cancelButton, _closeButton);
        if (modalDialog.Response == _closeButton)
        {
            System.Diagnostics.Process.Start(WebPage);
        }
        //}
    }

}