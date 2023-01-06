using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Retroness
{
	[ExecuteAlways()]
	public class MeshSpriteRenderer : MonoBehaviour
	{
		public Renderer renderer;
		public bool IncludeChildOfRenderer;
		MaterialPropertyBlock propblock;
		[SerializeField] PropertySprite BaseProperty;
		public List<PropertySprite> Extras = new List<PropertySprite>();

		public Sprite sprite
		{
			get
			{
				return BaseProperty.sprite;
			}
			set
			{
				BaseProperty.sprite = value;
				CallDirty();
			}
		} 
		[ContextMenu("Force call dirty")]
		public void CallDirty()
		{
			if(propblock == null)
			{
				propblock = new MaterialPropertyBlock();
			}
			propblock.Clear();
			renderer.GetPropertyBlock(propblock);
			ApplySpriteOntoProp(propblock, BaseProperty);
			foreach (var item in Extras)
			{
				ApplySpriteOntoProp(propblock, item);
			}
			renderer.SetPropertyBlock(propblock);
			if(IncludeChildOfRenderer)
				foreach (var child in renderer.GetComponentsInChildren<Renderer>())
				{
					child.SetPropertyBlock(propblock);
				}
		}
		void ApplySpriteOntoProp(MaterialPropertyBlock block, PropertySprite prop)
		{
			if(prop.TargetTexture == null)
				return;
			propblock.SetTexture(prop.property, prop.TargetTexture);
			if(prop.UVType != PropertySprite.UVOverrideType.None)
				propblock.SetVector(prop.property + "_ST", prop.OffsetAndTile);
		}

		void Awake()
		{
			CallDirty();
		}
		void OnEnable()
		{
			CallDirty();
		}
		void OnValidate()
		{
			sprite = BaseProperty.sprite;
		}
		
	}
	[System.Serializable]
	public class PropertySprite
	{
		public string property = "_MainTex";
		public UVOverrideType UVType =	UVOverrideType.FromSprite;
		public Texture2D TargetTexture
		{
			get
			{
				if(UseTextureInstead)
					return texture;
				if(sprite == null)
					return Texture2D.whiteTexture;
				return sprite.texture;
			}
		}
		public Sprite sprite;
		public bool UseTextureInstead;
		public Texture2D texture;
		public Vector4 OverrideUV;
		public Vector4 OffsetAndTile
		{
			get
			{
				switch (UVType)
				{
					case UVOverrideType.FromSprite:
						return GetSpriteTile(sprite);
					case UVOverrideType.FromValue:
						return OverrideUV;
					default:
						return Vector4.zero;
				}
			}
		}
		public static Vector4 GetSpriteTile(Sprite s)
		{
			if(s == null)
				return Vector4.zero;
			Vector2 TextureSize = new Vector2(s.texture.width, s.texture.height);
			Rect rect = s.textureRect;
			return new Vector4(rect.width/TextureSize.x, rect.height/TextureSize.y, rect.x/TextureSize.x, rect.y/TextureSize.y);
		}
		public enum UVOverrideType
		{
			None,
			FromSprite,
			FromValue,
		}
	}
}
