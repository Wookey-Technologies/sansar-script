/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using System;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class StarWarsVoiceover : SceneObjectScript
{
    // PUBLIC MEMBERS ------

    public SoundResource Sound;

    [Range(0.0, 60.0)]
    [DefaultValue(48.0)]
    public double Loudness_dB;


    // PRIVATE MEMBERS ------

    private float ZeroDBOffset = 48.0f;
    private RigidBodyComponent RigidBody;
    private float Relative_Loudness;
    static private Dictionary<SessionId, PlayHandle> playHandles = new Dictionary<SessionId, PlayHandle>();


    private void PlayOnAgent(AgentPrivate agent)
    {
        try
        {
            SessionId agentId = agent.AgentInfo.SessionId;
            Stop(agentId);

            PlaySettings playSettings = PlaySettings.PlayOnce;
            playSettings.Loudness = Relative_Loudness;
            if (agent.IsValid)
            {
                PlayHandle playHandle = agent.PlaySound(Sound, playSettings);
                if (playHandle != null)
                {
                    playHandles.Add(agentId, playHandle);
                    playHandle.OnFinished(() => Stop(agentId));
                }
            }
        }
        catch (Exception e)
        {
            Log.Write(LogLevel.Warning, "Voiceover", $"Exception {e.GetType().Name} in PlayOnAgent");
        }
    }

    private void Stop(SessionId id)
    {
        try
        {
            PlayHandle playing;
            if (playHandles.TryGetValue(id, out playing))
            {
                playing.Stop();
                playHandles.Remove(id);
            }
        }
        catch (Exception e)
        {
            Log.Write(LogLevel.Warning, "Voiceover", $"Exception {e.GetType().Name} while stopping a sound");
        }
    }

    private void OnCollide(CollisionData data)
    {
        if (data.Phase == Sansar.Simulation.CollisionEventPhase.TriggerEnter)
        {
            try
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                if (agent != null && agent.IsValid) PlayOnAgent(agent);
            }
            catch (Exception e)
            {
                // User left with _really_ bad timing, don't worry about it.
                Log.Write(LogLevel.Warning, "Voiceover", $"Exception {e.GetType().Name} in OnCollide");
            }
        }
    }

    private void UserLeft(UserData data)
    {
        Stop(data.User);
    }

    public override void Init()
    {
        ScenePrivate.User.Subscribe(User.RemoveUser, UserLeft);
        // Collision events are related to the RigidBody component so we must find it.
        // See if this object has a RigidBodyComponent and grab the first one.
        if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
        {
            Relative_Loudness = (float)Loudness_dB - ZeroDBOffset;

            if (RigidBody.IsTriggerVolume())
            {
                // Subscribe to TriggerVolume collisions on our rigid body: our callback (OnCollide) will get called
                // whenever a character or character vr hand collides with our trigger volume
                RigidBody.Subscribe(CollisionEventType.Trigger, OnCollide);
            }
        }
        else
        {
            // This will write to the region's log files. Only helpful to Linden developers.
            Log.Write("Couldn't find rigid body!");
        }
    }
}
