// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

public class LogoMoverWithQuest : SceneObjectScript
{
    // Public properties

    [DefaultValue(true)]
    public readonly bool ShowHelp;

    [DefaultValue(1.0f)]
    public readonly float MoveScale;

    [DefaultValue(5.0f)]
    public readonly float DefaultSpeed;

    [DefaultValue(180.0f)]
    public readonly float RotationSpeed;


    // Simple script compatible data structures

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


    // Privates

    Dictionary<string, Action<float, AgentPrivate>> _floatCommands = new Dictionary<string, Action<float, AgentPrivate>>();
    Dictionary<string, Action<AgentPrivate>> _noArgCommands = new Dictionary<string, Action<AgentPrivate>>();

    float _rotationSpeed;
    float _speed;

    SimpleData _simpleData = null;


    // Logic!

    public override void Init()
    {
        _floatCommands["fd"] = MoveForward;
        _floatCommands["bk"] = MoveBackward;
        _floatCommands["rt"] = TurnRight;
        _floatCommands["lt"] = TurnLeft;

        _floatCommands["move_speed"] = SetMoveSpeed;
        _floatCommands["spin_speed"] = SetSpinSpeed;

        _noArgCommands["home"] = Reset;

        _rotationSpeed = RotationSpeed;
        _speed = DefaultSpeed;

        _simpleData = new SimpleData(this);
        _simpleData.ObjectId = ObjectPrivate.ObjectId;
        _simpleData.SourceObjectId = ObjectPrivate.ObjectId;

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    void Reset(AgentPrivate agent)
    {
        ObjectPrivate.Mover.StopAndClear();
        ObjectPrivate.Mover.AddMove(ObjectPrivate.InitialPosition, ObjectPrivate.InitialRotation);
    }

    void SetMoveSpeed(float speed, AgentPrivate agent)
    {
        _speed = speed;
    }

    void SetSpinSpeed(float speed, AgentPrivate agent)
    {
        _rotationSpeed = speed;
    }

    void SendQuestEvent(string eventName, AgentPrivate agent)
    {
        if (agent != null)
        {
            _simpleData.AgentInfo = agent.AgentInfo;

            PostScriptEvent("logo_" + eventName, _simpleData);
        }
    }

    void MoveForward(float distance, AgentPrivate agent)
    {
        if (ObjectPrivate.IsMovable)
        {
            Vector translation = new Vector(0.0f, distance, 0.0f);

            if (distance != 0.0f)
                SendQuestEvent("move", agent);

            if (_speed != 0.0f)
            {
                double moveTime = Math.Abs(distance * MoveScale / _speed);

                ObjectPrivate.Mover.AddTranslateOffset(translation, moveTime, MoveMode.Smoothstep);
            }
            else
            {
                ObjectPrivate.Mover.AddTranslateOffset(translation);
            }
        }
    }

    void MoveBackward(float distance, AgentPrivate agent)
    {
        MoveForward(-distance, agent);
    }

    void TurnLeft(float degrees, AgentPrivate agent)
    {
        if (ObjectPrivate.IsMovable)
        {
            if (degrees == 47.0f)
                SendQuestEvent("turn", agent);
            else if (Math.Abs(degrees) == 1080.0f)  // Exactly 3 spins left or right
                SendQuestEvent("spin", agent);

            if (_rotationSpeed == 0.0f)
            {
                var rotation = Quaternion.FromEulerAngles(new Vector(0.0f, 0.0f, degrees * Mathf.RadiansPerDegree));
                ObjectPrivate.Mover.AddRotateOffset(rotation);
            }
            else
            {
                if (Math.Abs(degrees) > 180.0f)
                {
                    double totalMoveTime = Math.Abs(degrees / _rotationSpeed);
                    int turns = (int) Math.Ceiling(Math.Abs(degrees) / 180.0f);

                    var rotation = Quaternion.FromEulerAngles(new Vector(0.0f, 0.0f, (degrees / (float)turns) * Mathf.RadiansPerDegree));

                    for (int i = 0; i < turns; i++)
                        ObjectPrivate.Mover.AddRotateOffset(rotation, totalMoveTime / (double)turns, MoveMode.Linear);
                }
                else
                {
                    double moveTime = Math.Abs(degrees / _rotationSpeed);
                    var rotation = Quaternion.FromEulerAngles(new Vector(0.0f, 0.0f, degrees * Mathf.RadiansPerDegree));
                    ObjectPrivate.Mover.AddRotateOffset(rotation, moveTime, MoveMode.Linear);
                }
            }
        }
    }

    void TurnRight(float degrees, AgentPrivate agent)
    {
        TurnLeft(-degrees, agent);
    }

    void OnChat(ChatData data)
    {
        // Find the agent who wrote this chat message
        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);

        string[] msg = data.Message.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (msg[0].StartsWith("/"))
            msg[0] = msg[0].Substring(1);

        if (msg.Length == 1)
        {
            if (_noArgCommands.ContainsKey(msg[0]))
                _noArgCommands[msg[0]](agent);
            else if (ShowHelp && (msg[0] == "help"))
            {
                string helpMessage = "LogoMover includes the following commands:\n";

                foreach (var noArgCmd in _noArgCommands.Keys)
                    helpMessage += noArgCmd + "\n";
                foreach (var oneArgCmd in _floatCommands.Keys)
                    helpMessage += oneArgCmd + " <value>\n";
                helpMessage += "repeat N [other commands]\n";

                helpMessage += "\nExamples:\n";
                helpMessage += "fd 1 -- move forward 1\n";
                helpMessage += "rt 90 -- turn right 90 degrees\n";
                helpMessage += "repeat 4 [lt 90 bk 1] -- turn left 90 degrees and go back 1, four times";

                try { ScenePrivate.FindAgent(data.SourceId).SendChat(helpMessage); } catch { }
            }
        }
        else if (msg.Length > 1)
        {
            InterpretCommandWithArgs(msg, agent);
        }
    }

