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
using System;

namespace ScriptLibrary
{
    [Tooltip("Add this anywhere to see all simple script activity in the debug console. (CTRL+D)")]
    [DisplayName(nameof(Debugger))]

    // This class is used to debug simple scripts
    public class Debugger : SceneObjectBase, IDebug
    {
        #region EditorProperties
        [Tooltip("If true SimpleScripts all simple scripts in the scene will log the events they receive and send.")]
        [DefaultValue(true)]
        [DisplayName("Debug Mode")]
        public readonly bool _DebugSimple = true;
        #endregion

        public bool DebugSimple { get { return _DebugSimple; } }

        protected override Context ReflectiveContexts => Context.ScenePrivate | Context.ScenePublic;
        protected override string ReflectiveName => "Simple.Debugger";

        protected override void SimpleInit()
        {
        }
    }
}