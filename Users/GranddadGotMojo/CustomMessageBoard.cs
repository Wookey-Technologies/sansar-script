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

public class MessageBoard : SceneObjectScript
{
    #region ConstantsVariables

    [DefaultValue("")]
    [DisplayName("Board Type: ")]
    public string BoardType = "";

    [DefaultValue("")]
    [DisplayName("MessageEvent: ")]
    public string MessageEvent = "";

    [DefaultValue("")]
    [DisplayName("Row Name: ")]
    public string BoardName = "";

    [DefaultValue(0)]
    [DisplayName("Board Rows: ")]
    public int BoardRows = 0;

    [DefaultValue(0)]
    [DisplayName("Board Columns: ")]
    public int BoardColumns = 0;

    [DefaultValue(false)]
    [DisplayName("Center Text: ")]
    public bool CenterMyText = false;

    [DefaultValue(false)]
    [DisplayName("Space is Black: ")]
    public bool BlackSpace = false;

    public List<string> Clues = new List<string>();
    public List<string> Message1Text = new List<string>();
    public List<string> Message2Text = new List<string>();
    public List<string> Message3Text = new List<string>();
    public List<string> Message4Text = new List<string>();
    public List<string> Message5Text = new List<string>();
    public List<string> Message6Text = new List<string>();
    public List<string> Message7Text = new List<string>();
    public List<string> Message8Text = new List<string>();
    public List<string> Message9Text = new List<string>();

    // The sound resource to play
    [DisplayName("Match Sound")]
    public readonly SoundResource MatchSound;

    // The sound resource to play
    [DisplayName("Miss Sound")]
    public readonly SoundResource MissSound;

    [DefaultValue("ALL")]
    [DisplayName("Valid User List:")]
    public string UsersToListenTo = "ALL";

    private List<string> ValidUsers = new List<string>();

    private List<string> CurrentText = new List<string>();
    private List<string> HiddenText = new List<string>();

    private int CurrentMessageNumber;
    AgentInfo AgentName;

    private PlayHandle currentPlayHandle;
    private PlaySettings playSettings = PlaySettings.PlayOnce;

    public class SendChar : Reflective
    {
        public int CharIndex { get; set; }
        public string CharToSend { get; set; }
    }

    #endregion

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

    private void RevealPuzzle(ScriptEventData inMessage)
    {
        DisplayBoard(CurrentText);
    }

    private void PreviousGame(ScriptEventData inMessage)
    {
        if (CurrentMessageNumber < 2)
        {
            CurrentMessageNumber = 1;
        }
        else if (CurrentMessageNumber < 10)
        {
            CurrentMessageNumber--;
        }
        LoadMessage(CurrentMessageNumber);
        if (BoardType == "WOF")
        {
            //Display Clue
            Log.Write("CurrentMessageNumber: " + CurrentMessageNumber);
            if (CurrentMessageNumber <= Clues.Count())
            {
                Log.Write("Displaying Clue");
                DisplayClue(Clues[CurrentMessageNumber - 1]);
            }
        }
    }

    private void NextGame(ScriptEventData inMessage)
    {
        if (CurrentMessageNumber > 8)
        {
            CurrentMessageNumber = 9;
        }
        else
        {
            CurrentMessageNumber++;
        }
        LoadMessage(CurrentMessageNumber);
        if (BoardType == "WOF")
        {
            //Display Clue
            Log.Write("NextGame CurrentMessageNumber: " + CurrentMessageNumber);
            Log.Write("CurrentMessageNumber: " + CurrentMessageNumber + "Clues: " + Clues.Count());
            if (CurrentMessageNumber <= Clues.Count())
            {
                DisplayClue(Clues[CurrentMessageNumber - 1]);
            }
        }
    }

    private void ListenForMessage(ScriptEventData inMessage)
    {
        Log.Write("Listen for Message: " + inMessage.Message);
        if (inMessage.Data == null)
        {
            return;
        }
        ISimpleData simpleData = inMessage.Data?.AsInterface<ISimpleData>();

        string strActiveMessage = inMessage.Message;
        Log.Write("inMessage: " + strActiveMessage);
        strActiveMessage = strActiveMessage.Substring(MessageEvent.Length, 1);
      //Log.Write("inMessage2: " + strActiveMessage);
        int ActiveGame = Int32.Parse(strActiveMessage);

        LoadMessage(ActiveGame);
    }

