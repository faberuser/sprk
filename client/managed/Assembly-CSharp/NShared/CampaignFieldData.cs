using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[DataElement(Type = typeof(BattleAreaElementData), Count = 2)]
public class CampaignFieldData : IDataKeySelector<string>, IDataElement<BattleAreaElementData>
{
	private class __InternalData
	{
		public static BattleAreaElementData Empty = new BattleAreaElementData();

		private List<BattleAreaElementData> _list = new List<BattleAreaElementData>();

		public int GetBattleAreaGroupIndex(int index)
		{
			return GetElement(index, reserve: false).BattleAreaGroupIndex;
		}

		public void SetBattleAreaGroupIndex(int index, int value)
		{
			if (value != Empty.BattleAreaGroupIndex)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.BattleAreaGroupIndex = value;
				}
			}
		}

		public int GetAreaBottomM(int index)
		{
			return GetElement(index, reserve: false).AreaBottomM;
		}

		public void SetAreaBottomM(int index, int value)
		{
			if (value != Empty.AreaBottomM)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.AreaBottomM = value;
				}
			}
		}

		public int GetAreaTopM(int index)
		{
			return GetElement(index, reserve: false).AreaTopM;
		}

		public void SetAreaTopM(int index, int value)
		{
			if (value != Empty.AreaTopM)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.AreaTopM = value;
				}
			}
		}

		public int GetAreaLeftM(int index)
		{
			return GetElement(index, reserve: false).AreaLeftM;
		}

		public void SetAreaLeftM(int index, int value)
		{
			if (value != Empty.AreaLeftM)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.AreaLeftM = value;
				}
			}
		}

		public int GetAreaRightM(int index)
		{
			return GetElement(index, reserve: false).AreaRightM;
		}

		public void SetAreaRightM(int index, int value)
		{
			if (value != Empty.AreaRightM)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.AreaRightM = value;
				}
			}
		}

		public int GetCenterBiasM(int index)
		{
			return GetElement(index, reserve: false).CenterBiasM;
		}

		public void SetCenterBiasM(int index, int value)
		{
			if (value != Empty.CenterBiasM)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.CenterBiasM = value;
				}
			}
		}

		public int GetAngle(int index)
		{
			return GetElement(index, reserve: false).Angle;
		}

		public void SetAngle(int index, int value)
		{
			if (value != Empty.Angle)
			{
				BattleAreaElementData element = GetElement(index, reserve: true);
				if (element != Empty)
				{
					element.Angle = value;
				}
			}
		}

		public BattleAreaElementData GetElement(int index, bool reserve)
		{
			if (index < 0 || index > 2)
			{
				return Empty;
			}
			if (reserve)
			{
				while (_list.Count <= index)
				{
					_list.Add(Empty);
				}
				if (_list[index] == Empty)
				{
					_list[index] = new BattleAreaElementData();
				}
			}
			if (index < _list.Count)
			{
				return _list[index];
			}
			return Empty;
		}
	}

	public const int MaxElementCount = 2;

	private __InternalData _internal = new __InternalData();

	[Key(0)]
	public string Name { get; set; }

	string IDataKeySelector<string>.Key => Name;

	[Key(1)]
	public int BattleAreaGroupIndex1
	{
		get
		{
			return _internal.GetBattleAreaGroupIndex(1);
		}
		set
		{
			_internal.SetBattleAreaGroupIndex(1, value);
		}
	}

	[Key(2)]
	public int AreaBottomM1
	{
		get
		{
			return _internal.GetAreaBottomM(1);
		}
		set
		{
			_internal.SetAreaBottomM(1, value);
		}
	}

	[Key(3)]
	public int AreaTopM1
	{
		get
		{
			return _internal.GetAreaTopM(1);
		}
		set
		{
			_internal.SetAreaTopM(1, value);
		}
	}

	[Key(4)]
	public int AreaLeftM1
	{
		get
		{
			return _internal.GetAreaLeftM(1);
		}
		set
		{
			_internal.SetAreaLeftM(1, value);
		}
	}

	[Key(5)]
	public int AreaRightM1
	{
		get
		{
			return _internal.GetAreaRightM(1);
		}
		set
		{
			_internal.SetAreaRightM(1, value);
		}
	}

	[Key(6)]
	public int CenterBiasM1
	{
		get
		{
			return _internal.GetCenterBiasM(1);
		}
		set
		{
			_internal.SetCenterBiasM(1, value);
		}
	}

	[Key(7)]
	public int Angle1
	{
		get
		{
			return _internal.GetAngle(1);
		}
		set
		{
			_internal.SetAngle(1, value);
		}
	}

	[Key(8)]
	public int BattleAreaGroupIndex2
	{
		get
		{
			return _internal.GetBattleAreaGroupIndex(2);
		}
		set
		{
			_internal.SetBattleAreaGroupIndex(2, value);
		}
	}

	[Key(9)]
	public int AreaBottomM2
	{
		get
		{
			return _internal.GetAreaBottomM(2);
		}
		set
		{
			_internal.SetAreaBottomM(2, value);
		}
	}

	[Key(10)]
	public int AreaTopM2
	{
		get
		{
			return _internal.GetAreaTopM(2);
		}
		set
		{
			_internal.SetAreaTopM(2, value);
		}
	}

	[Key(11)]
	public int AreaLeftM2
	{
		get
		{
			return _internal.GetAreaLeftM(2);
		}
		set
		{
			_internal.SetAreaLeftM(2, value);
		}
	}

	[Key(12)]
	public int AreaRightM2
	{
		get
		{
			return _internal.GetAreaRightM(2);
		}
		set
		{
			_internal.SetAreaRightM(2, value);
		}
	}

	[Key(13)]
	public int CenterBiasM2
	{
		get
		{
			return _internal.GetCenterBiasM(2);
		}
		set
		{
			_internal.SetCenterBiasM(2, value);
		}
	}

	[Key(14)]
	public int Angle2
	{
		get
		{
			return _internal.GetAngle(2);
		}
		set
		{
			_internal.SetAngle(2, value);
		}
	}

	public List<BattleAreaElementData> GetBattleAreaDataList()
	{
		List<BattleAreaElementData> list = new List<BattleAreaElementData>();
		list.Add(new BattleAreaElementData(BattleAreaGroupIndex1, AreaBottomM1, AreaTopM1, AreaLeftM1, AreaRightM1, CenterBiasM1, Angle1));
		if (BattleAreaGroupIndex2 > 0)
		{
			list.Add(new BattleAreaElementData(BattleAreaGroupIndex2, AreaBottomM2, AreaTopM2, AreaLeftM2, AreaRightM2, CenterBiasM2, Angle2));
		}
		return list;
	}

	public BattleAreaElementData GetBattleAreaData(int groupIndex)
	{
		List<BattleAreaElementData> battleAreaDataList = GetBattleAreaDataList();
		if (battleAreaDataList == null || battleAreaDataList.Count == 0)
		{
			return null;
		}
		BattleAreaElementData result = null;
		for (int i = 0; i < battleAreaDataList.Count; i++)
		{
			BattleAreaElementData battleAreaElementData = battleAreaDataList[i];
			if (battleAreaElementData != null && battleAreaElementData.BattleAreaGroupIndex == groupIndex)
			{
				result = battleAreaElementData;
				break;
			}
		}
		return result;
	}

	public int GetGropCount()
	{
		if (BattleAreaGroupIndex2 == 0)
		{
			return 1;
		}
		return 2;
	}

	public List<BattleAreaElementData> GetElementList()
	{
		List<BattleAreaElementData> list = new List<BattleAreaElementData>();
		if (((DateElementField)_internal.GetElement(1, reserve: false)).IsValid())
		{
			BattleAreaElementData item = new BattleAreaElementData
			{
				BattleAreaGroupIndex = BattleAreaGroupIndex1,
				AreaBottomM = AreaBottomM1,
				AreaTopM = AreaTopM1,
				AreaLeftM = AreaLeftM1,
				AreaRightM = AreaRightM1,
				CenterBiasM = CenterBiasM1,
				Angle = Angle1
			};
			list.Add(item);
		}
		if (((DateElementField)_internal.GetElement(2, reserve: false)).IsValid())
		{
			BattleAreaElementData item2 = new BattleAreaElementData
			{
				BattleAreaGroupIndex = BattleAreaGroupIndex2,
				AreaBottomM = AreaBottomM2,
				AreaTopM = AreaTopM2,
				AreaLeftM = AreaLeftM2,
				AreaRightM = AreaRightM2,
				CenterBiasM = CenterBiasM2,
				Angle = Angle2
			};
			list.Add(item2);
		}
		return list;
	}
}
