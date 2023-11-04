// DrawColliders
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.Rendering.Universal;

public class DrawColliders : MonoBehaviour
{
	public Material material;

	[SerializeField]
	private Color solid;

	[SerializeField]
	private Color trigger;

	[SerializeField]
	private Color damage;

	[SerializeField]
	private Color special;

	[SerializeField]
	private Color ignore;

	[SerializeField]
	private Color selected;

	public string[] ignoreTags;

	public string[] ignoreTypes;

	private Color color;

	private List<Collider2D> colliders;

	private Plane[] planes;

	private Vector3 mousePos;
	private Camera mainCamera;

	[HideInInspector] public bool hideRenderers;
	Light2D[] lights;

	private void Awake()
	{
		planes = new Plane[6];
		color = Color.green;
		mainCamera = Camera.main;
		GetColliders();
	}

	private void OnEnable()
	{
		RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
	}

	private void OnDisable()
	{
		RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
	}

	private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		OnPostRender();
	}

	private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		OnPreRender();
	}

	private void OnPreRender()
	{
		GL.wireframe = false;
	}

	private void OnPostRender()
	{
		GL.wireframe = true;
		foreach (Collider2D collider in colliders)
		{
			GeometryUtility.CalculateFrustumPlanes(Camera.main, planes);
			planes[4].distance = 2000f;
			if (!(collider != null) || !collider.enabled || !GeometryUtility.TestPlanesAABB(planes, collider.bounds))
			{
				continue;
			}

			if (collider.isTrigger)
			{
				color = trigger;
			}
			else
			{
				color = solid;
			}

			if (collider.gameObject.layer == LayerMask.NameToLayer("Death"))
			{
				color = damage;
			}
			if (collider.usedByComposite)
			{
				color = ignore;
			}

			string[] array = ignoreTags;
			foreach (string text in array)
			{
				if (collider.tag == text)
				{
					color = ignore;
					break;
				}
			}
			array = ignoreTypes;
			foreach (string type in array)
			{
				if (collider.GetComponent(type) != null)
				{
					color = ignore;
					break;
				}
			}
			if ((collider.tag == "ToggleTrigger" && collider.GetComponent<ToggleTrigger>() != null && collider.GetComponent<ToggleTrigger>().activeCount > 0) || (collider.tag == "MoveTrigger" && collider.GetComponent<MoveTrigger>() != null && collider.GetComponent<MoveTrigger>().activate > 0))
			{
				color = special;
			}

			if (collider.tag == "Player")
			{
				color = Color.white;
			}

#if UNITY_EDITOR
			if (UnityEditor.Selection.Contains(collider.gameObject))
				color = selected;
#endif

			GL.PushMatrix();
			GL.MultMatrix(collider.transform.localToWorldMatrix);
			material.SetPass(0);

			if (collider is BoxCollider2D)
			{
				DrawBoxCollider((BoxCollider2D)collider);
			}
			else if (collider is CompositeCollider2D)
			{
				DrawCompositeCollider((CompositeCollider2D)collider);
			}
			/*else if (collider is TilemapCollider2D)
			{
				DrawTilemapCollider((TilemapCollider2D)collider);
			}*/
			else if (collider is CircleCollider2D)
			{
				DrawCircleCollider((CircleCollider2D)collider);
			}
			else if (collider is CapsuleCollider2D)
			{
				DrawCapsuleCollider((CapsuleCollider2D)collider);
			}
			else if (collider is PolygonCollider2D && !collider.name.Contains("Frame"))
			{
				DrawPolygonCollider((PolygonCollider2D)collider);
			}
			else if (collider is EdgeCollider2D)
			{
				DrawEdgeCollider((EdgeCollider2D)collider);
			}

			GL.PopMatrix();
		}
		GL.wireframe = false;
	}

	private void DrawBoxCollider(BoxCollider2D collider)
	{
		GL.Begin(7);
		GL.Color(color);

		Vector2 bottomLeft = new Vector2((0f - collider.size.x) / 2f, (0f - collider.size.y) / 2f) + collider.offset;
		Vector2 topLeft = new Vector2((0f - collider.size.x) / 2f, collider.size.y / 2f) + collider.offset;
		Vector2 topRight = new Vector2(collider.size.x / 2f, collider.size.y / 2f) + collider.offset;
		Vector2 bottomRight = new Vector2(collider.size.x / 2f, (0f - collider.size.y) / 2f) + collider.offset;

		GL.Vertex(bottomLeft);
		GL.Vertex(topLeft);
		GL.Vertex(topRight);
		GL.Vertex(bottomRight);
		GL.End();

		if(collider.edgeRadius > 0) { DrawBoxEdgeCollider(collider, bottomLeft, topLeft, topRight, bottomRight); }
	}

	private void DrawBoxEdgeCollider(BoxCollider2D collider, Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight)
	{
		GL.Begin(2);
		GL.Color(color);

		float edgeRadius = collider.edgeRadius;
		int num = 8;

		Vector2 vectorTopRight = topRight + Vector2.up * edgeRadius;
		Vector2 vectorTopLeft = topLeft + Vector2.up * edgeRadius;

		Vector2 vectorLeftUp = topLeft + Vector2.left * edgeRadius;
		Vector2 vectorLeftDown = bottomLeft + Vector2.left * edgeRadius;

		Vector2 vectorBottomLeft = bottomLeft + Vector2.down * edgeRadius;
		Vector2 vectorBottomRight = bottomRight + Vector2.down * edgeRadius;

		Vector2 vectorRightDown = bottomRight + Vector2.right * edgeRadius;
		Vector2 vectorRightUp = topRight + Vector2.right * edgeRadius;

		GL.Vertex(vectorTopRight);
		GL.Vertex(vectorTopLeft);
		for (int i = 0; i <= num; i++)
		{
			GL.Vertex((Vector2.up.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + topLeft);
		}

		GL.Vertex(vectorLeftUp);
		GL.Vertex(vectorLeftDown);
		for (int i = 0; i <= num; i++)
		{
			GL.Vertex((Vector2.left.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + bottomLeft);
		}

		GL.Vertex(vectorBottomLeft);
		GL.Vertex(vectorBottomRight);
		for (int i = 0; i <= num; i++)
		{
			GL.Vertex((Vector2.down.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + bottomRight);
		}

		GL.Vertex(vectorRightDown);
		GL.Vertex(vectorRightUp);
		for (int i = 0; i <= num; i++)
		{
			GL.Vertex((Vector2.right.Rotate(90f * ((float)i / (float)num)) * edgeRadius) + topRight);
		}

		GL.End();
	}

	private void DrawCircleCollider(CircleCollider2D collider)
	{
		GL.Begin(2);
		GL.Color(color);
		float radius = collider.radius;
		int num = 18;
		for (int i = 0; i < num; i++)
		{
			GL.Vertex(Vector2.right.Rotate(360f * ((float)i / (float)num)) * radius);
		}
		GL.Vertex(Vector2.right * radius);
		GL.End();
	}

	private void DrawCompositeCollider(CompositeCollider2D collider)
	{
		for (int i = 0; i < collider.pathCount; i++)
		{
			GL.Begin(2);
			GL.Color(color);
			Vector2[] array = new Vector2[collider.GetPathPointCount(i)];
			collider.GetPath(i, array);
			Vector2[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				GL.Vertex(array2[j]);
			}
			GL.Vertex(array[0]);
			GL.End();
		}
	}

	private void DrawTilemapCollider(TilemapCollider2D collider)
	{
		// 2021+ collider.setShapes()
		/*for (int i = 0; i < collider.bounds; i++)
		{
			GL.Begin(2);
			GL.Color(color);
			Vector2[] array = new Vector2[collider.GetPathPointCount(i)];
			collider.GetPath(i, array);
			Vector2[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				GL.Vertex(array2[j]);
			}
			GL.Vertex(array[0]);
			GL.End();
		}*/
	}

	private void DrawCapsuleCollider(CapsuleCollider2D collider)
	{
		GL.Begin(2);
		GL.Color(color);

		float radius = collider.direction == CapsuleDirection2D.Vertical ? collider.size.x / 2 : collider.size.y / 2;
		float difference = collider.direction == CapsuleDirection2D.Vertical ? (collider.size.y > collider.size.x ? (collider.size.y - collider.size.x) / 2 : 0)
							: (collider.size.x > collider.size.y ? (collider.size.x - collider.size.y) / 2 : 0);

		Vector2 right = collider.direction == CapsuleDirection2D.Vertical ? Vector2.right : Vector2.up;
		Vector2 up = collider.direction == CapsuleDirection2D.Vertical ? Vector2.up : Vector2.left;

		int num = 12;

		for (int i = 0; i < num; i++)
		{
			GL.Vertex((right.Rotate(180f * ((float)i / (float)num)) * radius) + (up * difference) + collider.offset);
		}
		GL.Vertex((-right * radius) + (up * difference) + collider.offset);

		for (int i = 0; i < num; i++)
		{
			GL.Vertex((right.Rotate(((180f * ((float)i / (float)num))) + 180f) * radius) + (-up * difference) + collider.offset);
		}
		GL.Vertex((right * radius) + (-up * difference) + collider.offset);
		GL.Vertex((right * radius) + (up * difference) + collider.offset);

		GL.End();
	}

	private void DrawPolygonCollider(PolygonCollider2D collider)
	{
		if (collider.points.Length != 3)
        {
			GL.Begin(2);
			GL.Color(color);
			foreach (Vector2 point in collider.points)
			{
				GL.Vertex(point);
			}
			GL.Vertex(collider.points[0]);
			GL.End();
		}
		else
        {
			GL.Begin(GL.TRIANGLES);
			GL.Color(color);
			foreach (Vector2 point in collider.points)
			{
				GL.Vertex(point);
			}
			GL.End();
		}
	}

	private void DrawEdgeCollider(EdgeCollider2D collider)
	{
		if (collider.points.Length != 3)
		{
			GL.Begin(2);
			GL.Color(color);
			foreach (Vector2 point in collider.points)
			{
				GL.Vertex(point);
			}
			GL.Vertex(collider.points[0]);
			GL.End();
		}
		else
		{
			GL.Begin(GL.TRIANGLES);
			GL.Color(color);
			foreach (Vector2 point in collider.points)
			{
				GL.Vertex(point);
			}
			GL.End();
		}
	}

	public void GetColliders()
	{
		colliders = Object.FindObjectsOfType<Collider2D>().ToList();
	}

	public void AddColliders(Collider2D c)
	{
		if (!colliders.Contains(c))
		{
			colliders.Add(c);
		}
	}

	public void RemoveColliders(Collider2D c)
	{
		if (colliders.Contains(c))
		{
			colliders.Remove(c);
		}
	}

	public void HideRenderers(bool hide)
	{
		/*SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
		foreach(SpriteRenderer sr in sprites)
        {
			sr.enabled = false;
        }

		TilemapRenderer[] maps = FindObjectsOfType<TilemapRenderer>();
		foreach (TilemapRenderer tmp in maps)
		{
			tmp.enabled = false;
		}

		ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
		foreach (ParticleSystem ps in particles)
		{
			ps.Stop();
		}

		TrailRenderer[] trails = FindObjectsOfType<TrailRenderer>();
		foreach (TrailRenderer tr in trails)
		{
			tr.enabled = false;
		}

		Light2D[] lights = FindObjectsOfType<Light2D>();
		foreach (Light2D l in lights)
		{
			l.enabled = false;
		}*/

		//mainCamera.enabled = !mainCamera.enabled;
		//StartCoroutine(UpdateHideRenderers());
		hideRenderers = hide;
		if (hide)
		{
			mainCamera.cullingMask = (1 << LayerMask.NameToLayer("Camera")) | (1 << LayerMask.NameToLayer("Lines"));
			foreach (ProximityEffector px in FindObjectsOfType<ProximityEffector>())
			{
				px.enabled = true;
			}
		}
		else
        {
			mainCamera.cullingMask = -1;
		}
	}

	/*public IEnumerator UpdateHideRenderers()
    {
		while(true)
        {
			HideRenderers();
			//yield return new WaitForSeconds(.2f);
			yield return null;
		}
    }*/
}
