/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Interface for Quest Characters.")]
    [DisplayName("Quest Character")]
    public class QuestCharacter : SceneObjectScript
    {
        public Sansar.Simulation.QuestCharacter Character;
        public Sansar.Simulation.Interaction Interaction;

        public override void Init()
        {
            if (Character == null)
            {
                Log.Write(LogLevel.Error, "Quest Character on object '" + ObjectPrivate.Name + "' must have a character set.");
                return;
            }

            Interaction.Subscribe((InteractionData data) =>
            {
                Interact(data.AgentId);
            });
        }

        void Interact(SessionId sessionId)
        {
            Character.TurnInQuests(sessionId);
        }
    }
}
