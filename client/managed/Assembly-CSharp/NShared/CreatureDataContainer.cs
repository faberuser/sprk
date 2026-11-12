using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CreatureDataContainer : JitDataContainerDefault<int, CreatureData>, ISnapshotMemberSerializer
{
	public const string TABLENAME = "CreatureTable";

	private JitDictionary<string, CreatureData> _dataByCode = new JitDictionary<string, CreatureData>();

	private JitDictionary<CreatureTagType, List<CreatureData>> _dataListByTagType = new JitDictionary<CreatureTagType, List<CreatureData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureTable");

	public CreatureDataContainer()
	{
		base.Packer.RegisterAndBind(_dataByCode);
		base.Packer.RegisterAndBind(_dataListByTagType);
	}

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		if (!int.TryParse(value, out var result))
		{
			return null;
		}
		return GetData(result);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (value is CreatureData creatureData)
		{
			return creatureData.GETKEY().ToString();
		}
		return null;
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CreatureData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.TagType != CreatureTagType.None && item.TagType != CreatureTagType.End)
			{
				_dataListByTagType.JsonLoad_AddToListValue(item.TagType, item);
			}
		}
		foreach (CreatureData item2 in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item2.CodeName != null && item2.CodeName.Length != 0)
			{
				if (_dataByCode.JsonLoad_Get().TryGetValue(item2.CodeName, out var value))
				{
					JitOption.JitLogWarning($"Duplicate Creature CodeName {item2.CodeName}: Index={item2.Index}, Old Index={value.Index}");
				}
				else
				{
					_dataByCode.JsonLoad_Get().Add(item2.CodeName, item2);
				}
			}
		}
	}

	public List<CreatureData> GetListByTagType(CreatureTagType creatureTag)
	{
		return _dataListByTagType.GetOrDefault(creatureTag);
	}

	public CreatureData GetDataByCode(string code)
	{
		if (_dataByCode.TryGetValue(code, out var value))
		{
			return value;
		}
		return null;
	}

	public List<CreatureData> GetOnlyHeroList()
	{
		List<CreatureData> list = new List<CreatureData>();
		foreach (CreatureData allDatum in GetAllData())
		{
			if (allDatum.StartHeroStar != 0 && (allDatum.OpenType == CreatureOpenType.Opened || allDatum.OpenType == CreatureOpenType.RecruitOnly || allDatum.OpenType == CreatureOpenType.Scenario))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public List<CreatureData> GetOpenedHeroList()
	{
		List<CreatureData> list = new List<CreatureData>();
		foreach (CreatureData allDatum in GetAllData())
		{
			if (allDatum.StartHeroStar != 0 && (allDatum.OpenType == CreatureOpenType.Opened || allDatum.OpenType == CreatureOpenType.CommingSoon))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public List<CreatureData> GetHeroListByOpenType(params CreatureOpenType[] openTypes)
	{
		List<CreatureData> list = new List<CreatureData>();
		foreach (CreatureData data in GetAllData())
		{
			if (data.StartHeroStar != 0 && Array.FindIndex(openTypes, (CreatureOpenType openType) => data.OpenType == openType) != -1)
			{
				list.Add(data);
			}
		}
		return list;
	}

	public List<CreatureData> GetListOf(Func<CreatureData, bool> predicate)
	{
		List<CreatureData> list = new List<CreatureData>();
		if (predicate == null)
		{
			return list;
		}
		foreach (CreatureData allDatum in GetAllData())
		{
			if (predicate(allDatum))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public CreatureData GetDataOf(Func<CreatureData, bool> predicate)
	{
		if (predicate == null)
		{
			return null;
		}
		foreach (CreatureData allDatum in GetAllData())
		{
			if (predicate(allDatum))
			{
				return allDatum;
			}
		}
		return null;
	}

	public CreatureRaceType GetRace(int creatureIndex)
	{
		return GetData(creatureIndex)?.Race ?? CreatureRaceType.None;
	}

	public string GetCode(int creatureIndex)
	{
		CreatureData data = GetData(creatureIndex);
		if (data == null)
		{
			return string.Empty;
		}
		return data.CodeName;
	}
}
