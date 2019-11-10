// throw this script into the system camera object to gain basic control

using Sansar.Simulation;
using Sansar.Script;
using System;

namespace ScriptLibrary
{
    [DisplayName("CameraController")]

    public class CameraController : LibraryBase
    {
        [Tooltip("Activate remote camera")]
        [DefaultValue("activate")]
        [DisplayName("-> Make camera active")]
        public readonly string activateCameraComponent;

        [Tooltip("Reset remote camera, return to default")]
        [DefaultValue("main_cam")]
        [DisplayName("-> Back to main cam")]
        public readonly string resetCameraComponent;

        [Tooltip("Switch back after x seconds, 0 for permanent switch")]
        [DefaultValue(4.5f)]
        [DisplayName("Switch back after (secs)")]
        public readonly float Duration;

        private CameraComponent CamComponent;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<CameraComponent>(out CamComponent))
            {
                Log.Write("No camera component found!");
                return;
            }

            Log.Write("Found camera component");

            if (!string.IsNullOrWhiteSpace(activateCameraComponent))
            {
                SubscribeToAll(activateCameraComponent, (data) =>
                {
                    ISimpleData idata = data.Data.AsInterface<ISimpleData>();
                    AgentPrivate agent = ScenePrivate.FindAgent(idata.ObjectId);

                    if (agent != null)
                    {
                        agent.Client.SetActiveCamera(CamComponent);
                        if (Duration > 0)
                        {
                           Wait(TimeSpan.FromSeconds(Duration));
                           agent.Client.ResetCamera();
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(resetCameraComponent))
            {
                SubscribeToAll(resetCameraComponent, (data) =>
                {
                    ISimpleData idata = data.Data.AsInterface<ISimpleData>();
                    AgentPrivate agent = ScenePrivate.FindAgent(idata.ObjectId);

                    if (agent != null)
                        agent.Client.ResetCamera();
                });
            }
        }
    }
}
