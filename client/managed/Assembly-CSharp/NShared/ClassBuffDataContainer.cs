using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ClassBuffDataContainer : JitDataContainerDefault<int, ClassBuffData>
{
	public const string TABLENAME = "ClassBuffTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ClassBuffTable");

	public Dictionary<int, List<ClassBuffData>> GetClassBuffDataDicByCreatureType(CreatureTagType type)
	{
		Dictionary<int, List<ClassBuffData>> dictionary = new Dictionary<int, List<ClassBuffData>>();
		dictionary.Clear();
		foreach (ClassBuffData allDatum in GetAllData())
		{
			if (allDatum != null && allDatum.TagType == type)
			{
				if (dictionary.ContainsKey(allDatum.ClassBuffIndex))
				{
					dictionary[allDatum.ClassBuffIndex].Add(allDatum);
					continue;
				}
				dictionary.Add(allDatum.ClassBuffIndex, new List<ClassBuffData>());
				dictionary[allDatum.ClassBuffIndex].Clear();
				dictionary[allDatum.ClassBuffIndex].Add(allDatum);
			}
		}
		return dictionary;
	}

	public ClassBuffData GetClassBuffData(CreatureTagType type, int index, int level)
	{
		foreach (ClassBuffData classBuffData in GetClassBuffDataList(type, index))
		{
			if (classBuffData != null && classBuffData.ClassBuffLevel == level)
			{
				return classBuffData;
			}
		}
		return null;
	}

	public List<ClassBuffData> GetClassBuffDataList(CreatureTagType type, int index)
	{
		List<ClassBuffData> list = new List<ClassBuffData>();
		list.Clear();
		foreach (List<ClassBuffData> value in GetClassBuffDataDicByCreatureType(type).Values)
		{
			foreach (ClassBuffData item in value)
			{
				if (item != null && item.ClassBuffIndex == index)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public int GeMaxtClassBuffLevel(CreatureTagType type, int index)
	{
		List<ClassBuffData> classBuffDataList = GetClassBuffDataList(type, index);
		if (classBuffDataList.Count <= 0)
		{
			return 0;
		}
		return classBuffDataList.Count;
	}

	public static string GetStatValue(int skillIndex, byte usePercentage)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data == null)
		{
			return "";
		}
		OperationData operationDataUnsafe = data.GetOperationDataUnsafe(1);
		if (operationDataUnsafe == null)
		{
			return "";
		}
		if (operationDataUnsafe.Values == null)
		{
			return "0";
		}
		float result = 0f;
		if (float.TryParse(operationDataUnsafe.Values[0], out result))
		{
			return ((usePercentage == 1) ? (result / 10f) : result).ToString();
		}
		return "";
	}
}
