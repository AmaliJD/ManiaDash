using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkewEffect : BaseMeshEffect
{
	public Vector2 PositionOffset;
	public Vector3 Position;
	public Vector3 Rotation;
	public Vector2 Scale = Vector2.one;
	public Vector2 Skew;

	List<UIVertex> verts = new List<UIVertex>();
	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
			return;
		if (Scale.x == 0)
			Scale.x = .001f;
		if (Scale.y == 0)
			Scale.y = .001f;
			
		Matrix4x4 MultiMatrix;
		MultiMatrix = Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), new Vector3(Scale.x, Scale.y, 1));
			
		MultiMatrix.m01 += Skew.x/45;
		MultiMatrix.m10 += Skew.y/45;
		
		vh.GetUIVertexStream(verts);
		for(int i = 0; i < verts.Count; ++i)
		{
			UIVertex pos = verts[i];
			//just need to find the factor to match whatever your animator set in skew
			pos.position = MultiMatrix.MultiplyVector(pos.position)+PositionOffset.SetZ(0);
			verts[i] = pos;
		}
		vh.Clear();
		vh.AddUIVertexTriangleStream(verts);
	}
}
