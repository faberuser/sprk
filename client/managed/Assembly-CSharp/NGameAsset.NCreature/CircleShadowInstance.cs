using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NCreature;

public class CircleShadowInstance : ILateUpdater
{
	public const string PrefabPath = "Resources/Vespa/Shadow/Shadow";

	public static PrefabGoAuto PrefabAuto = new PrefabGoAuto("Resources/Vespa/Shadow/Shadow");

	private CreatureInstance _owner;

	private RenderCache _renderCache;

	private bool _registerLateUpdater;

	private Transform _pivotTm;

	public CircleShadowInstance(CreatureInstance owner)
	{
		_owner = owner;
		RefreshShadow();
	}

	public void Close()
	{
		DestroyCircleShadow();
	}

	public void ChainUpdate()
	{
		RefreshShadow();
	}

	private void RefreshShadow()
	{
		if (SystemOptionData.Render_ShadowType == EShadowType.Circle && _renderCache == null)
		{
			CreateCircleShadow();
		}
		else if (SystemOptionData.Render_ShadowType != EShadowType.Circle && _renderCache != null)
		{
			DestroyCircleShadow();
		}
	}

	public void Visit(IVisitor visitor)
	{
		if (_renderCache != null)
		{
			_renderCache.Visit(visitor);
		}
	}

	void ILateUpdater.ChainLateUpdate()
	{
		SprkGraphicsOptions.UpdateCircleShadow(_pivotTm, _renderCache);
	}

	private void CreateCircleShadow()
	{
		float num = 0.5f;
		if (_owner != null && _owner.OpenData.GetStock().Size > 0.01f)
		{
			num = _owner.OpenData.GetStock().Size;
		}
		GameObject gameObject = PrefabAuto.Factory.Create(_owner.InstanceCache.Tm);
		gameObject.transform.localScale = Vector3.one * num;
		_renderCache = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<RenderCache>(gameObject);
		_renderCache.Initialize();
		if (!_registerLateUpdater)
		{
			GlobalUpdateBehaviour.AddUpdater(this);
			_registerLateUpdater = true;
			_pivotTm = Vespa_Common_Class_Extension_Method.Interface((ICreature)_owner).GetPivot(EPivotType.Hit);
			if (_pivotTm == null)
			{
				_pivotTm = Vespa_Common_Class_Extension_Method.Interface((ICreature)_owner).FindTransform("Bip001 Spine");
			}
		}
	}

	private void DestroyCircleShadow()
	{
		if (_renderCache != null)
		{
			_renderCache.transform.parent = null;
			VespaObject.DestroyGo(_renderCache);
			_renderCache = null;
		}
	}
}
