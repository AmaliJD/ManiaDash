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
    
    [HideInInspector] public float lerpAxisY = 0, lerpAxisX = 0, lerpAxisXY = 0;

    protected void OnValidate()
    {
        lerpAxisXY = lockAxis == LockType.lock_XY ? 1 : 0;
        lerpAxisX = lockAxis == LockType.lock_X ? 1 : 0;
        lerpAxisY = lockAxis == LockType.lock_Y ? 1 : 0;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;
            float halfHeight = state.Lens.OrthographicSize;
            float halfWidth = halfHeight * state.Lens.Aspect;

            Vector3 RawPos = pos;

            if (lockTarget != null)
            {
                Vector3 posLockX = new Vector3(lockTarget.position.x + offset.x, pos.y, pos.z);
                Vector3 posLockY = new Vector3(pos.x, lockTarget.position.y + offset.y, pos.z);
                Vector3 posLockXY = new Vector3(lockTarget.position.x + offset.x, lockTarget.position.y + offset.y, pos.z);

                /*if (lockAxis == LockType.lock_X) { pos.x = lockTarget.position.x + offset.x; }
                else if (lockAxis == LockType.lock_Y) { pos.y = lockTarget.position.y + offset.y; }
                else if (lockAxis == LockType.lock_XY)
                {
                    pos.x = lockTarget.position.x + offset.x;
                    pos.y = lockTarget.position.y + offset.y;
                }*/

                float totalLerpAxis = lerpAxisXY + lerpAxisX + lerpAxisY;
                pos = ((lerpAxisY / totalLerpAxis) * posLockY) + ((lerpAxisX / totalLerpAxis) * posLockX) + ((lerpAxisXY / totalLerpAxis) * posLockXY);

                pos = Vector3.LerpUnclamped(RawPos, pos, lerpLock);
                RawPos = pos;
            }

            float distX = halfWidth;
            float distY = halfHeight;
            float clampedRotation = (state.Lens.Dutch + 720) % 360;

            if (clampedRotation <= 90)
            {
                distX = remap(0, 90, halfWidth, halfHeight, clampedRotation);
                distY = remap(0, 90, halfHeight, halfWidth, clampedRotation);
            }
            else if (clampedRotation <= 180)
            {
                distX = remap(90, 180, halfHeight, halfWidth, clampedRotation);
                distY = remap(90, 180, halfWidth, halfHeight, clampedRotation);
            }
            else if (clampedRotation <= 270)
            {
                distX = remap(180, 270, halfWidth, halfHeight, clampedRotation);
                distY = remap(180, 270, halfHeight, halfWidth, clampedRotation);
            }
            else if (clampedRotation <= 360)
            {
                distX = remap(270, 360, halfHeight, halfWidth, clampedRotation);
                distY = remap(270, 360, halfWidth, halfHeight, clampedRotation);
            }

            if (leftWall != null)
            {
                pos.x = pos.x <= leftWall.position.x + distX ? leftWall.position.x + distX : pos.x;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpLeft);
                RawPos = pos;
            }
            if (rightWall != null)
            {
                pos.x = pos.x >= rightWall.position.x - distX ? rightWall.position.x - distX : pos.x;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpRight);
                RawPos = pos;
            }
            if (topWall != null)
            {
                pos.y = pos.y >= topWall.position.y - distY ? topWall.position.y - distY : pos.y;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpTop);
                RawPos = pos;
            }
            if (bottomWall != null)
            {
                pos.y = pos.y <= bottomWall.position.y + distY ? bottomWall.position.y + distY : pos.y;
                pos = Vector3.LerpUnclamped(RawPos, pos, lerpBottom);
                RawPos = pos;
            }

            state.RawPosition = pos;
        }
    }

    public static float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
    {
        float rel = Mathf.InverseLerp(origFrom, origTo, value);
        return Mathf.Lerp(targetFrom, targetTo, rel);
    }

    public Vector3 GetCamPosition(string type)
    {
        float camRotation = -Camera.main.transform.eulerAngles.z;
        float halfWidth = Camera.main.pixelWidth / 2;
        float halfHeight = Camera.main.pixelHeight / 2;
        Vector3 center = new Vector3(halfWidth, halfHeight, Camera.main.nearClipPlane);

        float distX = halfWidth;
        float distY = halfHeight;
        float clampedRotation = (camRotation + 720) % 360;

        if (clampedRotation <= 90)
        {
            distX = remap(0, 90, halfWidth, halfHeight, clampedRotation);
            distY = remap(0, 90, halfHeight, halfWidth, clampedRotation);
        }
        else if (clampedRotation <= 180)
        {
            distX = remap(90, 180, halfHeight, halfWidth, clampedRotation);
            distY = remap(90, 180, halfWidth, halfHeight, clampedRotation);
        }
        else if (clampedRotation <= 270)
        {
            distX = remap(180, 270, halfWidth, halfHeight, clampedRotation);
            distY = remap(180, 270, halfHeight, halfWidth, clampedRotation);
        }
        else if (clampedRotation <= 360)
        {
            distX = remap(270, 360, halfHeight, halfWidth, clampedRotation);
            distY = remap(270, 360, halfWidth, halfHeight, clampedRotation);
        }

        switch (type)
        {
            case "left":
                return Camera.main.ScreenToWorldPoint(new Vector3(-distX, 0, Camera.main.nearClipPlane).Rotate(camRotation) + center);

            case "right":
                return Camera.main.ScreenToWorldPoint(new Vector3(distX, 0, Camera.main.nearClipPlane).Rotate(camRotation) + center);

            case "top":
                return Camera.main.ScreenToWorldPoint(new Vector3(0, distY, Camera.main.nearClipPlane).Rotate(camRotation) + center);

            case "bottom":
                return Camera.main.ScreenToWorldPoint(new Vector3(0, -distY, Camera.main.nearClipPlane).Rotate(camRotation) + center);

            default:
                return Camera.main.ScreenToWorldPoint(center);
        }
    }
}
