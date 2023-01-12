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
    public enum LockType { lock_Y, lock_X, lock_XY };
    public LockType lockAxis;

    public Vector2 offset;

    public Transform leftWall, rightWall, topWall, bottomWall;
    [Range(0,1)] public float lerpLock = 1, lerpLeft = 1, lerpRight = 1, lerpTop = 1, lerpBottom = 1;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var pos = state.RawPosition;
            var halfHeight = state.Lens.OrthographicSize;
            var halfWidth = halfHeight * state.Lens.Aspect;

            var RawPos = pos;

            if (lockTarget != null)
            {
                if (lockAxis == LockType.lock_X) { pos.x = lockTarget.position.x + offset.x; }
                else if (lockAxis == LockType.lock_Y) { pos.y = lockTarget.position.y + offset.y; }
                else if (lockAxis == LockType.lock_XY)
                {
                    pos.x = lockTarget.position.x + offset.x;
                    pos.y = lockTarget.position.y + offset.y;
                }

                pos = Vector3.LerpUnclamped(RawPos, pos, lerpLock);
                RawPos = pos;
            }

            if(leftWall != null)
            {
                pos.x = pos.x <= leftWall.position.x + halfWidth ? leftWall.position.x + halfWidth : pos.x;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpLeft);
                RawPos = pos;
            }
            if (rightWall != null)
            {
                pos.x = pos.x >= rightWall.position.x - halfWidth ? rightWall.position.x - halfWidth : pos.x;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpRight);
                RawPos = pos;
            }
            if (topWall != null)
            {
                pos.y = pos.y >= topWall.position.y - halfHeight ? topWall.position.y - halfHeight : pos.y;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpTop);
                RawPos = pos;
            }
            if (bottomWall != null)
            {
                pos.y = pos.y <= bottomWall.position.y + halfHeight ? bottomWall.position.y + halfHeight : pos.y;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpBottom);
                RawPos = pos;
            }

            state.RawPosition = pos;
        }
    }
}
