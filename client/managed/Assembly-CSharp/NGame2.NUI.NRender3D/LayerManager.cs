using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NRender3D;

public class LayerManager : SingletonAuto<LayerManager>
{
	public enum ELayerType
	{
		Hero3DPreview,
		Hero3DPreview2,
		Hero3DPreview3,
		Hero3DPreview4,
		Hero3DPreview5
	}

	private List<int> _allLayerList = new List<int>();

	private List<int> _unusedList = new List<int>();

	public LayerManager()
	{
		EnumLookup<ELayerType>.ValueData[] valueArray = EnumLookup<ELayerType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			int item = LayerMask.NameToLayer(valueArray[i].Name);
			_allLayerList.Add(item);
			_unusedList.Add(LayerMask.NameToLayer(valueArray[i].Name));
		}
	}

	public int LockLayer()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_unusedList))
		{
			return _allLayerList[0];
		}
		int result = _unusedList[0];
		_unusedList.RemoveAt(0);
		return result;
	}

	public void ReleaseLayer(int layer)
	{
		if (_allLayerList.Contains(layer))
		{
			_unusedList.Add(layer);
		}
	}

	public void OnSceneChanged()
	{
		_unusedList.Clear();
		_unusedList.AddRange(_allLayerList);
	}
}
