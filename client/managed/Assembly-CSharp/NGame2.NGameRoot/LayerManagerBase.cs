using System.Collections.Generic;
using NGame2.N3D;
using NVespa;
using UnityEngine;

namespace NGame2.NGameRoot;

public abstract class LayerManagerBase
{
	public static LayerInfo CreateLayerInfo(GameObject parentGo, string layerName, bool isUiLayer)
	{
		LayerInfo layerInfo = new LayerInfo();
		layerInfo.LayerName = layerName;
		layerInfo.Layer = LayerMask.NameToLayer(layerName);
		layerInfo.IsUI = isUiLayer;
		List<Camera> list = new List<Camera>(NGUITools.FindActive<Camera>());
		int num = 1 << layerInfo.Layer;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			Camera camera = list[i];
			if (GlobalCameraManager.HasMainRenderTarget(camera))
			{
				camera.cullingMask &= ~num;
			}
		}
		layerInfo.Camera = NGUITools.AddChild<Camera>(parentGo, undo: false);
		if (isUiLayer)
		{
			layerInfo.UICamera = layerInfo.Camera.gameObject.AddComponent<UICamera>();
		}
		layerInfo.Camera.gameObject.name = layerName + " Camera";
		layerInfo.Camera.gameObject.layer = layerInfo.Layer;
		layerInfo.Camera.clearFlags = CameraClearFlags.Depth;
		layerInfo.Camera.backgroundColor = new Color32(29, 46, 73, byte.MaxValue);
		layerInfo.Camera.cullingMask = num;
		layerInfo.Camera.depth = 0f;
		if (isUiLayer)
		{
			layerInfo.Camera.orthographic = true;
			layerInfo.Camera.orthographicSize = 1f;
			layerInfo.Camera.nearClipPlane = -10f;
			layerInfo.Camera.farClipPlane = 10f;
		}
		else
		{
			layerInfo.Camera.fieldOfView = CameraStat.Basic.FieldOfView;
			layerInfo.Camera.nearClipPlane = 0.1f;
			layerInfo.Camera.farClipPlane = 100f;
		}
		AudioListener[] array = NGUITools.FindActive<AudioListener>();
		__Extension_Method_ICollection__.COUNT(array);
		_ = 1;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			layerInfo.Camera.gameObject.AddComponent<AudioListener>();
		}
		return layerInfo;
	}
}
