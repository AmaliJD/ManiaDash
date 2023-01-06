using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Retroness;

namespace Retroness
{
	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	public class CameraTransform : MonoBehaviour
	{
		public Vector2 Position;
		public Vector3 Rotation;
		public Vector2 Scale = Vector2.one;
		public Vector2 Skew;
		public bool SafeCheck;
		public bool Override;
		public Matrix4x4 MatrixOverride;
		Camera _cam;
		Camera Cam
		{
			get
			{
				if (_cam == null)
					_cam = GetComponent<Camera>();
				return _cam;
			}
		}
		[ContextMenu("Reset override")]
		public void SetOverrideToNow()
		{
			MatrixOverride = Cam.projectionMatrix;
		}
		void Update()
		{
			TransformCamera();
		}
		void TransformCamera()
		{
			Cam.ResetWorldToCameraMatrix();
			Cam.ResetProjectionMatrix();

			if (Override)
			{
				Cam.projectionMatrix = MatrixOverride;
				return;
			}



			Matrix4x4 FinalMatrix = Cam.projectionMatrix;
			if (Scale.x == 0)
				Scale.x = .001f;
			if (Scale.y == 0)
				Scale.y = .001f;
			Matrix4x4 MultiMatrix;

			MultiMatrix = Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), new Vector3(Scale.x, Scale.y, 1));

			MultiMatrix.m01 += Skew.x / 45;
			MultiMatrix.m10 += Skew.y / 45;


			FinalMatrix = FinalMatrix * MultiMatrix;
			if (SafeCheck && FinalMatrix.m10/*.Positive()*/ == .2f)
			{
				FinalMatrix.m10 = FinalMatrix.m10/*.Sign()*/ * .20001f;
			}

			Cam.projectionMatrix = FinalMatrix;
		}
	}
}