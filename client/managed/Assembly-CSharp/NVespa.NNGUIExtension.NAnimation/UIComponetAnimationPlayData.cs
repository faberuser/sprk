using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class UIComponetAnimationPlayData : BaseAnimationPlayData
{
	[Serializable]
	public class DataGroup
	{
		public string GroupName;

		public List<GameObject> GoList;

		public PositionData PositionData;

		public AlphaData AlphaData;

		public ScaleData ScaleData;

		public RotationData RotationData;

		public DataGroup()
		{
			GoList = new List<GameObject>();
			GroupName = "";
			PositionData = new PositionData();
			SetData(PositionData);
			AlphaData = new AlphaData();
			SetData(AlphaData);
			ScaleData = new ScaleData();
			SetData(ScaleData);
			RotationData = new RotationData();
			SetData(RotationData);
		}

		private void SetData(BaseData data)
		{
			data.IsEnableChangeUpdateType = false;
			data.UpdateValueType = BaseData.EUpdateValueType.Time;
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
