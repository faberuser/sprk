using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class RotationData : BaseData
{
	public Vector3 startAngle;

	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		RotationUpdater rotationUpdater = new RotationUpdater();
		rotationUpdater.Setup(this, objs, value, isReverse);
		return rotationUpdater;
	}

	public override IUpdater CreateUpdater(GameObject[] objs, CreateUpdaterStock stock)
	{
		if (!IsValid())
		{
			return null;
		}
		if (stock == null)
		{
			return null;
		}
		RotationUpdater rotationUpdater = new RotationUpdater();
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			rotationUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			rotationUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			rotationUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			rotationUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return rotationUpdater;
	}

	public override void DrawInspectorParts()
	{
	}

	public override void DrawInspector()
	{
	}
}
