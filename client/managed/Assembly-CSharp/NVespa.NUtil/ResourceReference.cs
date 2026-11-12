using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class ResourceReference : MonoBehaviour, IInspectorGuiDrawer
{
	[HideInInspector]
	public List<Object> ResourceList = new List<Object>();

	public void AtlasTouch()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ResourceList); i++)
		{
			UIAtlas atlas = GetAtlas(ResourceList[i]);
			if (!(atlas == null))
			{
				Texture texture = atlas.texture;
				if (!(texture == null))
				{
					Debug.Log($"Atlas Touch: width={texture.width}, height={texture.height}");
				}
			}
		}
	}

	public UIAtlas GetAtlas(Object obj)
	{
		if (obj == null)
		{
			return null;
		}
		GameObject gameObject = obj as GameObject;
		if (gameObject != null)
		{
			return gameObject.GetComponent<UIAtlas>();
		}
		return obj as UIAtlas;
	}
}
