using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NShared.NJit;
using NVespa;

namespace NShared;

public class CreatureStarGradeStatDataContainer : JitDataContainer<CreatureStarGradeStatData>
{
	private class DataForCache
	{
		public class EqualityComparer : IEqualityComparer<DataForCache>
		{
			public static EqualityComparer Default = new EqualityComparer();

			int IEqualityComparer<DataForCache>.GetHashCode(DataForCache obj)
			{
				return obj._hashCode;
			}

			bool IEqualityComparer<DataForCache>.Equals(DataForCache x, DataForCache y)
			{
				if (x == y)
				{
					return true;
				}
				if (x._dic.Count != y._dic.Count)
				{
					return false;
				}
				foreach (KeyValuePair<StatType, long> item in x._dic)
				{
					if (!y._dic.TryGetValue(item.Key, out var value))
					{
						return false;
					}
					if (item.Value != value)
					{
						return false;
					}
				}
				return true;
			}
		}

		private int _hashCode;

		private Dictionary<StatType, long> _dic = new Dictionary<StatType, long>();

		public DataForCache(Dictionary<StatType, long> dic)
		{
			_dic = dic;
			foreach (KeyValuePair<StatType, long> item in _dic)
			{
				_hashCode = (int)(_hashCode * 7199369 + item.Key);
				_hashCode = _hashCode * 7199369 + (int)item.Value;
			}
		}
	}

	private static class LoaderFromJsonData
	{
		public static void LoadFromJsonData(object jsonData, CreatureStarGradeStatDataContainer This)
		{
			if (jsonData == null)
			{
				string obj = "Failed to load table : " + This.PathDefine.ToString();
				VespaDebug.LogError(obj);
				return;
			}
			if (!(jsonData is List<object> list))
			{
				string obj2 = "Parsing table error : " + This.PathDefine.ToString();
				VespaDebug.LogError(obj2);
				return;
			}
			List<StatType> statTypeList = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList();
			Dictionary<DataForCache, int> cacheDic = new Dictionary<DataForCache, int>(DataForCache.EqualityComparer.Default);
			foreach (IDictionary item in list)
			{
				try
				{
					if (JsonMarshalerUsingReflection.Parse(typeof(CreatureStarGradeStatData), item) is CreatureStarGradeStatData creatureStarGradeStatData)
					{
						creatureStarGradeStatData.LookupTableIndex = ParseStatContainer(statTypeList, item, cacheDic, This._statLookupTable.JsonLoad_GetOrNew());
						CreatureStarGradeStatKey kEY = creatureStarGradeStatData.GETKEY();
						if (This.BaseDic.JsonLoad_Get().ContainsKey(kEY))
						{
							VespaDebug.LogError("[Jit Table] Duplicate Key!!!!");
							continue;
						}
						This.BaseDic.JsonLoad_Get().Add(creatureStarGradeStatData.GETKEY(), creatureStarGradeStatData);
						This.BaseList.JsonLoad_GetOrNew().Add(creatureStarGradeStatData);
					}
				}
				catch (Exception ex)
				{
					VespaDebug.LogError("Error while add data : " + ex);
					break;
				}
			}
		}

		private static int ParseStatContainer(List<StatType> statTypeList, IDictionary d, Dictionary<DataForCache, int> cacheDic, List<Dictionary<StatType, long>> statLookupTable)
		{
			Dictionary<StatType, long> dictionary = new Dictionary<StatType, long>();
			if (statTypeList != null)
			{
				for (int i = 0; i < statTypeList.Count; i++)
				{
					StatType key = statTypeList[i];
					string key2 = key.ToString();
					if (d.Contains(key2))
					{
						object obj = d[key2];
						long result;
						if (obj is long)
						{
							result = (long)obj;
						}
						else if (obj is double)
						{
							result = (long)(double)obj;
						}
						else if (!(obj is string) || !long.TryParse(obj as string, out result))
						{
							continue;
						}
						dictionary.Add(key, result);
					}
				}
			}
			DataForCache key3 = new DataForCache(dictionary);
			if (cacheDic.TryGetValue(key3, out var value))
			{
				return value;
			}
			value = statLookupTable.Count;
			statLookupTable.Add(dictionary);
			cacheDic.Add(key3, value);
			return value;
		}
	}

	public const string TABLENAME = "CreatureStarGradeStatTable";

	private JitDictionaryWithKey<CreatureStarGradeStatKey, CreatureStarGradeStatData> BaseDic = new JitDictionaryWithKey<CreatureStarGradeStatKey, CreatureStarGradeStatData>();

	private JitList<Dictionary<StatType, long>> _statLookupTable = new JitList<Dictionary<StatType, long>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureStarGradeStatTable");

	public CreatureStarGradeStatDataContainer()
	{
		base.Packer.RegisterAndBind(BaseDic);
		base.Packer.RegisterAndBind(_statLookupTable);
	}

	public CreatureStarGradeStatData GetData(CreatureStarGradeStatKey key)
	{
		if (BaseDic.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public Dictionary<StatType, long> GetStatLookup(int index)
	{
		if (index >= _statLookupTable.Count)
		{
			return null;
		}
		return _statLookupTable[index];
	}

	protected sealed override void LoadFromJsonData(object jsonData)
	{
		LoaderFromJsonData.LoadFromJsonData(jsonData, this);
	}
}
