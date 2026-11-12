using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class UIGridAnimationPlayData : BaseAnimationPlayData
{
	public float ColumnDelay;

	public float RowDelay;

	public EShowOrderType ShowOrderType;

	public PositionData PositionData;

	public AlphaData AlphaData;

	public RotationData RotationData;

	public ScaleData ScaleData;

	public CustomData CustomData;

	public WidgetDepthData DepthData;

	public override bool IsValid()
	{
		if (PositionData != null && PositionData.IsValid())
		{
			return true;
		}
		if (AlphaData != null && AlphaData.IsValid())
		{
			return true;
		}
		if (RotationData != null && RotationData.IsValid())
		{
			return true;
		}
		if (ScaleData != null && ScaleData.IsValid())
		{
			return true;
		}
		if (DepthData != null && DepthData.IsValid())
		{
			return true;
		}
		if (CustomData != null && CustomData.IsValid())
		{
			return true;
		}
		return false;
	}

	public override List<IUpdater> GetUpdaterList(GameObject[] objs, CreateUpdaterStock stock)
	{
		if (!IsValid())
		{
			return null;
		}
		List<IUpdater> list = new List<IUpdater>();
		if (PositionData != null && PositionData.IsValid())
		{
			list.Add(PositionData.CreateUpdater(objs, stock));
		}
		if (AlphaData != null && AlphaData.IsValid())
		{
			list.Add(AlphaData.CreateUpdater(objs, stock));
		}
		if (RotationData != null && RotationData.IsValid())
		{
			list.Add(RotationData.CreateUpdater(objs, stock));
		}
		if (ScaleData != null && ScaleData.IsValid())
		{
			list.Add(ScaleData.CreateUpdater(objs, stock));
		}
		if (DepthData != null && DepthData.IsValid())
		{
			list.Add(DepthData.CreateUpdater(objs, stock));
		}
		if (CustomData != null && CustomData.IsValid())
		{
			list.Add(CustomData.CreateUpdater(objs, stock));
		}
		return list;
	}

	public override void DrawInspectorParts()
	{
	}
}
