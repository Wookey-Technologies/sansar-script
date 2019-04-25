using System;
using System.Linq;
using System.Collections.Generic;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class BroadcastUsers : SceneObjectScript
{

    public override void Init()
    {
        ScenePrivate.User.Subscribe(User.AddUser, OnUserJoin);
        ScenePrivate.User.Subscribe(User.RemoveUser, OnUserLeave);
    }

    private void OnUserJoin(UserData data)
    {
        ScenePrivate.VoiceBroadcastStart(ScenePrivate.FindAgent(data.User));
    }

    private void OnUserLeave(UserData data)
    {
        ScenePrivate.VoiceBroadcastStop(ScenePrivate.FindAgent(data.User));
    }
}
