using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class CustomData : BaseData
{
	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		CustomUpdater customUpdater = new CustomUpdater();
		customUpdater.Setup(this, objs, value, isReverse);
		return customUpdater;
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
		CustomUpdater customUpdater = new CustomUpdater();
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			customUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			customUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			customUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			customUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return customUpdater;
	}

	public override void DrawInspector()
	{
	}
}
