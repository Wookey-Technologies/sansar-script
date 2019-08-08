//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class PrizeGiver : SceneObjectScript

{
    public string PrizeCmd;
    public List<string> Prizes = new List<string>();

    //private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);
    //ObjectId hitter;
    Guid ProductGuid;


    public override void Init()
    {        Log.Write("Prize  Giver Script Started");
        Script.UnhandledException += UnhandledException; 

        ScenePrivate.Chat.Subscribe(0, null, GetChatCommand);
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    private void GetChatCommand(ChatData Data)
    {
        Log.Write(Data.Message);
        if (Data.Message.Contains(PrizeCmd))
        {
            ParseCmd(Data.Message);
        }
    }

    private void ParseCmd(string PrizeString)
    {
        //Log.Write("In Give Prize");
        List<string> PrizeArray = new List<string>();
        PrizeArray.Clear();
        PrizeArray = PrizeString.Split(',').ToList();
    
        string WinnerRaw = PrizeArray[0];
        int WinnerLength = WinnerRaw.Length;
        int CmdLength = PrizeCmd.Length;

        string Winner = WinnerRaw.Substring(CmdLength + 2, WinnerLength - CmdLength - 2);
        //Log.Write("Winner: " + Winner);
        string PrizeNumber = PrizeArray[1];
        int PrizeIndex = Int32.Parse(PrizeNumber) - 1;
        //Get Agent

        foreach (AgentPrivate agent in ScenePrivate.GetAgents())
        {
            //Log.Write("AgentName: " + agent.AgentInfo.Name);
            if (agent.AgentInfo.Name == Winner)
            {
                string ProductId = Prizes[PrizeIndex]; 
                if (!Guid.TryParse(ProductId, out ProductGuid))
                {
                    bool foundId = false;
                    // Find the ID from the store listing url. Very generic, will just find the first url segment or query arg it can convert to a UUID.
                    // https://store.sansar.com/listings/9eb72eb2-38c1-4cd3-a9eb-360e2f19e403/female-pirate-hat-r3d
                    string[] segments = ProductId.Split(new string[] { "/", "?", "&", "=" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string segment in segments)
                    {
                        if (segment.Length >= 32
                            && Guid.TryParse(segment, out ProductGuid))
                        {
                            foundId = true;
                            break;
                        }
                    }

                    if (!foundId)
                    {
                        Log.Write("Not Found in Store");
                    }
                }
                //Log.Write("ProductID: " + ProductId);
                //Log.Write("Product GUID: " + ProductGuid);
                //Log.Write("agent: " + agent);
                agent.Client.OpenStoreListing(ProductGuid);
            }
        }
    }

}