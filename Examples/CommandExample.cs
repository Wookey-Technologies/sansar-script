/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;

public class CommandExample : SceneObjectScript
{
    #region ScriptParameters
    [Tooltip(@"The command to enable listening for the Action Command. Default: Confirm (Enter)")]
    [DefaultValue("Confirm")]
    [DisplayName("Subscribe Command")]
    public readonly string SubscribeCommand;

    [Tooltip(@"The command to disable listening for the Action Command. Default: Cancel (Escape)")]
    [DefaultValue("Cancel")]
    [DisplayName("Unsubscribe Command")]
    public readonly string UnsubscribeCommand;

    [Tooltip(@"If the command has been subscribed to by the 'Subscribe Command', this will log the action to the script console.")]
    [DefaultValue("Trigger")]
    [DisplayName("Action Command")]
    public readonly string ActionCommand;
    #endregion ScriptParameters

    public override void Init()
    {
    // Subscribe to new user events;
    ScenePrivate.User.Subscribe(User.AddUser, NewUser);
    }

    IEventSubscription subscription = null;

    void NewUser(UserData newUser)
    {
        Client client = ScenePrivate.FindAgent(newUser.User).Client;

        // CommandReceived will be called every time the command it triggered on the client
        // CommandCanceled will be called if the subscription fails
        if (SubscribeCommand != "")
        {
            client.SubscribeToCommand(SubscribeCommand, CommandAction.Pressed, (data) =>
            {
                if (subscription == null)
                {
                    Log.Write(GetType().Name, $"[{SubscribeCommand}] Subscribing to {ActionCommand}.");
                    subscription = client.SubscribeToCommand(ActionCommand, CommandAction.All, CommandReceived, CommandCanceled);
                }
            }, CommandCanceled);
        }

        if (UnsubscribeCommand != "")
        {
            client.SubscribeToCommand(UnsubscribeCommand, CommandAction.Pressed, (data) =>
            {
                if (subscription != null)
                {
                    Log.Write(GetType().Name, $"[{UnsubscribeCommand}] Unsubscribing to {ActionCommand}.");
                    subscription.Unsubscribe();
                    subscription = null;
                }
            }, CommandCanceled);
        }
    }

    void CommandReceived(CommandData command)
    {
        Log.Write(GetType().Name, $"Received command {command.Command}: {command.Action}. Targeting Info: {command.TargetingComponent}, Origin:{command.TargetingOrigin}, Position:{command.TargetingPosition}, Normal{command.TargetingNormal}");
    }

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: "+data.Message);
    }

}