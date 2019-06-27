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
        ScenePrivate.SetMegaphone(ScenePrivate.FindAgent(data.User),true);
    }

    private void OnUserLeave(UserData data)
    {
        ScenePrivate.SetMegaphone(ScenePrivate.FindAgent(data.User),false);
    }
}
