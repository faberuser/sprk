using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class CostumeGroupDataContainer : JitDataContainerDefault<int, CostumeGroupData>
{
	public const string TABLENAME = "CostumeGroupTable";

	private Dictionary<string, int> _temp_selectItemGroupDic;

	public override TablePathDefine PathDefine => new TablePathDefine("CostumeGroupTable");

	public CostumeGroupData GetDataByGroupName(string group)
	{
		return GetAllData().First((CostumeGroupData groupData) => groupData.GroupName == group);
	}

	public IEnumerable<string> GetSelectItemGroupList(string[] itemCategories)
	{
		return (from data in GetAllData()
			where itemCategories.Contains(data.GroupName)
			select data.SelectItemGroup).Distinct();
	}

	public IEnumerable<string> GetSelectItemGroupList(int[] itemIndices)
	{
		IEnumerable<string> groupNames = from costumeData in GameRoot.Instance.CostumeDataContainer.GetAllData()
			where TypeIterator.Contains(itemIndices, costumeData.CostumeIndex)
			select costumeData.Group;
		return (from groupData in GetAllData()
			where groupNames.Contains(groupData.GroupName)
			select groupData.SelectItemGroup).Distinct();
	}

	public int GetSelectItemGroupKey(string selectItemGroup)
	{
		if (_temp_selectItemGroupDic == null)
		{
			_temp_selectItemGroupDic = new Dictionary<string, int>();
		}
		if (!_temp_selectItemGroupDic.ContainsKey(selectItemGroup))
		{
			_temp_selectItemGroupDic.Add(selectItemGroup, _temp_selectItemGroupDic.Count + 1);
		}
		return _temp_selectItemGroupDic[selectItemGroup];
	}

	public CostumeGroupData GetNewestData(EHeroCostumeType costumeType = EHeroCostumeType.None)
	{
		if (costumeType == EHeroCostumeType.None)
		{
			return GetAllData().Last();
		}
		return GetAllData().LastOrDefault((CostumeGroupData groupData) => groupData.CostumeType == costumeType);
	}
}
