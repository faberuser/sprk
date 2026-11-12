using System.Collections.Generic;

namespace NShared.NContentsDefine;

[JsonSerializable]
public class SoulWeaponLimitBreak : ContentsDefine.IDefine
{
	[JsonSerializable]
	public class LimitBreakData
	{
		public int DetailIndex { get; set; }

		public int Star { get; set; }

		public int SuccessRatio { get; set; }

		public int FailBonusRatio { get; set; }

		public int GoldCount { get; set; }

		public int WeaponUniqueCount { get; set; }

		public int SoulStoneCount { get; set; }

		public int TransStoneIndex { get; set; }

		public int TransStoneCount { get; set; }
	}

	private struct MapKey
	{
		public class Comparer : IEqualityComparer<MapKey>
		{
			bool IEqualityComparer<MapKey>.Equals(MapKey x, MapKey y)
			{
				if (x.DetailIndex == y.DetailIndex)
				{
					return x.Star == y.Star;
				}
				return false;
			}

			int IEqualityComparer<MapKey>.GetHashCode(MapKey obj)
			{
				int num = 7199369;
				return obj.DetailIndex * num + obj.Star;
			}
		}

		public int DetailIndex;

		public int Star;

		public MapKey(int detail, int star)
		{
			DetailIndex = detail;
			Star = star;
		}
	}

	private class InnerData
	{
		public Dictionary<MapKey, LimitBreakData> DataMap { get; private set; }

		public Dictionary<int, int> MaxStarByDetailIndex { get; private set; }

		public InnerData(List<LimitBreakData> datas)
		{
			DataMap = new Dictionary<MapKey, LimitBreakData>(new MapKey.Comparer());
			MaxStarByDetailIndex = new Dictionary<int, int>();
			foreach (LimitBreakData data in datas)
			{
				DataMap.Add(new MapKey(data.DetailIndex, data.Star), data);
			}
			foreach (LimitBreakData data2 in datas)
			{
				if (!MaxStarByDetailIndex.TryGetValue(data2.DetailIndex, out var value))
				{
					value = 0;
				}
				if (value < data2.Star)
				{
					MaxStarByDetailIndex[data2.DetailIndex] = data2.Star;
				}
			}
		}

		public LimitBreakData GetData(int detailIndex, int star)
		{
			if (DataMap.TryGetValue(new MapKey(detailIndex, star), out var value))
			{
				return value;
			}
			return null;
		}

		public int GetMaxStar(int detailIndex)
		{
			if (MaxStarByDetailIndex.TryGetValue(detailIndex, out var value))
			{
				return value;
			}
			return 0;
		}
	}

	private static object _sync = new object();

	private InnerData _inner;

	public List<LimitBreakData> Datas { get; set; }

	public LimitBreakData GetAfterDataByCurrent(EquipItemInfo targetInfo)
	{
		if (targetInfo != null && targetInfo.EquipItemData != null)
		{
			return GetDataByResultStar(targetInfo.EquipItemData.DetailIndex, targetInfo.Star + 1);
		}
		return null;
	}

	public LimitBreakData GetDataByResultStar(int detailIndex, int star)
	{
		InitInner();
		_inner.DataMap.TryGetValue(new MapKey(detailIndex, star), out var value);
		return value;
	}

	public bool TryGetMaxStar(int detailIndex, out int maxStar)
	{
		InitInner();
		return _inner.MaxStarByDetailIndex.TryGetValue(detailIndex, out maxStar);
	}

	private void InitInner()
	{
		if (_inner != null)
		{
			return;
		}
		lock (_sync)
		{
			if (_inner == null)
			{
				_inner = new InnerData(Datas);
			}
		}
	}

	public static int ExceptionalMaxStar(int star)
	{
		if (star <= 5)
		{
			return star;
		}
		return 5;
	}
}
