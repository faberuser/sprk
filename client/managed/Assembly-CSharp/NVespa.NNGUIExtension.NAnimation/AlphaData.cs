using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class AlphaData : BaseData
{
	public bool AllowAlphaZero;

	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		AlphaUpdater alphaUpdater = new AlphaUpdater();
		alphaUpdater.Setup(this, objs, value, isReverse);
		return alphaUpdater;
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
		AlphaUpdater alphaUpdater = new AlphaUpdater();
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			alphaUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			alphaUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			alphaUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			alphaUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return alphaUpdater;
	}

	public override void DrawInspector()
	{
	}
}
