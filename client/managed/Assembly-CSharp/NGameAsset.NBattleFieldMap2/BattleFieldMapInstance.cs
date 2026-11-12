using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class BattleFieldMapInstance : IBattleFieldMap2
{
	private RenderGroupModifier2 _renderGroupModifier;

	private ModelInstance _modelInstance;

	private List<EffectFxInstance> _effectFxList = new List<EffectFxInstance>();

	public IBattleFieldMapOpenData OpenData { get; private set; }

	public GameObject InstanceGo
	{
		get
		{
			if (_modelInstance != null)
			{
				return _modelInstance.InstanceGo;
			}
			return null;
		}
	}

	public bool Open(IBattleFieldMapOpenData data)
	{
		_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
		_renderGroupModifier.Open(useSoloTimeColor: true, isBattleField: true);
		OpenData = data;
		_modelInstance = new ModelInstance(this);
		if (!_modelInstance.Open(OpenData))
		{
			return false;
		}
		List<EffectFxData> effectFxList = data.EffectFxList;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(effectFxList); i++)
		{
			EffectFxData data2 = effectFxList[i];
			EffectFxInstance effectFxInstance = new EffectFxInstance(this);
			effectFxInstance.Open(data2);
			_effectFxList.Add(effectFxInstance);
		}
		return true;
	}

	public Transform GetNodeByPath(string nodePath)
	{
		if (_modelInstance != null)
		{
			Transform nodeByPath = _modelInstance.GetNodeByPath(nodePath);
			if (nodeByPath != null)
			{
				return nodeByPath;
			}
		}
		if (InstanceGo == null)
		{
			return null;
		}
		return InstanceGo.transform;
	}

	void IBattleFieldMap2.Close()
	{
		if (_modelInstance != null)
		{
			_modelInstance.Close();
			_modelInstance = null;
		}
		for (int i = 0; i < _effectFxList.Count; i++)
		{
			_effectFxList[i].Close();
		}
		_effectFxList.Clear();
	}

	void IBattleFieldMap2.SetParent(Transform parentTm, Camera cam)
	{
		if (_modelInstance != null)
		{
			_modelInstance.SetParent(parentTm, OpenData.Stock, cam);
		}
	}

	RenderGroupModifier2 IBattleFieldMap2.GetRenderGroupModifier()
	{
		return _renderGroupModifier;
	}

	void IBattleFieldMap2.ChainUpdate(ILocalTimer localTimer, Camera cam)
	{
		_modelInstance.ChainUpdate(cam);
		for (int i = 0; i < _effectFxList.Count; i++)
		{
			_effectFxList[i].ChainUpdate(cam);
		}
		_renderGroupModifier.ChainUpdate(localTimer, isBattleField: true);
		if (_renderGroupModifier.BeginFlush(forcePropertyBlockDirty: true))
		{
			_modelInstance.Visit(_renderGroupModifier);
			for (int j = 0; j < _effectFxList.Count; j++)
			{
				_effectFxList[j].Apply(_renderGroupModifier);
			}
			_renderGroupModifier.EndFlush();
		}
	}

	void IBattleFieldMap2.OnEvent(EBattleFieldEventType eventType, int eventValue)
	{
		for (int i = 0; i < _effectFxList.Count; i++)
		{
			_effectFxList[i].OnEvent(eventType, eventValue);
		}
	}
}
