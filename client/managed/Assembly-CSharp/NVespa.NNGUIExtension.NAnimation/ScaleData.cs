using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class ScaleData : BaseData
{
	public Vector3 ScaleOffset;

	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		ScaleUpdater scaleUpdater = new ScaleUpdater();
		scaleUpdater.Setup(this, objs, value, isReverse);
		return scaleUpdater;
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
		ScaleUpdater scaleUpdater = new ScaleUpdater();
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			scaleUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			scaleUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			scaleUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			scaleUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return scaleUpdater;
	}

	public override void DrawInspectorParts()
	{
	}

	public override void DrawInspector()
	{
	}
}
