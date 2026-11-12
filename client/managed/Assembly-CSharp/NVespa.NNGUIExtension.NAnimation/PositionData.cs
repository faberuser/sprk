using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class PositionData : BaseData
{
	public Vector3 PositionOffset;

	public bool IsWorldPosition;

	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		PositionUpdater positionUpdater = new PositionUpdater();
		positionUpdater.Setup(this, objs, value, isReverse);
		return positionUpdater;
	}

	public override IUpdater CreateUpdater(GameObject[] objs, CreateUpdaterStock stock)
	{
		if (!IsValid())
		{
			return null;
		}
		PositionUpdater positionUpdater = new PositionUpdater();
		if (stock == null)
		{
			positionUpdater.Setup(this, objs, 0f, isReverse: false);
			return positionUpdater;
		}
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			positionUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			positionUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			positionUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			positionUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return positionUpdater;
	}

	public override void DrawInspectorParts()
	{
	}

	public override void DrawInspector()
	{
	}
}
