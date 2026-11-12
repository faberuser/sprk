using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class BattleFieldMapProxy : IBattleFieldMap2
{
	private bool _isClosed;

	private Transform _parentTm;

	private RenderGroupModifier2 _renderGroupModifier;

	private ILocalTimer _localTimer;

	private Camera _cam;

	public void ApplyToReal(IBattleFieldMap2 realMap)
	{
		if (!_isClosed)
		{
			if (_parentTm != null && _cam != null)
			{
				realMap.SetParent(_parentTm, _cam);
			}
			if (_renderGroupModifier != null)
			{
				RenderGroupModifier2 renderGroupModifier = realMap.GetRenderGroupModifier();
				_renderGroupModifier.CopyTo(renderGroupModifier);
			}
			if (_localTimer != null && _cam != null)
			{
				realMap.ChainUpdate(_localTimer, _cam);
			}
		}
	}

	void IBattleFieldMap2.Close()
	{
		_isClosed = true;
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
		_localTimer = null;
		_cam = null;
		_parentTm = null;
	}

	void IBattleFieldMap2.SetParent(Transform parentTm, Camera cam)
	{
		_parentTm = parentTm;
		_cam = cam;
	}

	RenderGroupModifier2 IBattleFieldMap2.GetRenderGroupModifier()
	{
		if (_renderGroupModifier == null)
		{
			_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
			_renderGroupModifier.Open(useSoloTimeColor: true, isBattleField: true);
		}
		return _renderGroupModifier;
	}

	void IBattleFieldMap2.ChainUpdate(ILocalTimer localTimer, Camera cam)
	{
		_localTimer = localTimer;
		_cam = cam;
	}

	void IBattleFieldMap2.OnEvent(EBattleFieldEventType eventType, int eventValue)
	{
	}
}