    bool InterpretCommandWithArgs(string[] commandWithArgs, AgentPrivate agent)
    {
        bool validCommands = true;

        if (_floatCommands.ContainsKey(commandWithArgs[0]))
        {
            float scalar = 0.0f;
            if (float.TryParse(commandWithArgs[1], out scalar))
            {
                _floatCommands[commandWithArgs[0]](scalar, agent);
            }
            else
                validCommands = false;

            if (commandWithArgs.Length > 2)
                validCommands &= InterpretCommandWithArgs(commandWithArgs.Skip(2).ToArray(), agent);
        }
        else if ((commandWithArgs[0] == "repeat") && (commandWithArgs.Length > 2) && (commandWithArgs.Length % 2 == 0))
        {
            int count = 0;
            if (int.TryParse(commandWithArgs[1], out count))
            {
                var newCommandWithArgs = commandWithArgs.Skip(2).ToArray();

                var firstCmd = newCommandWithArgs[0];
                var lastCmd = newCommandWithArgs[newCommandWithArgs.Length - 1];

                newCommandWithArgs[0] = firstCmd.Substring(1);  // Remove leading '['
                newCommandWithArgs[newCommandWithArgs.Length - 1] = lastCmd.Remove(lastCmd.Length - 1);  // Remove trailing ']'

                for (int i = 0; i < count; i++)
                    validCommands &= InterpretCommandWithArgs(newCommandWithArgs, agent);

                // Detect if the agent is making a square or star
                if ((count == 4) && (newCommandWithArgs.Length == 4)) // potentially a square
                {
                    bool validSquare = true;
                    validSquare &= (newCommandWithArgs[0] == "fd");

                    float sideLength = 0.0f;
                    validSquare &= (float.TryParse(newCommandWithArgs[1], out sideLength) && (sideLength > 0.0f));

                    validSquare &= ((newCommandWithArgs[2] == "rt") || (newCommandWithArgs[2] == "lt"));
                    validSquare &= (newCommandWithArgs[3] == "90");

                    if (validSquare)
                        SendQuestEvent("square", agent);
                }
                else if ((count == 5) && (newCommandWithArgs.Length == 8)) // potentially a star
                {
                    bool validStar = true;

                    validStar &= (newCommandWithArgs[0] == "fd") && (newCommandWithArgs[4] == "fd");  // validate forward movement

                    float movementDistance = 0.0f;
                    validStar &= (newCommandWithArgs[1] == newCommandWithArgs[5]) && float.TryParse(newCommandWithArgs[1], out movementDistance); // identical length parts
                    validStar &= (movementDistance > 0.0f);  // positive movement

                    validStar &= ((newCommandWithArgs[2] == "rt") && (newCommandWithArgs[6] == "lt")) ||  // right and then left or left and then right
                                 ((newCommandWithArgs[2] == "lt") && (newCommandWithArgs[6] == "rt"));
                    validStar &= ((newCommandWithArgs[3] == "144") && (newCommandWithArgs[7] == "72")) ||  // validate angles for a 5-pointed star
                                 ((newCommandWithArgs[3] == "72") && (newCommandWithArgs[7] == "144"));

                    if (validStar)
                        SendQuestEvent("star", agent);
                }
            }
            else
                validCommands = false;
        }

        return validCommands;
    }
}
