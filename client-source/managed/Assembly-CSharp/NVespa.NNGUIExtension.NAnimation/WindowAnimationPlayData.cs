using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class WindowAnimationPlayData : BaseAnimationPlayData
{
	[Serializable]
	public class DataGroup
	{
		public List<GameObject> GoList;

		public PositionData PositionData;

		public AlphaData AlphaData;

		public ScaleData ScaleData;

		public RotationData RotationData;

		public CustomData CustomData;

		public DataGroup()
		{
			GoList = new List<GameObject>();
			PositionData = new PositionData();
			AlphaData = new AlphaData();
			ScaleData = new ScaleData();
			RotationData = new RotationData();
			CustomData = new CustomData();
		}

		public void DrawInspector()
		{
		}

		public bool IsValid()
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(GoList))
			{
				return false;
			}
			if (PositionData != null && PositionData.IsValid())
			{
				return true;
			}
			if (AlphaData != null && AlphaData.IsValid())
			{
				return true;
			}
			if (ScaleData != null && ScaleData.IsValid())
			{
				return true;
			}
			if (RotationData != null && RotationData.IsValid())
			{
				return true;
			}
			return false;
		}

		public bool IsEnd()
		{
			return true;
		}

		public List<IUpdater> GetUpdaterList(CreateUpdaterStock stock)
		{
			if (!IsValid())
			{
				return null;
			}
			List<IUpdater> list = new List<IUpdater>();
			GameObject[] objs = GoList.ToArray();
			if (PositionData != null && PositionData.IsValid())
			{
				list.Add(PositionData.CreateUpdater(objs, stock));
			}
			if (AlphaData != null && AlphaData.IsValid())
			{
				list.Add(AlphaData.CreateUpdater(objs, stock));
			}
			if (ScaleData != null && ScaleData.IsValid())
			{
				list.Add(ScaleData.CreateUpdater(objs, stock));
			}
			if (RotationData != null && RotationData.IsValid())
			{
				list.Add(RotationData.CreateUpdater(objs, stock));
			}
			if (CustomData != null && CustomData.IsValid())
			{
				list.Add(CustomData.CreateUpdater(objs, stock));
			}
			return list;
		}
	}

	public List<DataGroup> ListDataGroup = new List<DataGroup>();

	public override bool IsValid()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(ListDataGroup))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ListDataGroup); i++)
		{
			if (ListDataGroup[i].IsValid())
			{
				return true;
			}
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
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ListDataGroup); i++)
		{
			DataGroup dataGroup = ListDataGroup[i];
			if (dataGroup != null && dataGroup.IsValid())
			{
				list.AddRange(dataGroup.GetUpdaterList(stock));
			}
		}
		return list;
	}

	public override void DrawInspectorParts()
	{
	}
}
