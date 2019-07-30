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

public class CollisionVolumeSender : SceneObjectScript

{
    public string WelcomeMessage = "Welcome to the Interactive Grand Piano";
    private int NumOfTime = 0;
    //private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);

    private SessionId Jammer = new SessionId();

    public override void Init()
    {
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        RigidBodyComponent rigidBody;
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody)
            && rigidBody.IsTriggerVolume())
        {
            //CurPos = rigidBody.GetPosition();

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
        //if (NumOfTime < 1)
        //{
            AgentPrivate hit = ScenePrivate.FindAgent(Data.HitComponentId.ObjectId);
            //SendCollisionInfo sendCollisionInfo = new SendCollisionInfo();
            //sendCollisionInfo.CollisionInfoArray = new List<object>();
            SendsCollisionData sendsCollisionData = new SendsCollisionData();;
            if (Data.Phase == CollisionEventPhase.TriggerEnter)
            {
                Log.Write("Entered Volume");
                SceneInfo info = ScenePrivate.SceneInfo;
                ScenePrivate.Chat.MessageAllUsers(WelcomeMessage);            
                //sendCollisionInfo.CollisionInfoArray.Add("Enter");
                sendsCollisionData.SentCollisionData = Data;
                PostScriptEvent(ScriptId.AllScripts, "CollisionData", sendsCollisionData);
            }
            else
            {
                //sendCollisionInfo.CollisionInfoArray.Clear();
                //sendCollisionInfo.CollisionInfoArray.Add("Exit");
                Log.Write("has left my volume!");
            }
           // NumOfTime++;
        //}  
    }

    public class SendsCollisionData : Reflective
    {
        public ScriptId SourceScriptId { get; internal set; }

        public CollisionData SentCollisionData { get; internal set; }
  
        public CollisionData SendCollisionData
        {
            get
            {
                return SentCollisionData;
            }
        }
    }

}