    private void LookUpLetter(ScriptEventData inMessage)
    {
        if (inMessage.Data == null)
        {
            return;
        }
        ISimpleData simpleData = inMessage.Data?.AsInterface<ISimpleData>();

        string strActiveLetter = inMessage.Message;
        //Log.Write("inMessage: " + strActiveMessage);
        strActiveLetter = strActiveLetter.Substring(6, 1);
        Log.Write("Letterin: " + strActiveLetter);
        RevealLetter(strActiveLetter);
    }

    #endregion

    public override void Init()
    {
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal

        if (BoardType == "Text")
        {
            ScenePrivate.Chat.Subscribe(0, GetChatCommand);
        }
        
        SubscribeToScriptEvent(MessageEvent + 1, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 2, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 3, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 4, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 5, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 6, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 7, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 8, ListenForMessage);
        SubscribeToScriptEvent(MessageEvent + 9, ListenForMessage);
        SubscribeToScriptEvent("PreviousGame", PreviousGame);
        SubscribeToScriptEvent("NextGame", NextGame);
        SubscribeToScriptEvent("RevealPuzzle", RevealPuzzle);
        SubscribeToScriptEvent("LetterA", LookUpLetter);
        SubscribeToScriptEvent("LetterB", LookUpLetter);
        SubscribeToScriptEvent("LetterC", LookUpLetter);
        SubscribeToScriptEvent("LetterD", LookUpLetter);
        SubscribeToScriptEvent("LetterE", LookUpLetter);
        SubscribeToScriptEvent("LetterF", LookUpLetter);
        SubscribeToScriptEvent("LetterG", LookUpLetter);
        SubscribeToScriptEvent("LetterH", LookUpLetter);
        SubscribeToScriptEvent("LetterI", LookUpLetter);
        SubscribeToScriptEvent("LetterJ", LookUpLetter);
        SubscribeToScriptEvent("LetterK", LookUpLetter);
        SubscribeToScriptEvent("LetterL", LookUpLetter);
        SubscribeToScriptEvent("LetterM", LookUpLetter);
        SubscribeToScriptEvent("LetterN", LookUpLetter);
        SubscribeToScriptEvent("LetterO", LookUpLetter);
        SubscribeToScriptEvent("LetterP", LookUpLetter);
        SubscribeToScriptEvent("LetterQ", LookUpLetter);
        SubscribeToScriptEvent("LetterR", LookUpLetter);
        SubscribeToScriptEvent("LetterS", LookUpLetter);
        SubscribeToScriptEvent("LetterT", LookUpLetter);
        SubscribeToScriptEvent("LetterU", LookUpLetter);
        SubscribeToScriptEvent("LetterV", LookUpLetter);
        SubscribeToScriptEvent("LetterW", LookUpLetter);
        SubscribeToScriptEvent("LetterX", LookUpLetter);
        SubscribeToScriptEvent("LetterY", LookUpLetter);
        SubscribeToScriptEvent("LetterZ", LookUpLetter);
        int cntr = 0;
        do
        {
            CurrentText.Add("");
            HiddenText.Add("");
            cntr++;
        } while (cntr < BoardRows);

        CurrentMessageNumber = 0;
        //Test only

    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    private void LoadMessage(int MessageNumber)
    {
        Log.Write("inGame: " + MessageNumber);
        CurrentText.Clear();
        HiddenText.Clear();
        int cntr = 0;

        if (MessageNumber == 1)
        {
                do
                {

                    CurrentText.Add(Message1Text[cntr]);
                    HiddenText.Add(Message1Text[cntr]);
                    cntr++;
                } while (cntr < Message1Text.Count());
        }
        else if (MessageNumber == 2)
        {
            do
            {
                CurrentText.Add(Message2Text[cntr]);
                HiddenText.Add(Message2Text[cntr]);
                cntr++;
            } while (cntr < Message2Text.Count());
        }
        else if (MessageNumber == 3)
        {
            do
            {
                CurrentText.Add(Message3Text[cntr]);
                HiddenText.Add(Message3Text[cntr]);
                cntr++;
            } while (cntr < Message3Text.Count());
        }
        else if (MessageNumber == 4)
        {
            do
            {
                CurrentText.Add(Message4Text[cntr]);
                HiddenText.Add(Message4Text[cntr]);
                cntr++;
            } while (cntr < Message4Text.Count());
        }
        else if (MessageNumber == 5)
        {
            do
            {
                CurrentText.Add(Message5Text[cntr]);
                HiddenText.Add(Message5Text[cntr]);
                cntr++;
            } while (cntr < Message5Text.Count());
        }
        else if (MessageNumber == 6)
        {
            do
            {
                CurrentText.Add(Message6Text[cntr]);
                HiddenText.Add(Message6Text[cntr]);
                cntr++;
            } while (cntr < Message6Text.Count());
        }
        else if (MessageNumber == 7)
        {
            do
            {
                CurrentText.Add(Message7Text[cntr]);
                HiddenText.Add(Message7Text[cntr]);
                cntr++;
            } while (cntr < Message7Text.Count());
        }
        else if (MessageNumber == 8)
        {
            do
            {
                CurrentText.Add(Message8Text[cntr]);
                HiddenText.Add(Message8Text[cntr]);
                cntr++;
            } while (cntr < Message8Text.Count());
        }
        else if (MessageNumber == 9)
        {
            do
            {
                CurrentText.Add(Message9Text[cntr]);
                HiddenText.Add(Message9Text[cntr]);
                cntr++;
            } while (cntr < Message9Text.Count());
        }
        if (BoardType == "Text")
        {
            DisplayBoard(CurrentText);
        }
        else if (BoardType == "WOF")
        {
            DisplayHiddenBoard(CurrentText);
        }
    }

    private void DisplayBoard(List<string> inCurrentText)
    {
        string CharToSendOut;
        int rowCntr = 0;

        do
        {
            //Log.Write("rowCntr: " + rowCntr);
            string CurrentText = "";
            //Log.Write("LineLength: " + inCurrentText[rowCntr].Length);
            if (rowCntr + 1 > inCurrentText.Count())
            {
                for (int i = 0; i < BoardColumns; i++)
                {
                    CurrentText = CurrentText + " ";
                }
                //Log.Write("New Length: " + CurrentText.Length);
            }
            else CurrentText = inCurrentText[rowCntr];

            if (CenterMyText)
            {
                CurrentText = CenterText(CurrentText);
            }

            if (BlackSpace)
            {
                CurrentText = BlackSpacesReplace(CurrentText);
            }

            int textLength = CurrentText.Length;

            //Log.Write("inCurrentText[" + rowCntr + "]: " + inCurrentText + "  Length: " + textLength);
            string MessageEventOut = BoardName + (rowCntr + 1).ToString();
            //Log.Write("MessageEvent: " + MessageEvent);
            //Log.Write("CurrentText: " + CurrentText);
            int columnCntr = 0;

            do
            {
                //Log.Write("columnCntr: " + columnCntr);
                CharToSendOut = CurrentText.Substring(columnCntr, 1);
                //Log.Write("CharToSendOut: " + CharToSendOut);
                SendChar sendChar = new SendChar();
                sendChar.CharIndex = columnCntr;
                sendChar.CharToSend = CharToSendOut;
                //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
                PostScriptEvent(ScriptId.AllScripts, MessageEventOut, sendChar);
                columnCntr++;
            } while (columnCntr < textLength);

            rowCntr++;
            //Log.Write("Out");
        } while (rowCntr < BoardRows);
    }

    private string CenterText(string textToCenter)
    {

        //Log.Write("textToCenter: " + textToCenter);

        int indent = (BoardColumns - textToCenter.Length)/2;
        string indentSpace = "";
        //Log.Write("indent: " + indent);

        for (int i = 0; i < indent; i++)
        {
            indentSpace = indentSpace + " ";
        }

        //Log.Write("indentSpace Length: " + indentSpace.Length);

        string centeredText = indentSpace + textToCenter;
        //Log.Write("centeredText1: " + centeredText);

        //Fill the back with blanks
        string trailerSpace = "";
        int trailerCount = BoardColumns - centeredText.Length;
        //Log.Write("trailerCount: " + trailerCount);

        for (int i = 0; i < trailerCount; i++)
        {
            trailerSpace = trailerSpace + " ";
        }
        centeredText = centeredText + trailerSpace;
        //Log.Write("centeredText: " + centeredText);
        //Log.Write("centeredText Length: " + centeredText.Length);

        return centeredText;
    }

    private string BlackSpacesReplace(string inString)
    {
        string fixedString = "";
        Char Blank = ' ';
        Char BlackChar = '*';
        fixedString = inString.Replace(Blank, BlackChar);

        return fixedString; 
    }

    private void ClearLine(int lineNumberIn)
    {
        //Log.Write("in ClearLine");
        string MessageEventClear = BoardName + (lineNumberIn + 1).ToString();
        //Log.Write("MessageEventClear: " + MessageEventClear);
        for (int i = 0; i < BoardColumns; i++)
        {
            //Log.Write("A");
            SendChar sendCharClr = new SendChar();
            //Log.Write("B");
            sendCharClr.CharIndex = i;
            //Log.Write("CharIndex: " + sendCharClr.CharIndex);
            sendCharClr.CharToSend = " ";
            //Log.Write("SendCharClr: " + sendCharClr.CharToSend);
            PostScriptEvent(ScriptId.AllScripts, MessageEventClear, sendCharClr);
        }
    }

    private void GetChatCommand(ChatData Data)
    {
        //Log.Write("Chat From: " + Data.SourceId);
        Log.Write("Chat person: " + ScenePrivate.FindAgent(Data.SourceId).AgentInfo.Name);
        AgentPrivate agent = ScenePrivate.FindAgent(Data.SourceId);
        ValidUsers.Clear();
        ValidUsers = UsersToListenTo.Split(',').ToList();
        Log.Write("UsersToListenTo: " + UsersToListenTo);
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
        if (DataCmdIn.Contains("/" + BoardName))
        {
            int start = BoardName.Length + 3;
            //Log.Write("DataCmdIn Length: " + DataCmdIn.Length);
            //Log.Write("BoardName Length: " + BoardName.Length);
            int end = DataCmdIn.Length - BoardName.Length - 3;

            //Log.Write("start: " + start + " end: " + end);
            string TextToDisplay = DataCmdIn.Substring(start, end);
            //Log.Write("start - 2: " + (start - 2));
            string strLineNumber = DataCmdIn.Substring(start - 2, 1);
            //Log.Write("strLineNumber: " + strLineNumber);
            int lineNumber = Int32.Parse(strLineNumber) - 1;
            //Log.Write("lineNumber: " + lineNumber);
            ClearLine(lineNumber);
            //Log.Write("After");
            CurrentText[lineNumber] = TextToDisplay;
            //Log.Write("Text To Display: " + TextToDisplay);
            //Log.Write("Current Text: " + CurrentText[lineNumber]);
            DisplayBoard(CurrentText);
            //Log.Write("After DisplayBoard");
        }
        else if (DataCmdIn.Contains("/Clear1"))
        {
            ClearLine(0);
            CurrentText[0] = "";
            ClearLine(0);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear2"))
        {
            ClearLine(1);
            CurrentText[1] = "";
            ClearLine(1);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear3"))
        {
            ClearLine(2);
            CurrentText[2] = "";
            ClearLine(2);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear4"))
        {
            ClearLine(3);
            CurrentText[3] = "";
            ClearLine(3);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear5"))
        {
            ClearLine(4);
            CurrentText[4] = "";
            ClearLine(4);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear6"))
        {
            ClearLine(5);
            CurrentText[5] = "";
            ClearLine(5);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear6"))
        {
            ClearLine(6);
            CurrentText[6] = "";
            ClearLine(6);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/Clear8"))
        {
            ClearLine(7);
            CurrentText[7] = "";
            ClearLine(7);
            DisplayBoard(CurrentText);
        }
        else if (DataCmdIn.Contains("/ClearAll"))
        {
            Log.Write("Current Text Members: " + CurrentText.Count());
            for (int i = 0; i < CurrentText.Count(); i++)
            {
                CurrentText[i] = "";
                ClearLine(i);
            }
            DisplayBoard(CurrentText);
        }
    }

    #region WheelOfFortune

    private void DisplayClue(string inCurrentText)
    {
        string CharToSendOut;

        Log.Write("In Display Clue");
        string CurrentText = "";
        //Log.Write("LineLength: " + inCurrentText[rowCntr].Length);
        CurrentText = inCurrentText;

        int tempLength = BoardColumns;
        BoardColumns = 25;

        if (CenterMyText)
        {
            CurrentText = CenterText(CurrentText);
        }

        if (BlackSpace)
        {
            CurrentText = BlackSpacesReplace(CurrentText);
        }

        int textLength = CurrentText.Length;

        //Log.Write("inCurrentText[" + rowCntr + "]: " + inCurrentText + "  Length: " + textLength);
        string MessageEventOut = "Line1";
        Log.Write("MessageEventOut: " + MessageEventOut);
        Log.Write("CurrentText: " + CurrentText);

        for (int i = 0; i < BoardColumns; i++)
        {
            //Log.Write("A");
            SendChar sendCharClr = new SendChar();
            //Log.Write("B");
            sendCharClr.CharIndex = i;
            //Log.Write("CharIndex: " + sendCharClr.CharIndex);
            sendCharClr.CharToSend = " ";
            //Log.Write("SendCharClr: " + sendCharClr.CharToSend);
            PostScriptEvent(ScriptId.AllScripts, MessageEventOut, sendCharClr);
        }

        int columnCntr = 0;
        do
        {
            //Log.Write("columnCntr: " + columnCntr);
            CharToSendOut = CurrentText.Substring(columnCntr, 1);
            //Log.Write("CharToSendOut: " + CharToSendOut);
            SendChar sendChar = new SendChar();
            sendChar.CharIndex = columnCntr;
            sendChar.CharToSend = CharToSendOut;
            //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
            PostScriptEvent(ScriptId.AllScripts, MessageEventOut, sendChar);
            columnCntr++;
        } while (columnCntr < textLength);

        BoardColumns = tempLength;
    }

    private void DisplayHiddenBoard(List<string> inCurrentText)
    {
        string CharToSendOut;
        int rowCntr = 0;

        do
        {
            //Log.Write("rowCntr: " + rowCntr);
            string CurrentText = "";
            //Log.Write("LineLength: " + inCurrentText[rowCntr].Length);
            if (rowCntr + 1 > inCurrentText.Count())
            {
                for (int i = 0; i < BoardColumns; i++)
                {
                    CurrentText = CurrentText + " ";
                }
                //Log.Write("New Length: " + CurrentText.Length);
            }
            else CurrentText = inCurrentText[rowCntr];

            if (CenterMyText)
            {
                CurrentText = CenterText(CurrentText);
            }

            if (BlackSpace)
            {
                CurrentText = BlackSpacesReplace(CurrentText);
            }

            int textLength = CurrentText.Length;

            //Log.Write("inCurrentText[" + rowCntr + "]: " + inCurrentText + "  Length: " + textLength);
            string MessageEventOut = BoardName + (rowCntr + 1).ToString();

            Log.Write("MessageEvent: " + MessageEvent);
            int columnCntr = 0;

            do
            {
                //Log.Write("columnCntr: " + columnCntr);
                CharToSendOut = CurrentText.Substring(columnCntr, 1);
                if (CharToSendOut != "*")
                {
                    CharToSendOut = " ";
                }

                //Log.Write("CharToSendOut: " + CharToSendOut);
                SendChar sendChar = new SendChar();
                sendChar.CharIndex = columnCntr;
                sendChar.CharToSend = CharToSendOut;
                //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
                PostScriptEvent(ScriptId.AllScripts, MessageEventOut, sendChar);
                columnCntr++;
            } while (columnCntr < textLength);

            rowCntr++;
            //Log.Write("Out");
        } while (rowCntr < BoardRows);

    }

    private void RevealLetter(string inLetter)
    {
        int rowCntr = 0;
        bool hit = false;
        do
        {
            //Log.Write("Current Text: " + CurrentText[rowCntr]);
            //Log.Write("Hidden Text: " + HiddenText[rowCntr]);
            int columnCntr = 0;
            string MessageEventOut = BoardName + (rowCntr + 1).ToString();
            string LineText = HiddenText[rowCntr];
            if (CenterMyText)
            {
                LineText = CenterText(LineText);
            }

            do
            {
                string boardLetter = LineText.Substring(columnCntr, 1).ToUpper();
                //Log.Write("row: " + rowCntr + " column: " + columnCntr + " boardLetter: " + boardLetter + " inLetter: " + inLetter);
                if (boardLetter == inLetter)
                {
                    //Log.Write("LetterMatch");
                    SendChar sendChar = new SendChar();
                    sendChar.CharIndex = columnCntr;
                    sendChar.CharToSend = inLetter;
                    //Log.Write("MessageEvent: " + MessageEvent + " Sending Letter: " + CharToSendOut + " To Letter #: " + sendChar.CharIndex);
                    PostScriptEvent(ScriptId.AllScripts, MessageEventOut, sendChar);
                    currentPlayHandle = ScenePrivate.PlaySound(MatchSound, playSettings);
                    hit = true;
                    Wait(TimeSpan.FromSeconds(0.5));
                }
                columnCntr++;
            } while (columnCntr < LineText.Length);

            rowCntr++;
        } while (rowCntr < HiddenText.Count());
        if (!hit) currentPlayHandle = ScenePrivate.PlaySound(MissSound, playSettings);
        //DisplayBoard(CurrentText);
    }

    #endregion
}