// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

public class FollowArrow : SceneObjectScript
{
    // Public properties

    [DefaultValue("<0,1,0>")]
    public Vector WorldObjectForward;

    [DefaultValue(1.0f)]
    public readonly float FollowDistance;

    [DefaultValue(0.1f)]
    public readonly float TickTime;

    [DefaultValue(1.0f)]
    public readonly float MoveSpeed;

    [DefaultValue(true)]
    public readonly bool FixHeight;

    [DefaultValue("<0,0,0.8>")]
    public Vector FollowObjectOffset;

    // Private properties

    AgentPrivate _followAgent = null;

    // Logic!

    public override void Init()
    {
        // Write an error to the debug console if the object is not set to movable
        if (!ObjectPrivate.IsMovable)
        {
            Log.Write($"FollowArrow script can't move {ObjectPrivate.Name} because the 'Movable from Script' flag was not set!");
            return;
        }

        ScenePrivate.User.Subscribe(User.AddUser, (UserData data) =>
        {
            _followAgent = ScenePrivate.FindAgent(data.User);
        });

        ScenePrivate.User.Subscribe(User.RemoveUser, (UserData data) =>
        {
            if ((_followAgent != null) && (_followAgent.AgentInfo.SessionId == data.User))
                _followAgent = null;

            if (_followAgent == null)
                _followAgent = ScenePrivate.GetAgents().FirstOrDefault();
        });

        StartCoroutine(UpdateFollow);
    }

    void UpdateFollow()
    {
        TimeSpan ts = TimeSpan.FromSeconds(TickTime);

        while (true)
        {
            Wait(ts);

            if (_followAgent == null)
                continue;

            Vector agentPos = ScenePrivate.FindObject(_followAgent.AgentInfo.ObjectId).Position + FollowObjectOffset;
            Vector objectPos = ObjectPrivate.Position;

            if (FixHeight)
                agentPos.Z = objectPos.Z;

            Vector toAgent = agentPos - objectPos;
            Vector toAgentDir = toAgent.Normalized();

            // Compute if a movement update would move us closer than the follow distance (or beyond the target)
            Vector finalPos = objectPos + toAgentDir * MoveSpeed * TickTime;
            Vector toAgentFromFinalPos = agentPos - finalPos;
            bool wouldOvershoot = (toAgentFromFinalPos.Dot(toAgentDir) < FollowDistance);

            Quaternion rotation = Quaternion.ShortestRotation(WorldObjectForward, toAgentDir);

            // Just rotate to point to the follow target instead of overshooting
            if (wouldOvershoot)
            {
                ObjectPrivate.Mover.AddRotate(rotation, TickTime, MoveMode.Linear);
            }
            else
            {
                ObjectPrivate.Mover.AddMove(finalPos, rotation, TickTime, MoveMode.Linear);
            }
        }
    }
}
