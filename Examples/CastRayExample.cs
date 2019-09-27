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
using System.Collections.Generic;
using System.Linq;

namespace CastRayExample
{
    // Listens for "PrimaryAction" to be pressed for each user in the scene and casts a ray forward to check for collisions.
    // Collisions are sent to the user
    public class Script : SceneObjectScript
    {

        public override void Init()
        {
            ScenePrivate.User.Subscribe(User.AddUser, newUser);
        }

        private void reset(AgentPrivate ap, ObjectPrivate op)
        {
            ap.Client.SubscribeToCommand("PrimaryAction", CommandAction.Pressed, x => onAction(ap, op), x => { Log.Write("Command canceled"); }, false);
        }

        private void newUser(UserData data)
        {
            AgentPrivate agentPrivate = ScenePrivate.FindAgent(data.User);
            ObjectPrivate objectPrivate = ScenePrivate.FindObject(agentPrivate.AgentInfo.ObjectId);

            reset(agentPrivate, objectPrivate);
        }

        private void getSegment(AgentPrivate ap, ObjectPrivate op, out Vector start, out Vector end)
        {
            float distance = 10;
            Quaternion orientation;
            Vector offset;
            ControlPointType controlPoint = ControlPointType.GazeTarget;
            if (ap.GetControlPointEnabled(controlPoint))
            {
                offset = Vector.ObjectForward * distance;
                start = ap.GetControlPointPosition(controlPoint);
                orientation = ap.GetControlPointOrientation(controlPoint);
            }
            else
            {
                offset = Vector.ObjectLeft * distance;
                start = op.Position + Vector.ObjectUp * 0.5f;
                orientation = op.Rotation;
            }

            offset = offset.Rotate(orientation);
            end = start + offset;
        }

        private void cast(AgentPrivate ap, ObjectPrivate op)
        {
            Vector start;
            Vector end;
            getSegment(ap, op, out start, out end);

            var hitData = ScenePrivate.CastRay(start, end);

            ap.SendChat("Intersected " + hitData.Length);
            foreach (var hit in hitData)
            {
                ap.SendChat("Location: " + hit.Location);
                ap.SendChat("Normal: " + hit.Normal);
            }
        }
        private void onAction(AgentPrivate ap, ObjectPrivate op)
        {
            cast(ap, op);
            reset(ap, op);
        }
    }

}