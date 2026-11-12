using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NGameRoot;

[Serializable]
public abstract class GenericLayerManager<TLayerEnum> : LayerManagerBase where TLayerEnum : struct
{
	[HideInInspector]
	[SerializeField]
	private List<LayerInfo> _layerList = new List<LayerInfo>();

	private IncreaseDic<TLayerEnum, LayerInfo> _dicInternal;

	public int Count => __Extension_Method_ICollection__.COUNT(_layerList);

	public LayerInfo this[int index] => Vespa_Generic_IList_Extension_Method.GetValid(_layerList, index);

	private IncreaseDic<TLayerEnum, LayerInfo> _dic
	{
		get
		{
			if (_dicInternal == null)
			{
				_dicInternal = new IncreaseDic<TLayerEnum, LayerInfo>();
				for (int i = 0; i < _layerList.Count; i++)
				{
					LayerInfo layerInfo = _layerList[i];
					EnumLookup<TLayerEnum>.ValueData byName = EnumLookup<TLayerEnum>.GetByName(layerInfo.LayerName);
					_dicInternal.Add(byName.EnumValue, layerInfo);
				}
			}
			return _dicInternal;
		}
	}

	public static GameLayerAttribute GetLayerAttribute(TLayerEnum layerType)
	{
		object[] customAttributes = layerType.GetType().GetMember(EnumTools.GetName(layerType))[0].GetCustomAttributes(typeof(GameLayerAttribute), inherit: false);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(customAttributes))
		{
			return null;
		}
		return customAttributes[0] as GameLayerAttribute;
	}

	public static int GetLayerDepth(TLayerEnum layerType)
	{
		return GetLayerAttribute(layerType)?.Depth ?? 0;
	}

	public static bool IsUiLayer(TLayerEnum layerType)
	{
		return GetLayerAttribute(layerType)?.IsUI ?? false;
	}

	public LayerInfo GetLayerInfo(int layer)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_layerList); i++)
		{
			LayerInfo layerInfo = _layerList[i];
			if (layerInfo.Layer == layer)
			{
				return layerInfo;
			}
		}
		return null;
	}

	public LayerInfo GetLayerInfo(GameObject parentGo, TLayerEnum layerType)
	{
		if (TryGetLayerInfo(layerType, out var resultLayerInfo))
		{
			return resultLayerInfo;
		}
		resultLayerInfo = LayerManagerBase.CreateLayerInfo(parentGo, EnumTools.GetName(layerType), IsUiLayer(layerType));
		_layerList.Add(resultLayerInfo);
		_dicInternal = null;
		AdjustCameraDepthAndClearFlags();
		return resultLayerInfo;
	}

	public bool TryGetLayerInfo(TLayerEnum layerType, out LayerInfo resultLayerInfo)
	{
		return _dic.TryGetValue(layerType, out resultLayerInfo);
	}

	public bool TryGetLayerInfo(GameObject go, out LayerInfo resultLayerInfo)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_layerList); i++)
		{
			LayerInfo layerInfo = _layerList[i];
			if (go.layer == layerInfo.Layer)
			{
				resultLayerInfo = layerInfo;
				return true;
			}
		}
		resultLayerInfo = null;
		return false;
	}

	public void RebuildLayer()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_layerList); i++)
		{
			_layerList[i].Rebuild();
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(UIPanel.list); j++)
		{
			UIPanel uIPanel = UIPanel.list[j];
			if (GetLayerInfo(uIPanel.gameObject.layer) != null)
			{
				uIPanel.ResetAnchors();
			}
		}
		AdjustCameraDepthAndClearFlags();
	}

	public void AdjustCameraDepthAndClearFlags()
	{
		EnumLookup<TLayerEnum>.ValueData[] valueArray = EnumLookup<TLayerEnum>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			TLayerEnum enumValue = valueArray[i].EnumValue;
			if (TryGetLayerInfo(enumValue, out var resultLayerInfo))
			{
				int layerDepth = GetLayerDepth(enumValue);
				resultLayerInfo.Camera.depth = layerDepth;
			}
		}
		UICamera.list.Sort(UICamera.CompareFunc);
		if (SystemStatus.IsPlayingCritical)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.AdjustCameraRenderAndClearFlags();
		}
	}
}
