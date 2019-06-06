// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

public class LogoMover : SceneObjectScript
{
    [DefaultValue(true)]
    public readonly bool ShowHelp;

    [DefaultValue(1.0f)]
    public readonly float MoveScale;

    [DefaultValue(5.0f)]
    public readonly float DefaultSpeed;

    [DefaultValue(180.0f)]
    public readonly float RotationSpeed;


    Dictionary<string, Action<float>> _floatCommands = new Dictionary<string, Action<float>>();
    Dictionary<string, Action> _noArgCommands = new Dictionary<string, Action>();

    Queue<Action> _commandQueue = new Queue<Action>();

    float _rotationSpeed;
    float _speed;

    bool _queueMethods;


    public override void Init()
    {
        _floatCommands["fd"] = MoveForward;
        _floatCommands["bk"] = MoveBackward;
        _floatCommands["rt"] = TurnRight;
        _floatCommands["lt"] = TurnLeft;

        _noArgCommands["queue"] = StartQueueing;
        _noArgCommands["go"] = RunQueue;
        _noArgCommands["home"] = Reset;

        _rotationSpeed = RotationSpeed;
        _speed = DefaultSpeed;

        _queueMethods = false;

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    void Reset()
    {
        ObjectPrivate.Mover.StopAndClear();
        ObjectPrivate.Mover.AddMove(ObjectPrivate.InitialPosition, ObjectPrivate.InitialRotation);
    }

    void StartQueueing()
    {
        _queueMethods = true;
    }

    void RunQueue()
    {
        _queueMethods = false;

        while (_commandQueue.Count > 0)
        {
            var a = _commandQueue.Dequeue();
            a();
        }
    }

    void MoveForward(float distance)
    {
        if (_queueMethods)
        {
            _commandQueue.Enqueue(() => { MoveForward(distance); });
            return;
        }

        if (ObjectPrivate.IsMovable)
        {
            Vector translation = new Vector(0.0f, distance, 0.0f);

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

    void MoveBackward(float distance)
    {
        MoveForward(-distance);
    }

    void TurnLeft(float degrees)
    {
        if (_queueMethods)
        {
            _commandQueue.Enqueue(() => { TurnLeft(degrees); });
            return;
        }

        if (ObjectPrivate.IsMovable)
        {
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

    void TurnRight(float degrees)
    {
        TurnLeft(-degrees);
    }

    void OnChat(ChatData data)
    {
        string[] msg = data.Message.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (msg[0].StartsWith("/"))
            msg[0] = msg[0].Substring(1);

        if (msg.Length == 1)
        {
            if (_noArgCommands.ContainsKey(msg[0]))
                _noArgCommands[msg[0]]();
            else if (ShowHelp && (msg[0] == "help"))
            {
                string helpMessage = "LogoMover includes the following commands:\n";

                foreach (var noArgCmd in _noArgCommands.Keys)
                    helpMessage += noArgCmd + "\n";
                foreach (var oneArgCmd in _floatCommands.Keys)
                    helpMessage += oneArgCmd + " <value>\n";

                helpMessage += "\nExamples:\n";
                helpMessage += "fd 1 -- move forward 1\n";
                helpMessage += "rt 90 -- turn right 90 degrees\n";

                try { ScenePrivate.FindAgent(data.SourceId).SendChat(helpMessage); } catch { }
            }
        }
        else if ((msg.Length > 1) && _floatCommands.ContainsKey(msg[0]))
        {
            float scalar = 0.0f;
            if (float.TryParse(msg[1], out scalar))
            {
                _floatCommands[msg[0]](scalar);
            }
        }
    }
}