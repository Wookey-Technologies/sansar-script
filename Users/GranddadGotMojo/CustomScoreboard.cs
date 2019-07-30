/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * �    Acknowledge that the content is from the Sansar Knowledge Base.
 * �    Include our copyright notice: "� 2017 Linden Research, Inc."
 * �    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * �    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. � 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CustomScoreboard : SceneObjectScript
{
    #region ConstantsVariables

    [DefaultValue("")]
    [DisplayName("Game: ")]
    public string GameForScoreboard = "";

    // ScoreTypes
    // Add: Add Multiple Records for Game and Name (multiple scores for the same person)
    // Replace:  One Record in the Database for Game and Name .... if new it does a post with the first score if already there it does a put to replace with a new score 
    [DefaultValue("")]
    [DisplayName("ScoreType: ")]
    public string ScoreTypeForScoreboard = "";

    public List<string> ContestantList = new List<string>();
    public List<int> InitialScoreList = new List<int>();

    [DefaultValue("ALL")]
    [DisplayName("Valid User List:")]
    public string UsersToListenTo = "ALL";

    [DefaultValue(false)]
    [DisplayName("Persist Scores: ")]
    public bool Persistence = false;

    [DefaultValue("")]
    [DisplayName("Web Server URL: ")]
    public string WebAppServer = "";

    private string Contestent1 = "";
    private string Contestent2 = "";
    private string Contestent3 = "";
    private string Contestent4 = "";
    private string Contestent5 = "";
    private string Contestent6 = "";
    private string Contestent7 = "";
    private string Contestent8 = "";

    private int Score1 = 0;
    private int Score2 = 0;
    private int Score3 = 0;
    private int Score4 = 0;
    private int Score5 = 0;
    private int Score6 = 0;
    private int Score7 = 0;
    private int Score8 = 0;

    //[DefaultValue("ALL")]
    //[DisplayName("Valid User List:")]
    //public string UsersToListenTo = "ALL";

    private List<string> ValidUsers = new List<string>();

    private string Errormsg = "No Errors";
    private bool strErrors = false;
    private SessionId Jammer = new SessionId();
    private Action SimpleScriptSubscription;
    AgentInfo AgentName;
    ObjectId ComponentID;

    public class SendChar : Reflective
    {
        public int CharIndex { get; set; }
        public string CharToSend { get; set; }
    }

    private Vector CurPos = new Vector(0.0f, 0.0f, 0.0f);

    public class GameScores
    {
        public int ID { get; set; }
        public string Game { get; set; }
        public string Name { get; set; }
        public string ScoreDate { get; set; }
        public int Score { get; set; }
    }

    public class SendGameScores : Reflective
    {
        public int ID { get; set; }
        public string Game { get; set; }
        public string Name { get; set; }
        public string ScoreDate { get; set; }
        public int Score { get; set; }
    }

    public class SendContestent : Reflective
    {
        public int ContestentNumber { get; set; }
        public string ContestentName { get; set; }
    }

    public interface ISendContestentScore
    {
        int ContestentNumber { get; }
        string ContestentName { get; }
        string ScoreType { get; }
        int Score { get; }
    }

    #endregion

    #region Communication

    private bool HTTPPostItem(GameScores scoreIn)
    {
        bool Success;

        HttpRequestOptions options = new HttpRequestOptions();
        options.Method = HttpRequestMethod.POST;
        options.Headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
        //options.Body = "{\"game\": \"TestGame\",\"name\": \"Guy3\",\"scoreDate\": \"2019 - 06 - 11T20: 49:05\",\"score\": \"1020\"}";
        options.Body = "{\"game\": \""
            + scoreIn.Game
            + "\",\"name\": \""
            + scoreIn.Name
            + "\",\"ScoreDate\": \""
            + scoreIn.ScoreDate
            + "\",\"Score\": \""
            + scoreIn.Score
            + "\"}";
        //Log.Write("Body: " + options.Body);
        //Log.Write("WebAppServer: " + WebAppServer);
        var result = WaitFor(ScenePrivate.HttpClient.Request, WebAppServer, options) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result: " + result.Success);
        if (result.Response.Status == 200)
        {
            if (result.Success)
            {
                Success = true;
            }
            else
            {
                Success = false;
            }
            return Success;
        }
        else
        {
            ScenePrivate.Chat.MessageAllUsers("Database Down Contact Granddad GotMojo");
            return false;
        }
    }

    private bool HTTPUpdateItem(GameScores gsIn)
    {
        bool UpdateStatus = false;

        //Test to See if Record Exists
        //Log.Write("Name: " + gsIn.Name + " Game: " + gsIn.Game);
        string QueryURL = WebAppServer + "/Condition=" + "Game='" + gsIn.Game + "'" + "AND" + "Name='" + gsIn.Name + "'";
        //Log.Write("QueryURL: " + QueryURL);
        var queryResult = WaitFor(ScenePrivate.HttpClient.Request, QueryURL) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));

        //Log.Write("result.Response.Status: " + queryResult.Response.Status);
        if (queryResult.Response.Status == 200)
        {
            //Log.Write("How did I get here?");
            string Record = queryResult.Response.Body;
            //Log.Write("Record: " + Record);
            if (Record == "NoneFound")
            {
                //Post New Item 
                UpdateStatus = HTTPPostItem(gsIn);
            }
            else
            {
                int IDtoGet = Int32.Parse(Record.Substring(0, Record.Length - 1));
                //Log.Write("IDtoGet: " + IDtoGet);
                GameScores gs = new GameScores();
                gs = HTTPGetItem(IDtoGet);
                //Log.Write("Put GS Returned: " + gs.ID + " Game: " + gs.Game + " Name: " + gs.Name + " Date: " + gs.ScoreDate.ToString() + " Score: " + gs.Score);
                DateTime CurrentDateTime = DateTime.Now;
                //Log.Write("CurrentDateTime: " + CurrentDateTime);

                // Update Item if it Exists
                HttpRequestOptions options = new HttpRequestOptions();
                options.Method = HttpRequestMethod.PUT;
                options.Headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
                int newScore = 0;
                //Log.Write("ScoreTypeForScoreboard: " + ScoreTypeForScoreboard + " gsIn.Score: " + gsIn.Score + " gs.Score: " + gs.Score);
                if (ScoreTypeForScoreboard == "Replace")
                {
                    newScore = gsIn.Score;
                }
                else if (ScoreTypeForScoreboard == "Increment")
                {
                    newScore = gs.Score + gsIn.Score;
                }
                else if (ScoreTypeForScoreboard == "Clear")
                {
                    newScore = 0;
                }
                options.Body = "{\"id\": "
                    + gs.ID
                    + ",\"game\": \""
                    + gs.Game
                    + "\",\"name\": \""
                    + gs.Name
                    + "\",\"scoreDate\": \""
                    //+ gs.ScoreDate
                    + CurrentDateTime
                    + "\",\"score\": "
                    + newScore
                    + "}";
                //Log.Write("Body: " + options.Body);

                string URLString = WebAppServer + "/" + gs.ID.ToString();
                var updateResult = WaitFor(ScenePrivate.HttpClient.Request, URLString, options) as HttpClient.RequestData;
                Wait(TimeSpan.FromSeconds(0.12));
                //Log.Write("result: " + updateResult.Success);

                if (updateResult.Success)
                {
                    UpdateStatus = true;
                }
                else
                {
                    UpdateStatus = false;
                }
            }
            return UpdateStatus;
        }
        else
        {
            UpdateStatus = false;
            ScenePrivate.Chat.MessageAllUsers("Database Down Contact Granddad GotMojo");
            Log.Write("Error Contacting Web Server");
            return UpdateStatus;
        }
    }

    private GameScores JSONtoGS(string inJSON)
    {
        //Log.Write("inJSON: " + inJSON);
        string name = null;
        string value = null;
        List<string> JSONList = new List<string>();
        //Log.Write("B");
        JSONList.Clear();
        //Log.Write("C");
        JSONList = inJSON.Split(',').ToList();
        //Log.Write("D");
        GameScores gsOut = new GameScores();
        int cntr = 0;
        do
        {
            //Log.Write("cntr: " + cntr + " JSON Item: " + JSONList[cntr]);
            //split

            if (cntr == 3)
            {
                name = JSONList[cntr].Substring(1, 9);
                value = JSONList[cntr].Substring(13, JSONList[cntr].Length - 14);
            }
            else
            {
                string[] tmp = JSONList[cntr].Split(':');
                name = tmp[0].TrimStart('{');
                name = name.Trim('"');
                value = tmp[1].TrimEnd('}');
                value = value.Trim('"');
            }

            //Log.Write("tmp[0]: " + tmp[0]);
            //Log.Write("tmp[1]:" + tmp[1]);
            //trim

            //Log.Write("cntr: " + cntr + " Name: " + name + " Value: " + value);
            if (cntr == 0) gsOut.ID = Int32.Parse(value);
            else if (cntr == 1) gsOut.Game = value;
            else if (cntr == 2) gsOut.Name = value;
            else if (cntr == 3) gsOut.ScoreDate = value;
            else if (cntr == 4) gsOut.Score = Int32.Parse(value);
            cntr++;
        } while (cntr < JSONList.Count());

        return gsOut;
    }

    private GameScores HTTPGetItem(int idIn)
    {
        //HttpRequestOptions options = new HttpRequestOptions();
        //options.Method = HttpRequestMethod.GET;
        //options.Headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } };

        string url = WebAppServer + "/" + idIn.ToString();
        //Log.Write("Get URL: " + url);
        var result = WaitFor(ScenePrivate.HttpClient.Request, url) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result.success: " + result.Success);
        //Log.Write("result.message: " + result.Message);
        //Log.Write("result.exception: " + result.Exception.Message);
        //Log.Write("result.Respose: " + result.Response.Body);
        //Log.Write("Get result.Response.Status: " + result.Response.Status);
        string Record = result.Response.Body;
        //Log.Write("Get Record: " + Record);
        GameScores gs = new GameScores();
        gs = JSONtoGS(Record);

        return gs;
    }

    private List<GameScores> HTTPGetTop8GameScores(string gameIn)
    {
        List<GameScores> scoreArray = new List<GameScores>();

        string QueryURL = WebAppServer + "/Condition=" + "'Game=" +  gameIn + "'";
        //Log.Write("QueryURL: " + QueryURL);
        var result = WaitFor(ScenePrivate.HttpClient.Request, QueryURL) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result.Response.Status: " + result.Response.Status);
        string Record = result.Response.Body;
        //Log.Write("Record: " + Record);

        List<string> SelectedIDList = new List<string>();
        SelectedIDList.Clear();
        SelectedIDList = Record.Split(',').ToList();
        int cntr = 0;
        do
        {
            //Log.Write("cntr: " + cntr + " ID: " + SelectedIDList[cntr]);

            GameScores gotItem = HTTPGetItem(Int32.Parse(SelectedIDList[cntr]));
            //Log.Write("ID: " + gotItem.ID + " Game: " + gotItem.Game + " Name: " + gotItem.Name + " Date: " + gotItem.ScoreDate + " Score: " + gotItem.Score);
            scoreArray.Add(gotItem);

            cntr++;
        } while (cntr < SelectedIDList.Count() - 1);

        return scoreArray;
    }

    private ArrayList HTTPGetItems()
    {
        ArrayList scoreArray = new ArrayList();
        var result = WaitFor(ScenePrivate.HttpClient.Request, WebAppServer) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result.Response.Status: " + result.Response.Status);
        string Record = result.Response.Body;
        //Log.Write("Record: " + Record);

        List<string> SelectedIDList = new List<string>();
        SelectedIDList.Clear();
        SelectedIDList = Record.Split(',').ToList();
        int cntr = 0;
        int queuecntr = 0;
        do
        {
            //Log.Write("cntr: " + cntr + " ID: " + SelectedIDList[cntr]);

            GameScores gotItem = HTTPGetItem(Int32.Parse(SelectedIDList[cntr]));
            if (queuecntr > 7)
            {
                queuecntr = 0;
                Wait(TimeSpan.FromSeconds(1.2));
            }
            //Log.Write("gotItemID: " + gotItem.ID);
            scoreArray.Add(gotItem);
            queuecntr++;
            cntr++;
        } while (cntr < SelectedIDList.Count() - 1);

        return scoreArray;
    }

    private GameScores GetItem(int idIn)
    {
        string url = WebAppServer + "/" + idIn.ToString();
        //Log.Write("url: " + url);
        var result = WaitFor(ScenePrivate.HttpClient.Request, url) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result.Response.Status: " + result.Response.Status);
        GameScores gs = new GameScores();
        if (result.Response.Status == 200)
        {
            string Record = result.Response.Body;
            //Log.Write("Record: " + Record);
            gs = JSONtoGS(Record);
        }
        else
        {
            ScenePrivate.Chat.MessageAllUsers("Database Down Contact Granddad GotMojo");
        }

        return gs;
    
    }

    private GameScores HTTPGetItemForGameName(string gameIn, string nameIn)
    {
        string QueryURL = WebAppServer + "/Condition=" + "Game='" + gameIn + "'" + "AND" + "Name=" + nameIn + "'";
        Log.Write("QueryURL: " + QueryURL);
        var result = WaitFor(ScenePrivate.HttpClient.Request, QueryURL) as HttpClient.RequestData;
        Wait(TimeSpan.FromSeconds(0.12));
        //Log.Write("result.Response.Status: " + result.Response.Status);
        string Record = result.Response.Body;
        //Log.Write("Record: " + Record);
        GameScores gs = new GameScores();
        if (result.Response.Status == 200)
        {
            gs = JSONtoGS(Record);
        }
        else
        {
            ScenePrivate.Chat.MessageAllUsers("Database Down Contact Granddad GotMojo");
        }
        return gs;

    }

    private void HTTPDeleteItem(int idIn)
    {

    }

    private int HTTPGetScore(string inContestent)
    {
        List<string> SelectedIDList = new List<string>();

        string URLString = WebAppServer + "/Condition=Game='";
        //Log.Write("URLString: " + URLString);
        if (inContestent.Length > 0)
        {
            URLString = URLString + GameForScoreboard + "'ANDName='" + inContestent + "'";
            //Log.Write("URLString: " + URLString);
            var result = WaitFor(ScenePrivate.HttpClient.Request, URLString) as HttpClient.RequestData;
            //Log.Write("result.Response.Status: " + result.Response.Status);
            Wait(TimeSpan.FromSeconds(0.12));
            if (result.Response.Status == 200)
            {
                string Record = result.Response.Body;
                //Log.Write("Record: " + Record);
                if (Record == "NoneFound")
                {
                    return 0;
                }
                else
                {
                    SelectedIDList.Clear();
                    SelectedIDList = Record.Split(',').ToList();
                    GameScores gotItem = GetItem(Int32.Parse(SelectedIDList[0]));
                    //Log.Write("ID: " + gotItem.ID + " Game: " + gotItem.Game + " Name: " + gotItem.Name + " Date: " + gotItem.ScoreDate + " Score: " + gotItem.Score);
                    return gotItem.Score;
                }
            }
            else
            {
                //ModalDialog Dlg;
                //AgentPrivate agent = ScenePrivate.FindAgent(hitter);
                //if (agent == null) return;
                //Dlg = agent.Client.UI.ModalDialog;
                //WaitFor(Dlg.Show, "Are you sure you want to reset the entire scene?", "YES", "NO");
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }

    private void ClearScore(string inContestent)
    {
        GameScores gs = new GameScores();
        gs.Game = GameForScoreboard;
        gs.Name = inContestent;
        gs.ScoreDate = DateTime.Now.ToString();
        gs.Score = 0;
        if (Persistence)
        {
            bool ScoreResult = HTTPUpdateItem(gs);
        }
    }

    #endregion

    public override void Init()
    {
        //Log.Write("A");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        if (ContestantList.Count() > 0) Contestent1 = ContestantList[0];
        //Log.Write("B");
        if (ContestantList.Count() > 1) Contestent2 = ContestantList[1];
        //Log.Write("C");
        if (ContestantList.Count() > 2) Contestent3 = ContestantList[2];
        if (ContestantList.Count() > 3) Contestent4 = ContestantList[3];
        if (ContestantList.Count() > 4) Contestent5 = ContestantList[4];
        if (ContestantList.Count() > 5) Contestent6 = ContestantList[5];
        if (ContestantList.Count() > 6) Contestent7 = ContestantList[6];
        if (ContestantList.Count() > 7) Contestent8 = ContestantList[7];
        //Log.Write("D");
        if (InitialScoreList.Count() > 0) Score1 = InitialScoreList[0];
        //Log.Write("E");
        if (InitialScoreList.Count() > 1) Score2 = InitialScoreList[1];
        //Log.Write("F");
        if (InitialScoreList.Count() > 2) Score3 = InitialScoreList[2];
        if (InitialScoreList.Count() > 3) Score4 = InitialScoreList[3];
        if (InitialScoreList.Count() > 4) Score5 = InitialScoreList[4];
        if (InitialScoreList.Count() > 5) Score6 = InitialScoreList[5];
        if (InitialScoreList.Count() > 6) Score7 = InitialScoreList[6];
        if (InitialScoreList.Count() > 7) Score8 = InitialScoreList[7];
        //Log.Write("G");
        //getTop8GameScores(GameForScoreboard);
        //Log.Write("H");
        ScenePrivate.Chat.Subscribe(0, GetChatCommand);
        if (Persistence) GetCurrentScores();
        DisplayScoreboard();
        //Log.Write("I");
        SubscribeToScriptEvent("ScoreForContestent", ListenForScore);
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    private void getTop8GameScores(string gameIn)
    {
        GameScores gs = new GameScores();
        List<GameScores> CurrentScoreboardScores = new List<GameScores>();

        CurrentScoreboardScores = HTTPGetTop8GameScores(gameIn);

        gs = CurrentScoreboardScores[0];
        Contestent1 = CurrentScoreboardScores[0].Name;
        Score1 = CurrentScoreboardScores[0].Score;
        Contestent2 = CurrentScoreboardScores[0].Name;
        Score2 = CurrentScoreboardScores[0].Score;
        Contestent3 = CurrentScoreboardScores[0].Name;
        Score3 = CurrentScoreboardScores[0].Score;
        Contestent4 = CurrentScoreboardScores[0].Name;
        Score4 = CurrentScoreboardScores[0].Score;
        Contestent5 = CurrentScoreboardScores[0].Name;
        Score5 = CurrentScoreboardScores[0].Score;
        Contestent6 = CurrentScoreboardScores[0].Name;
        Score6 = CurrentScoreboardScores[0].Score;
        Contestent7 = CurrentScoreboardScores[0].Name;
        Score7 = CurrentScoreboardScores[0].Score;
        Contestent8 = CurrentScoreboardScores[0].Name;
        Score8 = CurrentScoreboardScores[0].Score;
    }

    private void GetCurrentScores()
    {
        if (Contestent1.Length > 0)
        {
            Score1 = HTTPGetScore(Contestent1);
            //Log.Write("GetCurrentScore Score: " + Score1);
        }
        if (Contestent2.Length > 0)
        {
            Score2 = HTTPGetScore(Contestent2);
        }
        if (Contestent3.Length > 0)
        {
            Score3 = HTTPGetScore(Contestent3);
        }
        if (Contestent4.Length > 0)
        {
            Score4 = HTTPGetScore(Contestent4);
        }
        if (Contestent5.Length > 0)
        {
            Score5 = HTTPGetScore(Contestent5);
        }
        if (Contestent6.Length > 0)
        {
            Score6 = HTTPGetScore(Contestent6);
        }
        if (Contestent7.Length > 0)
        {
            Score7 = HTTPGetScore(Contestent7);
        }
        if (Contestent8.Length > 0)
        {
            Score8 = HTTPGetScore(Contestent8);
        }
    }

    private void ListenForScore(ScriptEventData inScoreRecord)
    {
        if (inScoreRecord.Data == null)
        {
            return;
        }

        ISendContestentScore gotScore = inScoreRecord.Data.AsInterface<ISendContestentScore>();
        //Log.Write("gotScore.ContestentNumber: " + gotScore.ContestentNumber);
        //Log.Write("gotScore.ContestentName: " + gotScore.ContestentName);
        //Log.Write("gotScore.ScoreType: " + gotScore.ScoreType);
        //Log.Write("gotScore.Score: " + gotScore.Score);

        GameScores gs = new GameScores();
        gs.Game = GameForScoreboard;
        gs.Name = gotScore.ContestentName;
        gs.ScoreDate = DateTime.Now.ToString();
        gs.Score = gotScore.Score;
        //HTTPPostItem(gs);
        bool ScoreResult = true;
        if (Persistence)
        {
            ScoreResult = HTTPUpdateItem(gs);
        }

        if (ScoreResult)
        {
            if (gotScore.ContestentNumber == 1)
            {
                UpdateRow(0, gotScore.ContestentName);
                Score1 = CalcScore(gotScore.ScoreType, gotScore.Score, Score1);
                UpdateScore(0, FormatScore(Score1));
            }
            else if (gotScore.ContestentNumber == 2)
            {
                UpdateRow(1, gotScore.ContestentName);
                Score2 = CalcScore(gotScore.ScoreType, gotScore.Score, Score2);
                UpdateScore(1, FormatScore(Score2));
            }
            else if (gotScore.ContestentNumber == 3)
            {
                UpdateRow(2, gotScore.ContestentName);
                Score3 = CalcScore(gotScore.ScoreType, gotScore.Score, Score3);
                UpdateScore(2, FormatScore(Score3));
            }
            else if (gotScore.ContestentNumber == 4)
            {
                UpdateRow(3, gotScore.ContestentName);
                Score4 = CalcScore(gotScore.ScoreType, gotScore.Score, Score4);
                UpdateScore(3, FormatScore(Score4));
            }
            else if (gotScore.ContestentNumber == 5)
            {
                UpdateRow(5, gotScore.ContestentName);
                Score5 = CalcScore(gotScore.ScoreType, gotScore.Score, Score5);
                UpdateScore(4, FormatScore(Score5));
            }
            else if (gotScore.ContestentNumber == 6)
            {
                UpdateRow(5, gotScore.ContestentName);
                Score6 = CalcScore(gotScore.ScoreType, gotScore.Score, Score6);
                UpdateScore(5, FormatScore(Score6));
            }
            else if (gotScore.ContestentNumber == 7)
            {
                UpdateRow(6, gotScore.ContestentName);
                Score7 = CalcScore(gotScore.ScoreType, gotScore.Score, Score7);
                UpdateScore(6, FormatScore(Score7));
            }
            else if (gotScore.ContestentNumber == 8)
            {
                UpdateRow(7, gotScore.ContestentName);
                Score8 = CalcScore(gotScore.ScoreType, gotScore.Score, Score8);
                UpdateScore(7, FormatScore(Score8));
            }
        }
    }

    private int CalcScore(string ScoreTypeIn, int ScoreIn, int CurrentScore)
    {
        int ScoreToPost = 0;
        if (ScoreTypeIn == "Add")
        {
            ScoreToPost = CurrentScore + ScoreIn;
        }
        else if (ScoreTypeIn == "Replace")
        {
            ScoreToPost = ScoreIn;
        }
        else if (ScoreTypeIn == "Reset")
        {
            ScoreToPost = 0;
        }
        return ScoreToPost;
    }

    private string FormatScore(int ScoreIn)
    {
        string strScore = null;
        if (ScoreIn > 99999) ScoreIn = 99999;
        if (ScoreIn > 9999) strScore = ScoreIn.ToString();
        else if (ScoreIn > 999) strScore = " " + ScoreIn.ToString();
        else if (ScoreIn > 99) strScore = "  " + ScoreIn.ToString();
        else if (ScoreIn > 9) strScore = "   " + ScoreIn.ToString();
        else if (ScoreIn >= 0) strScore = "    " + ScoreIn.ToString();
        return strScore;
    }

    private void DisplayScoreboard()
    {
        if (Contestent1.Length > 0)
        {
            UpdateRow(0, Contestent1);
            UpdateScore(0, FormatScore(Score1));
        }
        if (Contestent2.Length > 0)
        {
            UpdateRow(1, Contestent2);
            UpdateScore(1, FormatScore(Score2));
        }
        if (Contestent3.Length > 0)
        {
            UpdateRow(2, Contestent3);
            UpdateScore(2, FormatScore(Score3));
        }
        if (Contestent4.Length > 0)
        {
            UpdateRow(3, Contestent4);
            UpdateScore(3, FormatScore(Score4));
        }
        if (Contestent5.Length > 0)
        {
            UpdateRow(4, Contestent5);
            UpdateScore(4, FormatScore(Score5));
        }
        if (Contestent6.Length > 0)
        {
            UpdateRow(5, Contestent6);
            UpdateScore(5, FormatScore(Score6));
        }
        if (Contestent7.Length > 0)
        {
            UpdateRow(6, Contestent7);
            UpdateScore(6, FormatScore(Score7));
        }
        if (Contestent8.Length > 0)
        {
            UpdateRow(7, Contestent8);
            UpdateScore(7, FormatScore(Score8));
        }
    }

    private void UpdateRow(int RowToUpdate, string Contestent)
    {
        //Log.Write("Row: " + RowToUpdate +  " Contestent: " + Contestent);
        string CharToSendOut;
        int NameLength = 0;
        if (Contestent.Length > 16)
        {
            NameLength = 16;
        } 
        else
        {
            NameLength = Contestent.Length;
        }

        string MessageEvent = "LetterSent" + RowToUpdate;
        int cntr = 0;
        do
        {
            CharToSendOut = Contestent.Substring(cntr, 1);
            SendChar sendChar = new SendChar();
            sendChar.CharIndex = cntr;
            sendChar.CharToSend = CharToSendOut;
            //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
            PostScriptEvent(ScriptId.AllScripts, MessageEvent, sendChar);  //used by synth
            cntr++;
        } while (cntr < NameLength);

        //Send Mesasge to Vote Butttons
        string ContestentInfoEvent = "ContestentInfo";
        SendContestent sendContestent = new SendContestent();
        sendContestent.ContestentNumber = RowToUpdate;
        sendContestent.ContestentName = Contestent;
        PostScriptEvent(ScriptId.AllScripts, ContestentInfoEvent, sendContestent);

    }

    private void UpdateScore(int RowToUpdate, string Score)
    {
        //Log.Write("Row: " + RowToUpdate +  " Score: " + Score);
        string CharToSendOut;
        int NameLength = 0;
        if (Score.Length > 6)
        {
            NameLength = 6;
        }
        else
        {
            NameLength = Score.Length;
        }

        string MessageEvent = "ScoreSent" + RowToUpdate;
        int cntr = 0;
        do
        {
            CharToSendOut = Score.Substring(cntr, 1);
            SendChar sendChar = new SendChar();
            sendChar.CharIndex = cntr;
            sendChar.CharToSend = CharToSendOut;
            //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
            PostScriptEvent(ScriptId.AllScripts, MessageEvent, sendChar);  //used by synth
            cntr++;
        } while (cntr < NameLength);
    }

    private void GetChatCommand(ChatData Data)
    {
        //Log.Write("Chat From: " + Data.SourceId);
        //Log.Write("Chat person: " + ScenePrivate.FindAgent(Data.SourceId).AgentInfo.Name);
        AgentPrivate agent = ScenePrivate.FindAgent(Data.SourceId);
        ValidUsers.Clear();
        ValidUsers = UsersToListenTo.Split(',').ToList();
        if (UsersToListenTo.Contains("ALL"))
        {
            string DataCmd = Data.Message;
            //Log.Write("DataCmd: " + DataCmd);
            ParseCommands(DataCmd);
        }
        else
        {
            foreach (string ValidUser in ValidUsers)
            {
                //Log.Write("ValidUser: " + ValidUser);
                if (ScenePrivate.FindAgent(Data.SourceId).AgentInfo.Name == ValidUser.Trim())
                {
                    string DataCmd = Data.Message;
                    ParseCommands(DataCmd);
                }
            }
        }

    }

    private void ParseCommands(string DataCmdIn)
    {
        //Log.Write("DataCmdIn: " + DataCmdIn);
        if (DataCmdIn.Contains("clear"))
        {
            string OriginalScoreTypeForScoreboard = ScoreTypeForScoreboard;
            ScoreTypeForScoreboard = "Clear";
            if (Contestent1.Length > 0)
            {
                ClearScore(Contestent1);
                Score1 = 0;
            }
            if (Contestent2.Length > 0)
            {
                ClearScore(Contestent2);
                Score2 = 0;
            }
            if (Contestent3.Length > 0)
            {
                ClearScore(Contestent3);
                Score3 = 0;
            }
            if (Contestent4.Length > 0)
            {
                ClearScore(Contestent4);
                Score4 = 0;
            }
            if (Contestent5.Length > 0)
            {
                ClearScore(Contestent5);
                Score5 = 0;
            }
            if (Contestent6.Length > 0)
            {
                ClearScore(Contestent6);
                Score6 = 0;
            }
            if (Contestent7.Length > 0)
            {
                ClearScore(Contestent7);
                Score7 = 0;
            }
            if (Contestent8.Length > 0)
            {
                ClearScore(Contestent8);
                Score8 = 0;
            }
            ScoreTypeForScoreboard = OriginalScoreTypeForScoreboard;
            DisplayScoreboard();
        }
    }

}