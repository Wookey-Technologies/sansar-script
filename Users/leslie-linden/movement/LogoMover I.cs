// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;

public class LogoMover : SceneObjectScript
{
    [DefaultValue(1.0f)]
    public readonly float MoveScale;

    [DefaultValue(5.0f)]
    public readonly float DefaultSpeed;

    [DefaultValue(180.0f)]
    public readonly float RotationSpeed;


    Dictionary<string, Action<float>> _commands = new Dictionary<string, Action<float>>();

    float _rotationSpeed;
    float _speed;


    public override void Init()
    {
        //_commands["help"] = PrintHelp;
        _commands["fd"] = MoveForward;
        _commands["bk"] = MoveBackward;

        _commands["rt"] = TurnRight;
        _commands["lt"] = TurnLeft;

        _rotationSpeed = RotationSpeed;
        _speed = DefaultSpeed;

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    void MoveForward(float distance)
    {
        if (ObjectPrivate.IsMovable && (_speed != 0.0f))
        {
            double moveTime = Math.Abs(distance * MoveScale / _speed);

            ObjectPrivate.Mover.AddTranslateOffset(new Vector(0.0f, distance, 0.0f), moveTime, MoveMode.Smoothstep);
        }
    }

    void MoveBackward(float distance)
    {
        MoveForward(-distance);
    }

    void TurnLeft(float degrees)
    {
        if (ObjectPrivate.IsMovable && (_rotationSpeed != 0.0f))
        {
            double moveTime = Math.Abs(degrees / _rotationSpeed);

            var rotation = Quaternion.FromEulerAngles(new Vector(0.0f, 0.0f, degrees * Mathf.RadiansPerDegree));
            ObjectPrivate.Mover.AddRotateOffset(rotation, moveTime, MoveMode.Smoothstep);
        }
    }

    void TurnRight(float degrees)
    {
        TurnLeft(-degrees);
    }

    void OnChat(ChatData data)
    {
        string[] msg = data.Message.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (msg.Length > 1)
        {
            if (_commands.ContainsKey(msg[0]))
            {
                float arg = 0.0f;
                if (float.TryParse(msg[1], out arg))
                {
                    _commands[msg[0]](arg);
                }
            }
        }
    }
}