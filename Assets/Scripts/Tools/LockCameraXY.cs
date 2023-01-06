using UnityEngine;
using Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Z co-ordinate
/// </summary>
[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class LockCameraXY : CinemachineExtension
{
    [Tooltip("Lock the camera's X or Y position to this value")]
    public Transform lockTarget;
    public enum LockType { lock_Y, lock_X };
    public LockType lockAxis;

    public Vector2 offset;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var pos = state.RawPosition;

            if(lockAxis == LockType.lock_X) { pos.x = (lockTarget != null ? lockTarget.position.x : 0) + offset.x; }
            else if (lockAxis == LockType.lock_Y) { pos.y = (lockTarget != null ? lockTarget.position.y : 0) + offset.y; }

            state.RawPosition = pos;
        }
    }
}
