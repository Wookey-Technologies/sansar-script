// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class AnimationInspector : SceneObjectScript
{
    [DefaultValue(true)]
    public readonly bool Enabled;

    AnimationComponent _animComp = null;
    int _totalAnimations = 0;
    int _currentAnimationIndex = -1;

    public override void Init()
    {
        if (!Enabled)
            return;

        if (ObjectPrivate.TryGetFirstComponent(out _animComp))
        {
            int animIndex = 0;

            foreach (var anim in _animComp.GetAnimations())
            {
                int frameCount = anim.GetFrameCount();
                string name = anim.GetName();

                Log.Write($"Object {ObjectPrivate.Name}: animation {animIndex} name '{name}' with {frameCount} frames");

                animIndex += 1;
            }

            _totalAnimations = animIndex;

            Log.Write($"Object {ObjectPrivate.Name}: has {_totalAnimations} animations");

            ListenForCommands();
        }
        else
        {
            Log.Write($"Object {ObjectPrivate.Name}: no animation");
        }
    }

    void ListenForCommands()
    {
        // Find each user as they enter the scene and listen for commands from them
        ScenePrivate.User.Subscribe(User.AddUser, (UserData ud) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(ud.User);
            if (agent != null)
                ListenForCommand(agent);
        });

        // Listen for commands from any users already in the scene
        foreach (var agent in ScenePrivate.GetAgents())
            ListenForCommand(agent);
    }

    void ListenForCommand(AgentPrivate agent)
    {
        agent.Client.SubscribeToCommand("PrimaryAction", CommandAction.Pressed, (CommandData command) =>
        {
            _currentAnimationIndex = (_currentAnimationIndex + 1) % _totalAnimations;
            PlayAnimation(_currentAnimationIndex);
        },
        (canceledData) => { });

        agent.Client.SubscribeToCommand("SecondaryAction", CommandAction.Pressed, (CommandData command) =>
        {
            _currentAnimationIndex = (_currentAnimationIndex + _totalAnimations - 1) % _totalAnimations;
            PlayAnimation(_currentAnimationIndex);
        },
        (canceledData) => { });
    }

    void PlayAnimation(int animIndex)
    {
        int index = 0;

        foreach (var anim in _animComp.GetAnimations())
        {
            if (index == animIndex)
            {
                int frameCount = anim.GetFrameCount();
                string name = anim.GetName();

                ScenePrivate.Chat.MessageAllUsers($"Playing object {ObjectPrivate.Name}: animation {animIndex} name '{name}' with {frameCount} frames");
                Log.Write($"Playing object {ObjectPrivate.Name}: animation {animIndex} name '{name}' with {frameCount} frames");

                AnimationParameters animParams = anim.GetParameters();
                animParams.PlaybackMode = AnimationPlaybackMode.PlayOnce;

                anim.Play(animParams);

                break;
            }
            else
            {
                index += 1;
            }
        }
    }
}
