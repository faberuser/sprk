using NGame2.NRender;
using NGameAsset;
using NGameAsset.NCreature;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class HudOffset
{
	public const string PivotName = "Bip001 Head";

	public readonly Bounds ShapeBounds;

	public readonly RenderFacade RenderFacade;

	private bool _isInit;

	private Transform _creatureRoot;

	private Transform _pivot;

	public bool DynamicHub { get; private set; }

	public float HudOffetY { get; private set; }

	public HudOffset(Bounds shapeBounds, RenderFacade renderFacade)
	{
		ShapeBounds = shapeBounds;
		RenderFacade = renderFacade;
	}

	public void SetDynamicHub(bool isDynamic)
	{
		DynamicHub = isDynamic;
	}

	public void SetHudOffsetY(float offsetY)
	{
		HudOffetY = offsetY;
	}

	public float GetY()
	{
		if (DynamicHub)
		{
			if (!_isInit)
			{
				_isInit = InitPivot();
			}
			return GetDynamicY();
		}
		return GetDefaultY();
	}

	private float GetDefaultY()
	{
		return ShapeBounds.size.y + 0.1f;
	}

	private float GetDynamicY()
	{
		if (!IsValidPivot())
		{
			return GetDefaultY();
		}
		return (_pivot.position - _creatureRoot.position).y + HudOffetY;
	}

	private bool IsInitializeDone()
	{
		return _creatureRoot != null;
	}

	private bool IsValidPivot()
	{
		return _pivot != null;
	}

	private bool InitPivot()
	{
		if (RenderFacade == null || RenderFacade.AssetCreature == null)
		{
			return false;
		}
		if (!(RenderFacade.AssetCreature is CreatureInstance creatureInstance))
		{
			return false;
		}
		_creatureRoot = Vespa_Common_Class_Extension_Method.Interface((ICreature)creatureInstance).GetPivot(EPivotType.AssetRoot);
		_pivot = Vespa_Common_Class_Extension_Method.Interface((ICreature)creatureInstance).FindTransform("Bip001 Head");
		return true;
	}
}
