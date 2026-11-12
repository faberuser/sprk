using System;
using NGame2.NGameRoot;
using UnityEngine;

namespace NGame2;

[Serializable]
public class LayerInfo
{
	public bool IsUI;

	public string LayerName;

	public int Layer;

	public Camera Camera;

	public UICamera UICamera;

	[NonSerialized]
	public bool IsContentsExist = true;

	private Transform _shakeTm;

	public bool IsValid()
	{
		if (String_Extension_Method.IsNullOrEmpty(LayerName))
		{
			return false;
		}
		if (Camera == null)
		{
			return false;
		}
		if (IsUI && UICamera == null)
		{
			return false;
		}
		return true;
	}

	public Transform GetShakeTransform()
	{
		if (_shakeTm != null)
		{
			return _shakeTm;
		}
		Transform parent = Camera.transform.parent;
		if (string.Equals(parent.name, "CameraShake"))
		{
			_shakeTm = parent;
			return _shakeTm;
		}
		GameObject gameObject = new GameObject("CameraShake");
		_shakeTm = gameObject.transform;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_shakeTm, parent);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(Camera.transform, _shakeTm);
		return _shakeTm;
	}

	public void Rebuild()
	{
		Transform parent = Camera.transform.parent;
		VespaObject.DestroyGo(Camera);
		LayerInfo layerInfo = LayerManagerBase.CreateLayerInfo(parent.gameObject, LayerName, IsUI);
		Camera = layerInfo.Camera;
		UICamera = layerInfo.UICamera;
	}
}
