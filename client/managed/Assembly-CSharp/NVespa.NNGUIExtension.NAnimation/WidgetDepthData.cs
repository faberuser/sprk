using System;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class WidgetDepthData : BaseData
{
	public int DepthStep;

	public int DepthRange;

	private float _depthValue;

	public int GetDepthValue(float weight)
	{
		return (int)(weight * (float)DepthStep * (float)DepthRange);
	}

	public override IUpdater CreateUpdater(GameObject[] objs, float value, bool isReverse)
	{
		if (!IsValid())
		{
			return null;
		}
		WidgetDepthUpdater widgetDepthUpdater = new WidgetDepthUpdater();
		widgetDepthUpdater.Setup(this, objs, value, isReverse);
		return widgetDepthUpdater;
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
		WidgetDepthUpdater widgetDepthUpdater = new WidgetDepthUpdater();
		switch (UpdateValueType)
		{
		case EUpdateValueType.Time:
			widgetDepthUpdater.Setup(this, objs, stock.Delay, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewVertical:
			widgetDepthUpdater.Setup(this, objs, stock.PosY, stock.IsReverse);
			break;
		case EUpdateValueType.ScrollViewHorizontal:
			widgetDepthUpdater.Setup(this, objs, stock.PosX, stock.IsReverse);
			break;
		default:
			widgetDepthUpdater.Setup(this, objs, 0f, isReverse: false);
			break;
		}
		return widgetDepthUpdater;
	}

	public override void DrawInspectorParts()
	{
	}

	public override void DrawInspector()
	{
	}
}
