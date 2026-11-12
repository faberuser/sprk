using NGameAsset;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class PivotData
{
	public enum ETag
	{
		Position,
		DirectionReal,
		DirectionView,
		Root,
		AssetRoot,
		Hit
	}

	private RenderFacade _facade;

	public PivotData(RenderFacade facade)
	{
		_facade = facade;
	}

	public Transform FindTransform(EPivotType pivot)
	{
		switch (pivot)
		{
		case EPivotType.Position:
			return _facade.transform;
		case EPivotType.DirectionReal:
			return _facade.DirectionReal;
		case EPivotType.DirectionView:
			return _facade.DirectionView;
		default:
			if (!_facade.AssetCreatureAsync.IsValid)
			{
				return _facade.DirectionView;
			}
			if (pivot == EPivotType.Root || pivot == EPivotType.AssetRoot || pivot == EPivotType.Hit)
			{
				return _facade.AssetCreature.GetPivot(pivot);
			}
			return _facade.transform;
		}
	}

	public Transform FindTransform(string name, bool skipNullCaching = false)
	{
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			return _facade.transform;
		}
		EPivotType ePivotType = Vespa_String_Extension_Method.TOENUM(name, EPivotType.__NoPivot__);
		if (ePivotType != EPivotType.__NoPivot__)
		{
			return FindTransform(ePivotType);
		}
		if (!_facade.AssetCreatureAsync.IsValid)
		{
			return _facade.DirectionView;
		}
		Transform pivot = _facade.AssetCreature.GetPivot(name);
		if (pivot != null)
		{
			return pivot;
		}
		return _facade.AssetCreature.FindTransform(name, skipNullCaching);
	}
}
