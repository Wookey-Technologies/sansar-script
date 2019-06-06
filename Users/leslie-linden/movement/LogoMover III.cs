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

    float _rotationSpeed;
    float _speed;


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

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    void Reset()
    {
        ObjectPrivate.Mover.StopAndClear();
        ObjectPrivate.Mover.AddMove(ObjectPrivate.InitialPosition, ObjectPrivate.InitialRotation);
    }

    void SetMoveSpeed(float speed)
    {
        _speed = speed;
    }

    void SetSpinSpeed(float speed)
    {
        _rotationSpeed = speed;
    }

    void MoveForward(float distance)
    {
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
            InterpretCommandWithArgs(msg);
        }
    }

    void InterpretCommandWithArgs(string[] commandWithArgs)
    {
        if (_floatCommands.ContainsKey(commandWithArgs[0]))
        {
            float scalar = 0.0f;
            if (float.TryParse(commandWithArgs[1], out scalar))
            {
                _floatCommands[commandWithArgs[0]](scalar);
            }

            if (commandWithArgs.Length > 2)
                InterpretCommandWithArgs(commandWithArgs.Skip(2).ToArray());
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
                    InterpretCommandWithArgs(newCommandWithArgs);
            }
        }
    }
}