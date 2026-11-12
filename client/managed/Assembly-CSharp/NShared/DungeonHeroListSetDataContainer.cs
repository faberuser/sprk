using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DungeonHeroListSetDataContainer : JitDataContainerDefault<DungeonHeroListSetKey, DungeonHeroListSetData>
{
	public const string TABLENAME = "DungeonHeroListSetTable";

	private JitDictionary<int, List<DungeonHeroListSetData>> _dicByGroup = new JitDictionary<int, List<DungeonHeroListSetData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("DungeonHeroListSetTable");

	public DungeonHeroListSetDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByGroup);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (DungeonHeroListSetData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByGroup.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
	}

	public IList<DungeonHeroListSetData> GetDatasByGroupIndex(int groupIndex)
	{
		return _dicByGroup.GetOrDefault(groupIndex);
	}

	public DungeonHeroListSetData GetData(int groupIndex, int heroIndex)
	{
		return GetData(new DungeonHeroListSetKey(groupIndex, heroIndex));
	}
}
